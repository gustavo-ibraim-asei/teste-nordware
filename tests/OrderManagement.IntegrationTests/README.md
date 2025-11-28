# Testes de Integração

Este projeto contém testes de integração que verificam o funcionamento completo do sistema.

## Testes Implementados

### 1. Fluxo Completo de Criação de Pedido ✅
- **Arquivo:** `OrderCreationFlowTests.cs`
- **Cenários:**
  - Criação de pedido com múltiplos itens
  - Recuperação de pedido por ID
  - Atualização de status
  - Cancelamento de pedido
  - Validação de dados inválidos

### 2. Integração com Banco de Dados ✅
- **Arquivo:** `DatabaseIntegrationTests.cs`
- **Cenários:**
  - Persistência de pedidos no banco
  - Filtros por cliente
  - Filtros por status
  - Transações (commit e rollback)
  - Multitenancy e isolamento de dados

### 3. Publicação/Consumo de Mensagens ✅
- **Arquivo:** `MessagingIntegrationTests.cs`
- **Cenários:**
  - Publicação de eventos (OrderCreated, OrderStatusChanged, OrderCancelled)
  - Limpeza de recursos

### 4. Testes End-to-End ✅
- **Arquivo:** `EndToEndOrderFlowTests.cs`
- **Cenários:**
  - Fluxo completo: criar → atualizar → cancelar
  - Listagem com filtros
  - Verificação em banco de dados

## Como Executar

```bash
# Executar todos os testes de integração
dotnet test tests/OrderManagement.IntegrationTests

# Executar um teste específico
dotnet test tests/OrderManagement.IntegrationTests --filter "FullyQualifiedName~OrderCreationFlowTests"

# Executar com output detalhado
dotnet test tests/OrderManagement.IntegrationTests --verbosity detailed
```

## Configuração

Os testes usam:
- **In-Memory Database** para testes rápidos
- **WebApplicationFactory** para testes de API
- **Moq** para mocks quando necessário
- **TestContainers** (opcional) para testes com RabbitMQ real

## Notas

- Os testes de mensageria podem falhar se RabbitMQ não estiver rodando
- Para testes com RabbitMQ real, use TestContainers (já incluído nas dependências)
- Cada teste usa um banco de dados isolado (In-Memory)


