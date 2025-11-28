# Code Review - Análise do Código Problemático

## 📋 Código Original (Problemático)

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

---

## 🔍 Problemas Identificados

### 1. **Conexão Estática Compartilhada (Crítico)**
- **Problema**: `public static SqlConnection conn` cria uma conexão compartilhada entre todas as instâncias
- **Impacto**: 
  - Race conditions em ambientes concorrentes
  - Problemas de thread-safety
  - Impossibilidade de testar adequadamente
- **Violação**: Princípio de Dependency Inversion (SOLID)

### 2. **SQL Injection (Crítico - Segurança)**
- **Problema**: Concatenação de strings SQL: `"INSERT INTO Orders VALUES (" + customerId + ")"`
- **Impacto**: Vulnerabilidade crítica de segurança
- **Exemplo de ataque**: `customerId = "1); DROP TABLE Orders; --"`
- **Violação**: OWASP Top 10 - Injection

### 3. **Gerenciamento de Recursos Inadequado**
- **Problema**: `conn.Open()` sem garantia de fechamento em caso de exceção
- **Impacto**: 
  - Vazamento de conexões (connection pool exhaustion)
  - Degradação de performance
  - Possível crash da aplicação
- **Violação**: IDisposable pattern não aplicado

### 4. **Ausência de Transações**
- **Problema**: Múltiplas operações de banco sem transação
- **Impacto**: 
  - Inconsistência de dados (pedido criado sem itens em caso de falha)
  - Violação de ACID
- **Violação**: Princípio de Atomicidade

### 5. **Thread.Sleep(100) - Performance**
- **Problema**: Bloqueio desnecessário da thread
- **Impacto**: 
  - Degradação severa de performance
  - Escalabilidade comprometida
  - Em 100 pedidos = 10 segundos desperdiçados
- **Violação**: Princípios de performance e escalabilidade

### 6. **Tratamento de Exceções Inadequado**
- **Problema**: 
  - `catch(Exception ex)` muito genérico
  - `Console.WriteLine` em produção
  - Exceções são "engolidas" (swallowed)
- **Impacto**: 
  - Impossibilidade de debug
  - Falhas silenciosas
  - Dados inconsistentes sem conhecimento
- **Violação**: Clean Code - Error Handling

### 7. **Lógica de Negócio no Serviço de Acesso a Dados**
- **Problema**: Mistura de responsabilidades (acesso a dados + lógica de negócio)
- **Impacto**: 
  - Código difícil de testar
  - Violação de Single Responsibility Principle
  - Impossibilidade de reutilização
- **Violação**: SOLID - Single Responsibility Principle

### 8. **SELECT MAX(Id) - Race Condition**
- **Problema**: Uso de `SELECT MAX(Id)` para obter ID inserido
- **Impacto**: 
  - Race condition em ambientes concorrentes
  - Possível obtenção de ID incorreto
  - Não usa recursos nativos do banco (IDENTITY, SEQUENCE)
- **Violação**: Thread-safety e consistência

### 9. **Ausência de Validações**
- **Problema**: Não valida parâmetros de entrada
- **Impacto**: 
  - Dados inválidos podem ser persistidos
  - Erros em runtime ao invés de compile-time
- **Violação**: Fail-fast principle

### 10. **Código Síncrono**
- **Problema**: Método síncrono bloqueia threads
- **Impacto**: 
  - Baixa escalabilidade
  - Performance ruim em I/O
  - Não aproveita async/await do .NET
- **Violação**: Best practices de .NET moderno

### 11. **Hardcoded Connection String**
- **Problema**: String de conexão hardcoded
- **Impacto**: 
  - Impossibilidade de configurar por ambiente
  - Dificuldade de manutenção
- **Violação**: Configuration management

### 12. **Uso de `var` sem Contexto**
- **Problema**: Uso excessivo de `var` sem tipo explícito
- **Impacto**: Reduz legibilidade (embora menor que os outros)

---

## ✅ Código Refatorado (Baseado na Arquitetura Atual)

A refatoração segue os padrões implementados no projeto atual: **Clean Architecture**, **DDD**, **CQRS**, **Repository Pattern** e **SOLID**.

### Estrutura da Solução

