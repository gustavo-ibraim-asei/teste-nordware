# Code Review - Análise de Código Problemático

## Código Original

```csharp
public class OrderService
{
    public static SqlConnection conn = new SqlConnection("Server=.;Database=Orders;");
    
    public void CreateOrder(int customerId, List<int> productIds)
    {
        try
        {
            conn.Open();
            var cmd = new SqlCommand("INSERT INTO Orders VALUES (" + customerId + ")", conn);
            cmd.ExecuteNonQuery();
            
            var orderId = 0;
            var cmd2 = new SqlCommand("SELECT MAX(Id) FROM Orders", conn);
            orderId = (int)cmd2.ExecuteScalar();
            
            foreach(var p in productIds)
            {
                Thread.Sleep(100);
                var cmd3 = new SqlCommand("INSERT INTO OrderItems VALUES (" + orderId + "," + p + ")", conn);
                cmd3.ExecuteNonQuery();
            }
            
            conn.Close();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
```

## Problemas Identificados

### 1. SQL Injection

**Problema:**
A concatenação direta de valores nas queries SQL permite SQL Injection, uma vulnerabilidade crítica de segurança.

```csharp
var cmd = new SqlCommand("INSERT INTO Orders VALUES (" + customerId + ")", conn);
var cmd3 = new SqlCommand("INSERT INTO OrderItems VALUES (" + orderId + "," + p + ")", conn);
```

**Exemplo de ataque:**
Se `customerId` for `"1); DROP TABLE Orders; --"`, o SQL executado seria:
```sql
INSERT INTO Orders VALUES (1); DROP TABLE Orders; --)
```

**Solução:**
Usar parâmetros SQL para evitar SQL Injection:
```csharp
var cmd = new SqlCommand("INSERT INTO Orders (CustomerId) VALUES (@CustomerId)", conn);
cmd.Parameters.AddWithValue("@CustomerId", customerId);
```

---

### 2. Connection String Hardcoded

**Problema:**
A connection string está fixa no código, dificultando deploy em diferentes ambientes e expondo credenciais.

```csharp
public static SqlConnection conn = new SqlConnection("Server=.;Database=Orders;");
```

**Solução:**
Configurar a connection string via `appsettings.json` e usar Dependency Injection:
```csharp
public class OrderService
{
    private readonly string _connectionString;
    
    public OrderService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string não configurada");
    }
}
```

---

### 3. Thread.Sleep em Loop

**Problema:**
O `Thread.Sleep(100)` bloqueia a thread por 100ms a cada iteração, degradando significativamente a performance. Para 100 produtos, isso adiciona 10 segundos de espera desnecessária.

```csharp
foreach(var p in productIds)
{
    Thread.Sleep(100);  // Bloqueia thread desnecessariamente
    var cmd3 = new SqlCommand("INSERT INTO OrderItems VALUES (" + orderId + "," + p + ")", conn);
    cmd3.ExecuteNonQuery();
}
```

**Solução:**
Remover o `Thread.Sleep`. Se houver necessidade de controle de concorrência, usar transações adequadas ou implementar rate limiting apropriado.

---

### 4. Falta de Operações em Bulk

**Problema:**
Inserir itens um por um em um loop resulta em múltiplas round-trips ao banco de dados, aumentando latência e carga desnecessariamente.

**Solução:**
Usar bulk insert ou Table-Valued Parameters (TVP) para inserir todos os itens de uma vez:
```csharp
// Usando Table-Valued Parameter
var itemsTable = new DataTable();
itemsTable.Columns.Add("OrderId", typeof(int));
itemsTable.Columns.Add("ProductId", typeof(int));

foreach (var productId in productIds)
{
    var row = itemsTable.NewRow();
    row["OrderId"] = orderId;
    row["ProductId"] = productId;
    itemsTable.Rows.Add(row);
}

var cmd = new SqlCommand("INSERT INTO OrderItems SELECT OrderId, ProductId FROM @Items", conn);
var param = cmd.Parameters.AddWithValue("@Items", itemsTable);
param.SqlDbType = SqlDbType.Structured;
param.TypeName = "dbo.OrderItemType";
cmd.ExecuteNonQuery();
```

---

### 5. Connection Estática Compartilhada

**Problema:**
A conexão estática é compartilhada entre todas as threads/requests, causando problemas de concorrência, race conditions e possíveis deadlocks.

```csharp
public static SqlConnection conn = new SqlConnection(...);
```

**Solução:**
Criar uma nova conexão por operação e usar `using` para garantir o dispose adequado:
```csharp
public async Task CreateOrderAsync(int customerId, List<int> productIds)
{
    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();
    // ... operações
}
```

---

### 6. Falta de Transações

**Problema:**
As operações não estão dentro de uma transação, o que pode resultar em dados inconsistentes. Se falhar ao inserir um item, o pedido já foi criado, deixando dados órfãos no banco.

**Solução:**
Envolver todas as operações em uma transação para garantir atomicidade:
```csharp
await using var transaction = await connection.BeginTransactionAsync();
try
{
    // Inserir pedido
    // Inserir itens
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

---

### 7. Race Condition com MAX(Id)

**Problema:**
Usar `SELECT MAX(Id)` para obter o ID do pedido criado pode resultar em race condition em cenários de alta concorrência, onde múltiplos pedidos podem receber o mesmo ID.

```csharp
var cmd2 = new SqlCommand("SELECT MAX(Id) FROM Orders", conn);
orderId = (int)cmd2.ExecuteScalar();
```

**Solução:**
Usar `OUTPUT INSERTED.Id` para obter o ID gerado diretamente na inserção:
```csharp
var cmd = new SqlCommand(@"
    INSERT INTO Orders (CustomerId, CreatedAt)
    OUTPUT INSERTED.Id
    VALUES (@CustomerId, @CreatedAt)", conn);
cmd.Parameters.AddWithValue("@CustomerId", customerId);
cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
orderId = (int)await cmd.ExecuteScalarAsync();
```

---

### 8. Falta de Async/Await

**Problema:**
O método é síncrono, bloqueando threads desnecessariamente e reduzindo a capacidade de processamento concorrente da aplicação.

**Solução:**
Converter para métodos assíncronos:
```csharp
public async Task<int> CreateOrderAsync(int customerId, List<int> productIds, CancellationToken cancellationToken = default)
{
    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);
    // ... operações assíncronas
}
```

---

### 9. Tratamento de Exceções Genérico

**Problema:**
O tratamento de exceções é genérico e apenas loga no console, perdendo informações importantes e não diferenciando tipos de erro.

```csharp
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}
```

**Solução:**
Usar logging estruturado e tratar exceções específicas:
```csharp
catch (SqlException ex)
{
    _logger.LogError(ex, "Erro SQL ao criar pedido para cliente {CustomerId}", customerId);
    throw new InvalidOperationException($"Erro ao criar pedido: {ex.Message}", ex);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erro inesperado ao criar pedido para cliente {CustomerId}", customerId);
    throw;
}
```

---

### 10. Falta de Dispose/Using

**Problema:**
A conexão não é fechada adequadamente em caso de exceção, podendo resultar em conexões abertas e esgotamento do pool de conexões.

**Solução:**
Usar `using` statements para garantir dispose automático:
```csharp
await using var connection = new SqlConnection(_connectionString);
await using var transaction = await connection.BeginTransactionAsync();
// Recursos são automaticamente liberados ao sair do escopo
```

---

### 11. Falta de Validações

**Problema:**
Não há validação de entrada, permitindo que dados inválidos sejam processados e resultem em erros em runtime.

**Solução:**
Adicionar validações no início do método:
```csharp
if (customerId <= 0)
    throw new ArgumentException("CustomerId deve ser maior que zero", nameof(customerId));

if (productIds == null || !productIds.Any())
    throw new ArgumentException("Pedido deve conter pelo menos um produto", nameof(productIds));
```

---

### 12. Violação de Princípios SOLID

**Problema:**
A classe mistura responsabilidades de acesso a dados e lógica de negócio, violando o Single Responsibility Principle e dificultando testes e manutenção.

**Solução:**
Separar responsabilidades usando Repository Pattern e Dependency Injection:
```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    
    public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
    }
}
```

---

### 13. Falta de Logging Estruturado

**Problema:**
O uso de `Console.WriteLine` não é adequado para produção, não fornece contexto suficiente e não diferencia níveis de log.

**Solução:**
Implementar logging estruturado com bibliotecas como Serilog ou NLog:
```csharp
_logger.LogInformation("Criando pedido para cliente {CustomerId} com {ItemCount} itens", 
    customerId, productIds.Count);