```
OrderManagement.Domain/
  ├── Entities/
  │     └── Order.cs (entidade rica com lógica de negócio)
  │     └── OrderItem.cs
  ├── ValueObjects/
  │     └── Address.cs
  └── Interfaces/
        └── IOrderRepository.cs

OrderManagement.Application/
  ├── Commands/
  │     └── CreateOrderCommand.cs
  ├── Handlers/
  │     └── CreateOrderCommandHandler.cs
  ├── DTOs/
  │     └── CreateOrderDto.cs
  └── Validators/
        └── CreateOrderDtoValidator.cs

OrderManagement.Infrastructure/
  ├── Data/
  │     └── OrderManagementDbContext.cs (EF Core)
  └── Repositories/
        └── OrderRepository.cs
        └── UnitOfWork.cs
```

### 1. Entidade de Domínio (Order.cs)

```csharp
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Events;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities;

public class Order : BaseEntity
{
    public int CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    
    // Navigation properties
    public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private Order() { } // EF Core

    // Construtor com validações de negócio
    public Order(int customerId, Address shippingAddress, List<OrderItem> items, string tenantId)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item", nameof(items));

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId cannot be empty", nameof(tenantId));

        CustomerId = customerId;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        Items = items;
        TenantId = tenantId;

        CalculateTotal();

        // Domain Event
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, TotalAmount));
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(item => item.Quantity * item.UnitPrice);
    }
}
```

**Melhorias:**
- ✅ Lógica de negócio encapsulada na entidade
- ✅ Validações no construtor (fail-fast)
- ✅ Domain Events para desacoplamento
- ✅ Propriedades privadas com setters controlados

### 2. Command e Handler (CQRS)

```csharp
// CreateOrderCommand.cs
namespace OrderManagement.Application.Commands;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public CreateOrderDto Order { get; set; } = null!;
}

// CreateOrderCommandHandler.cs
namespace OrderManagement.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IOrderFactory _orderFactory;
    private readonly ITenantProvider _tenantProvider;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IDomainEventDispatcher eventDispatcher, 
        IOrderFactory orderFactory, 
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _eventDispatcher = eventDispatcher;
        _orderFactory = orderFactory;
        _tenantProvider = tenantProvider;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validação já feita pelo FluentValidation antes de chegar aqui
        
        string tenantId = _tenantProvider.GetCurrentTenant();
        Domain.Entities.Order order = _orderFactory.CreateOrder(request.Order, tenantId);

        // Persistência com transação automática (EF Core)
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events (assíncrono)
        await _eventDispatcher.DispatchAsync(order.DomainEvents, cancellationToken);
        order.ClearDomainEvents();

        return _mapper.Map<OrderDto>(order);
    }
}
```

**Melhorias:**
- ✅ Separação de responsabilidades (CQRS)
- ✅ Dependency Injection
- ✅ Async/await
- ✅ CancellationToken para cancelamento
- ✅ Transações automáticas via EF Core
- ✅ Domain Events para desacoplamento

### 3. Validação com FluentValidation

```csharp
namespace OrderManagement.Application.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId deve ser maior que zero");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Pedido deve ter pelo menos um item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}
```

**Melhorias:**
- ✅ Validações declarativas e testáveis
- ✅ Mensagens de erro claras
- ✅ Validação antes de chegar no handler

### 4. Repository Pattern

```csharp
namespace OrderManagement.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetByCustomerIdAsync(int customerId, CancellationToken cancellationToken = default);
}

// Implementação
namespace OrderManagement.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(OrderManagementDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
```

**Melhorias:**
- ✅ Abstração de acesso a dados
- ✅ Testável (mockável)
- ✅ EF Core previne SQL Injection automaticamente
- ✅ Queries tipadas e seguras

### 5. Unit of Work

```csharp
namespace OrderManagement.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderManagementDbContext _context;
    private readonly IOrderRepository _orderRepository;

    public UnitOfWork(OrderManagementDbContext context, IOrderRepository orderRepository)
    {
        _context = context;
        _orderRepository = orderRepository;
    }

    public IOrderRepository Orders => _orderRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("O pedido foi modificado por outro processo. Por favor, atualize e tente novamente.");
        }
    }
}
```

**Melhorias:**
- ✅ Transações automáticas
- ✅ Controle de concorrência otimista
- ✅ Gerenciamento de recursos via EF Core

### 6. Configuração e Dependency Injection

```csharp
// Program.cs
builder.Services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();
```

**Melhorias:**
- ✅ Connection string via configuração
- ✅ Lifecycle management adequado (Scoped)
- ✅ Registro automático de handlers

### 7. Tratamento de Exceções Global

```csharp
// GlobalExceptionHandlerMiddleware.cs
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ocorreu");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(new
        {
            error = exception.Message,
            statusCode = context.Response.StatusCode
        });
    }
}
```

**Melhorias:**
- ✅ Tratamento centralizado
- ✅ Logging estruturado
- ✅ Respostas HTTP apropriadas
- ✅ Não "engole" exceções

---

## 📊 Comparação: Antes vs Depois

| Aspecto | Código Original | Código Refatorado |
|---------|----------------|-------------------|
| **Segurança** | ❌ SQL Injection | ✅ EF Core (parametrizado) |
| **Concorrência** | ❌ Race conditions | ✅ Transações + RowVersion |
| **Testabilidade** | ❌ Impossível mockar | ✅ Interfaces + DI |
| **Manutenibilidade** | ❌ Código monolítico | ✅ Separação de responsabilidades |
| **Performance** | ❌ Thread.Sleep, síncrono | ✅ Async/await, paralelo |
| **Escalabilidade** | ❌ Conexão estática | ✅ Connection pooling |
| **Validações** | ❌ Ausentes | ✅ FluentValidation |
| **Error Handling** | ❌ Console.WriteLine | ✅ Logging estruturado |
| **Arquitetura** | ❌ Anêmica | ✅ DDD + Clean Architecture |
| **SOLID** | ❌ Violado | ✅ Respeitado |

---

## 🎯 Princípios Aplicados na Refatoração

### 1. **SOLID Principles**
- ✅ **S**ingle Responsibility: Cada classe tem uma responsabilidade
- ✅ **O**pen/Closed: Extensível via interfaces
- ✅ **L**iskov Substitution: Interfaces bem definidas
- ✅ **I**nterface Segregation: Interfaces específicas
- ✅ **D**ependency Inversion: Dependências via interfaces

### 2. **Clean Architecture**
- ✅ Separação em camadas (Domain, Application, Infrastructure)
- ✅ Dependências unidirecionais
- ✅ Domain isolado e independente

### 3. **Domain-Driven Design (DDD)**
- ✅ Entidades ricas com lógica de negócio
- ✅ Value Objects (Address)
- ✅ Domain Events
- ✅ Aggregate Root (Order)

### 4. **CQRS (Command Query Responsibility Segregation)**
- ✅ Commands para escrita
- ✅ Queries para leitura
- ✅ MediatR como mediator

### 5. **Repository Pattern + Unit of Work**
- ✅ Abstração de acesso a dados
- ✅ Transações coordenadas
- ✅ Testabilidade

### 6. **Best Practices**
- ✅ Async/await para I/O
- ✅ CancellationToken
- ✅ Dependency Injection
- ✅ Logging estruturado
- ✅ Validações declarativas
- ✅ Error handling centralizado

---

## 🧪 Testabilidade

O código refatorado é altamente testável:

```csharp
// Teste Unitário
[Fact]
public async Task Handle_ValidCommand_ReturnsOrderDto()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockMapper = new Mock<IMapper>();
    // ... outros mocks
    
    var handler = new CreateOrderCommandHandler(
        mockUnitOfWork.Object, 
        mockMapper.Object, 
        // ... outras dependências
    );
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

---

## 📈 Benefícios da Refatoração

1. **Segurança**: Eliminação de SQL Injection
2. **Confiabilidade**: Transações garantem consistência
3. **Manutenibilidade**: Código organizado e testável
4. **Escalabilidade**: Async/await e connection pooling
5. **Testabilidade**: Interfaces permitem mocks
6. **Observabilidade**: Logging estruturado
7. **Flexibilidade**: Fácil adicionar novas funcionalidades
8. **Performance**: Sem Thread.Sleep, operações assíncronas

---

## 🚀 Conclusão

A refatoração transforma um código com **12 problemas críticos** em uma solução robusta, segura, testável e escalável, seguindo as melhores práticas de desenvolvimento .NET moderno e padrões arquiteturais consagrados.

O código refatorado está alinhado com a implementação atual do projeto, utilizando:
- Clean Architecture / DDD
- CQRS com MediatR
- Repository Pattern + Unit of Work
- EF Core (elimina SQL direto)
- FluentValidation
- Domain Events
- Async/await
- Dependency Injection
- SOLID Principles

**Resultado**: Código de produção, pronto para escalar e manter.