```

---

## Código Refatorado

```csharp
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class OrderService
{
    private readonly string _connectionString;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IConfiguration configuration, ILogger<OrderService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string não configurada");
        _logger = logger;
    }

    public async Task<int> CreateOrderAsync(int customerId, List<int> productIds, CancellationToken cancellationToken = default)
    {
        // Validações
        if (customerId <= 0)
            throw new ArgumentException("CustomerId deve ser maior que zero", nameof(customerId));
        
        if (productIds == null || !productIds.Any())
            throw new ArgumentException("Pedido deve conter pelo menos um produto", nameof(productIds));

        // Criar conexão (não estática, gerenciada por using)
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Transação explícita para garantir atomicidade
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            int orderId;

            // Inserir pedido com OUTPUT para obter ID gerado (evita race condition)
            const string insertOrderSql = @"
                INSERT INTO Orders (CustomerId, CreatedAt, Status)
                OUTPUT INSERTED.Id
                VALUES (@CustomerId, @CreatedAt, @Status)";

            await using var orderCmd = new SqlCommand(insertOrderSql, connection, transaction);
            orderCmd.Parameters.AddWithValue("@CustomerId", customerId);
            orderCmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            orderCmd.Parameters.AddWithValue("@Status", 1); // Pending

            orderId = (int)await orderCmd.ExecuteScalarAsync(cancellationToken);

            // Bulk insert de itens usando Table-Valued Parameter (TVP)
            var itemsTable = new DataTable();
            itemsTable.Columns.Add("OrderId", typeof(int));
            itemsTable.Columns.Add("ProductId", typeof(int));

            foreach (var productId in productIds)
            {
                var row = itemsTable.NewRow();
                row["OrderId"] = orderId;
                row["ProductId"] = productId;
                itemsTable.Rows.Add(row);
            }

            const string insertItemsSql = @"
                INSERT INTO OrderItems (OrderId, ProductId)
                SELECT OrderId, ProductId
                FROM @Items";

            await using var itemsCmd = new SqlCommand(insertItemsSql, connection, transaction);
            var itemsParam = itemsCmd.Parameters.AddWithValue("@Items", itemsTable);
            itemsParam.SqlDbType = SqlDbType.Structured;
            itemsParam.TypeName = "dbo.OrderItemType"; // Tipo de tabela no SQL Server

            await itemsCmd.ExecuteNonQueryAsync(cancellationToken);

            // Commit da transação
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Pedido {OrderId} criado com sucesso para cliente {CustomerId} com {ItemCount} itens",
                orderId, customerId, productIds.Count);

            return orderId;
        }
        catch (SqlException ex)
        {
            // Rollback em caso de erro SQL
            await transaction.RollbackAsync(cancellationToken);
            
            _logger.LogError(ex, 
                "Erro SQL ao criar pedido para cliente {CustomerId}. Código de erro: {ErrorNumber}",
                customerId, ex.Number);
            
            throw new InvalidOperationException($"Erro ao criar pedido: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            // Rollback em caso de qualquer outro erro
            await transaction.RollbackAsync(cancellationToken);
            
            _logger.LogError(ex, "Erro inesperado ao criar pedido para cliente {CustomerId}", customerId);
            throw;
        }
        // Connection e Transaction são automaticamente fechadas pelo using
    }
}
```

## Melhorias Aplicadas

1. **SQL Injection**: Eliminado usando parâmetros SQL (`@CustomerId`, `@ProductId`)
2. **Connection String**: Configurada via `IConfiguration` e Dependency Injection
3. **Thread.Sleep**: Removido completamente
4. **Bulk Operations**: Implementado usando Table-Valued Parameters (TVP) para inserir todos os itens de uma vez
5. **Connection**: Criada por método (não estática), gerenciada com `using`
6. **Transações**: Explícitas com `BeginTransaction`, `Commit` e `Rollback`
7. **Race Condition**: Resolvido usando `OUTPUT INSERTED.Id` ao invés de `MAX(Id)`
8. **Async/Await**: Implementado corretamente em todos os métodos
9. **Dispose**: Garantido com `using` statements
10. **Logging**: Estruturado com `ILogger` e níveis apropriados
11. **Validações**: Implementadas no início do método
12. **Tratamento de Exceções**: Específico para `SqlException` e genérico para outras
13. **SOLID**: Preparado para uso com Dependency Injection e separação de responsabilidades

## Observações Adicionais

### Performance

O código refatorado é significativamente mais rápido:
- **Código original**: Para 100 produtos, aproximadamente 10 segundos (100ms × 100 itens)
- **Código refatorado**: Para 100 produtos, aproximadamente 50-150ms (bulk insert)

### Segurança

O uso de parâmetros SQL elimina completamente o risco de SQL Injection, uma das vulnerabilidades mais críticas em aplicações web.

### Manutenibilidade

A separação de responsabilidades e uso de Dependency Injection facilitam testes unitários e manutenção futura do código.
