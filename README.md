# Order Management System - API RESTful

## 📋 Descrição do Projeto

Sistema de gestão de pedidos de e-commerce desenvolvido em **.NET 10** com **ASP.NET Core Web API**. O sistema implementa uma arquitetura **Clean Architecture/DDD**, processamento assíncrono com **RabbitMQ**, integrações externas, autenticação **JWT**, **multitenancy** e diversas funcionalidades avançadas.

### 🎯 Objetivo

Desenvolver uma API RESTful completa para gerenciamento de pedidos de um e-commerce integrado com múltiplos marketplaces. O sistema processa pedidos de forma assíncrona, realiza integrações externas e garante alta disponibilidade.

### ✨ Principais Características

- **Arquitetura Limpa**: Separação clara de responsabilidades em camadas (Domain, Application, Infrastructure, API)
- **DDD**: Domain-Driven Design com entidades ricas, value objects e domain events
- **CQRS**: Separação de comandos e queries usando MediatR
- **Processamento Assíncrono**: RabbitMQ com Dead Letter Queue e idempotência
- **Multitenancy**: Isolamento completo de dados por tenant
- **Autenticação JWT**: Sistema completo de registro, login e autorização
- **Cache Distribuído**: Redis para melhorar performance de consultas
- **Concorrência Otimista**: Controle de conflitos com RowVersion
- **Testes Abrangentes**: 69+ testes unitários e 16+ testes de integração
- **Documentação Completa**: Swagger, Postman Collection, diagramas

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, organizado em camadas bem definidas com dependências unidirecionais (camadas externas dependem das internas, nunca o contrário):

```
OrderManagement/
├── src/
│   ├── OrderManagement.Domain/          # Camada de Domínio (núcleo)
│   │   ├── Entities/                    # Entidades de negócio (Order, OrderItem, User, Role)
│   │   ├── ValueObjects/                 # Objetos de valor (Address, ShippingOption)
│   │   ├── Events/                      # Eventos de domínio
│   │   ├── Interfaces/                  # Contratos (IOrderRepository, IUnitOfWork)
│   │   └── Enums/                       # Enumerações
│   │
│   ├── OrderManagement.Application/      # Camada de Aplicação
│   │   ├── Commands/                    # Comandos CQRS (CreateOrder, UpdateStatus)
│   │   ├── Queries/                     # Queries CQRS (GetOrderById, GetOrders)
│   │   ├── Handlers/                    # Handlers MediatR
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Services/                    # Serviços de aplicação
│   │   ├── Validators/                  # FluentValidation
│   │   └── Mappings/                    # AutoMapper profiles
│   │
│   ├── OrderManagement.Infrastructure/  # Camada de Infraestrutura
│   │   ├── Data/                        # DbContext, Migrations, DbInitializer
│   │   ├── Repositories/                 # Implementações de repositórios
│   │   ├── ExternalServices/            # Integrações externas (ViaCEP, Shipping)
│   │   ├── Services/                    # Serviços de infraestrutura (PasswordHasher)
│   │   ├── Multitenancy/               # Suporte a multitenancy
│   │   └── Configurations/              # Configurações EF Core
│   │
│   ├── OrderManagement.API/             # Camada de Apresentação
│   │   ├── Controllers/                 # API Controllers
│   │   ├── Middleware/                  # Middlewares (Tenant, Exception Handler)
│   │   ├── Services/                    # Serviços da API (JWT, Notification)
│   │   ├── Hubs/                        # SignalR Hubs
│   │   └── Attributes/                  # Atributos customizados
│   │
│   └── OrderManagement.Messaging/       # Camada de Mensageria
│       ├── Publishers/                  # Publicadores RabbitMQ
│       └── Consumers/                  # Consumers RabbitMQ
│
├── tests/
│   ├── OrderManagement.UnitTests/       # Testes unitários
│   └── OrderManagement.IntegrationTests/ # Testes de integração
│
├── frontend/                            # SPA Vue 3
├── k8s/                                 # Manifests Kubernetes
├── postman/                             # Postman Collection
└── docs/                                # Documentação e diagramas
```

### Fluxo de Dependências

```
API → Application → Domain
  ↓         ↓
Infrastructure → Domain
  ↓
Messaging → Domain
```

**Regra**: Apenas dependências de dentro para fora. Domain não depende de nada.

### Padrões Arquiteturais Implementados

- ✅ **Clean Architecture / DDD**
  - Separação clara de responsabilidades por camadas
  - Domain isolado e independente
  - Dependências unidirecionais

- ✅ **Repository Pattern**
  - Abstração de acesso a dados
  - `IRepository<T>`, `IOrderRepository`, `IUserRepository`
  - Implementações em Infrastructure

- ✅ **Unit of Work**
  - Gerenciamento de transações
  - Coordenação de múltiplos repositórios
  - Controle de concorrência otimista

- ✅ **Dependency Injection**
  - Injeção via construtor
  - Registrado em `Program.cs`
  - Ciclo de vida apropriado (Scoped, Singleton, Transient)

- ✅ **CQRS** (Command Query Responsibility Segregation)
  - Commands para escrita (CreateOrder, UpdateStatus)
  - Queries para leitura (GetOrderById, GetOrders)
  - MediatR como mediator pattern

- ✅ **SOLID Principles**
  - **S**ingle Responsibility: Cada classe tem uma responsabilidade
  - **O**pen/Closed: Extensível via interfaces
  - **L**iskov Substitution: Interfaces bem definidas
  - **I**nterface Segregation: Interfaces específicas
  - **D**ependency Inversion: Dependências via interfaces

- ✅ **Domain Events**
  - `OrderCreatedEvent`, `OrderStatusChangedEvent`, `OrderCancelledEvent`
  - Publicação assíncrona via RabbitMQ
  - Desacoplamento entre componentes

---

## 🚀 Tecnologias Utilizadas

### Backend
- **.NET 10** com ASP.NET Core Web API
- **Entity Framework Core** (Code First)
- **PostgreSQL** (banco de dados)
- **RabbitMQ** (processamento assíncrono)
- **Redis** (cache distribuído)
- **Serilog** (logs estruturados)
- **FluentValidation** (validações)
- **AutoMapper** (mapeamento de objetos)
- **MediatR** (mediator pattern para CQRS)
- **Polly** (retry policies e circuit breaker)
- **JWT Bearer** (autenticação)
- **SignalR** (atualizações em tempo real)

### Frontend
- **Vue 3** com Composition API
- **Vite** (build tool)
- **Pinia** (state management)
- **Vue Router** (roteamento)

### DevOps
- **Docker** e **Docker Compose**
- **Kubernetes** (manifests)
- **GitHub Actions** (CI/CD)

### Testes
- **xUnit** (testes unitários)
- **Moq** (mocks)
- **FluentAssertions** (assertions)

---

## 📦 Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose
- PostgreSQL (ou usar Docker)
- RabbitMQ (ou usar Docker)
- Redis (opcional, para cache)

---

## 🔧 Setup e Execução

### 1. Clonar o Repositório

```bash
git clone <repository-url>
cd OrderManagement
```

### 2. Executar com Docker Compose

O projeto inclui um `docker-compose.yml` que configura automaticamente PostgreSQL, RabbitMQ e Redis:

```bash
docker-compose up -d
```

Isso iniciará os seguintes serviços:
- **PostgreSQL** na porta `5432`
  - Database: `OrderManagement`
  - Username: `postgres`
  - Password: `postgres`
- **RabbitMQ** na porta `5672` (Management UI em `http://localhost:15672`)
  - Username: `guest`
  - Password: `guest`
- **Redis** na porta `6379` (opcional, para cache distribuído)

Para verificar se os serviços estão rodando:
```bash
docker-compose ps
```

Para parar os serviços:
```bash
docker-compose down
```

Para parar e remover volumes (limpar dados):
```bash
docker-compose down -v
```

### 3. Configurar Variáveis de Ambiente

Copie o arquivo `appsettings.Development.json` e ajuste as conexões se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=OrderManagement;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### 4. Executar Migrações

As migrações do Entity Framework Core são aplicadas automaticamente na inicialização da aplicação através do `DbInitializer`. 

**O que é criado automaticamente:**
- Todas as tabelas do banco de dados (Orders, OrderItems, Users, Roles, etc.)
- Roles iniciais: `User` e `Admin`
- Índices e constraints necessários

**Nota**: Se precisar recriar o banco do zero, pare a aplicação, execute `docker-compose down -v` para remover os volumes, e reinicie os serviços.

### 5. Executar a API

**Opção 1: Executar diretamente com .NET CLI**

```bash
cd src/OrderManagement.API
dotnet run
```

**Opção 2: Executar com Docker**

```bash
docker build -t order-management-api .
docker run -p 5000:8080 --env-file .env order-management-api
```

A API estará disponível em:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/health`

**Nota**: Certifique-se de que os serviços do Docker Compose (PostgreSQL, RabbitMQ, Redis) estão rodando antes de iniciar a API.

### 6. Executar o Frontend (Opcional)

```bash
cd frontend
npm install
npm run dev
```

O frontend estará disponível em `http://localhost:5173`

---

## 📚 Documentação da API

### Swagger/OpenAPI

A documentação completa e interativa da API está disponível via Swagger em:
- **URL**: `http://localhost:5000/swagger`

O Swagger inclui:
- ✅ Todos os endpoints documentados
- ✅ Schemas de request/response
- ✅ Exemplos de uso
- ✅ Autenticação JWT integrada (botão "Authorize")
- ✅ Teste direto dos endpoints

**Nota**: Para testar endpoints protegidos, primeiro faça login via `/api/auth/login` e copie o token retornado. Depois, clique em "Authorize" no Swagger e cole o token no formato: `Bearer {seu_token}`

### Autenticação

A API utiliza autenticação JWT. Para obter um token:

1. **Registrar usuário**:
   ```http
   POST /api/auth/register
   Content-Type: application/json
   
   {
     "email": "usuario@exemplo.com",
     "userName": "usuario",
     "password": "senhaSegura123",
     "tenantId": "tenant1"
   }
   ```

2. **Login**:
   ```http
   POST /api/auth/login
   Content-Type: application/json
   
   {
     "emailOrUserName": "usuario@exemplo.com",
     "password": "senhaSegura123",
     "tenantId": "tenant1"
   }
   ```

3. **Usar o token**:
   ```http
   Authorization: Bearer {seu_token_jwt}
   X-Tenant-Id: tenant1
   ```

### Webhook de Pagamento

O webhook de pagamento é um endpoint público que recebe atualizações de gateways de pagamento:

```http
POST /api/paymentwebhook/payment-update
Content-Type: application/json

{
  "orderId": 123,
  "paymentStatus": "paid",
  "transactionId": "TXN-12345",
  "amount": 109.97,
  "processedAt": "2025-01-15T10:30:00Z"
}
```

**Status de pagamento suportados:**
- `paid`, `approved`, `confirmed` → Atualiza pedido para `Confirmed`
- `pending`, `processing` → Mantém pedido como `Pending`
- `cancelled`, `refunded`, `rejected` → Atualiza pedido para `Cancelled`

**Nota**: Este endpoint não requer autenticação JWT, pois é chamado por gateways de pagamento externos.

### Endpoints Principais

#### Pedidos
- `POST /api/orders` - Criar novo pedido
- `GET /api/orders` - Listar pedidos com filtros, paginação e ordenação
- `GET /api/orders/{id}` - Obter pedido por ID
- `PUT /api/orders/{id}/status` - Atualizar status do pedido
- `DELETE /api/orders/{id}` - Cancelar pedido
- `POST /api/orders/{id}/complete` - Finalizar pedido com frete
- `POST /api/orders/batch` - Processar múltiplos pedidos em paralelo

#### Autenticação
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/login` - Autenticar e obter token JWT
- `GET /api/auth/me` - Obter informações do usuário autenticado

#### Frete
- `POST /api/shipping/calculate` - Calcular opções de frete

#### Webhook
- `POST /api/paymentwebhook/payment-update` - Receber atualizações de pagamento (público)

#### Feature Flags
- `GET /api/featureflags/{featureName}` - Obter status de feature flag
- `POST /api/featureflags/{featureName}` - Atualizar feature flag (requer role Admin)

#### Health Check
- `GET /health` - Verificar saúde da aplicação e dependências

### Postman Collection

Uma collection completa do Postman está disponível em:
- **Arquivo**: `postman/OrderManagement.postman_collection.json`

**Como usar:**
1. Importe a collection no Postman
2. Configure as variáveis de ambiente:
   - `base_url`: `http://localhost:5000`
   - `jwt_token`: (será preenchido automaticamente após login)
   - `tenant_id`: (será preenchido automaticamente após login)
3. Execute o request "Register User" ou "Login" primeiro para obter o token
4. Os próximos requests usarão automaticamente o token JWT

---

## 🎯 Funcionalidades Implementadas

### 1. Gestão de Pedidos (CRUD Completo)

- ✅ **Criar pedido** com múltiplos itens
  - `POST /api/orders`
  - Suporta frete opcional na criação
  - Validações com FluentValidation

- ✅ **Consultar pedidos** com filtros avançados
  - `GET /api/orders`
  - Filtros: `customerId`, `status`, `startDate`, `endDate`
  - Paginação: `page`, `pageSize`
  - Ordenação: `sortBy`, `sortDescending`

- ✅ **Obter pedido por ID**
  - `GET /api/orders/{id}`

- ✅ **Atualizar status do pedido**
  - `PUT /api/orders/{id}/status`
  - Validações de transição de status no domínio

- ✅ **Cancelar pedido**
  - `DELETE /api/orders/{id}?reason={reason}`
  - Validações de regra de negócio (não pode cancelar se já entregue)

- ✅ **Finalizar pedido com frete**
  - `POST /api/orders/{id}/complete`
  - Frete obrigatório na finalização

- ✅ **Processar pedidos em lote** (paralelo)
  - `POST /api/orders/batch`

- ✅ **Webhook de pagamento**
  - `POST /api/paymentwebhook/payment-update`
  - Recebe atualizações de gateway de pagamento
  - Atualiza status do pedido automaticamente

### 2. Processamento Assíncrono com RabbitMQ

- ✅ **Publicação de eventos**
  - `OrderCreatedEvent` - publicado ao criar pedido
  - `OrderStatusChangedEvent` - publicado ao alterar status
  - `OrderCancelledEvent` - publicado ao cancelar

- ✅ **Consumers**
  - `OrderCreatedConsumer` - processa notificações por email (simulado)
  - `OrderStatusChangedConsumer` - atualiza estoque (simulado)

- ✅ **Dead Letter Queue (DLQ)**
  - Configurado para ambos os consumers
  - Mensagens com falha são enviadas para DLQ

- ✅ **Idempotência**
  - `IdempotentMessageProcessor` garante que mensagens não sejam processadas duas vezes
  - Utiliza tabela `ProcessedMessages` para rastreamento

### 3. Integração com Sistemas Externos

- ✅ **API de consulta de CEP (ViaCEP)**
  - Integração com ViaCEP para validação de endereços
  - Retry policy e circuit breaker com Polly

- ✅ **API de cálculo de frete (mock)**
  - `POST /api/shipping/calculate`
  - Regras complexas:
    - Frete grátis para pedidos acima de R$200
    - Múltiplas transportadoras (Correios, Loggi, JadLog)
    - Tipos de entrega (Padrão, Expresso, Imediato, Econômico)
    - Entrega imediata (mesmo dia) para grandes centros

- ✅ **Webhook para receber atualizações de pagamento**
  - `POST /api/paymentwebhook/payment-update`
  - Recebe atualizações de status de pagamento de gateways externos
  - Mapeia automaticamente para status do pedido:
    - `paid`, `approved`, `confirmed` → `OrderStatus.Confirmed`
    - `pending`, `processing` → `OrderStatus.Pending`
    - `cancelled`, `refunded`, `rejected` → `OrderStatus.Cancelled`
  - Atualiza o status do pedido automaticamente via MediatR
  - Endpoint público (sem autenticação JWT) para receber callbacks de gateways

- ✅ **Retry Policies e Circuit Breaker**
  - Implementado com Polly
  - Retry: 3 tentativas com backoff exponencial
  - Circuit Breaker: abre após 5 falhas, fecha após 30s

### 4. Concorrência e Performance

- ✅ **Processamento paralelo de pedidos em lote**
  - `ProcessOrdersBatchCommandHandler` usa `Task.WhenAll` para processamento paralelo

- ✅ **Controle de concorrência otimista**
  - Entity `Order` possui `RowVersion` (byte[])
  - EF Core detecta conflitos automaticamente

- ✅ **Cache distribuído (Redis)**
  - Implementado em `GetOrdersQueryHandler`
  - Cache de 5 minutos para consultas de pedidos
  - Invalidação automática

- ✅ **Rate Limiting**
  - Configurado com política "fixed"
  - Aplicado em `OrdersController`

---

## Funcionalidades Adicionais (Diferenciais)

### Multitenancy
- ✅ Isolamento de dados por tenant
- ✅ `TenantMiddleware` extrai tenant do header `X-Tenant-Id` ou JWT
- ✅ Global query filters no EF Core

### Health Checks
- ✅ Endpoint `/health`
- ✅ Verifica PostgreSQL, RabbitMQ e Redis

### CI/CD
- ✅ GitHub Actions workflow (`.github/workflows/ci-cd.yml`)
- ✅ Build, testes e push de Docker image

### Kubernetes
- ✅ Manifests em `k8s/`:
  - `deployment.yaml`
  - `service.yaml`
  - `configmap.yaml`
  - `secret.yaml.example`

### Dashboard Frontend
- ✅ SPA Vue 3 com listagem de pedidos
- ✅ Autenticação JWT
- ✅ Integração com SignalR para atualizações em tempo real

### Autenticação JWT
- ✅ Sistema completo de registro e login
- ✅ Password hashing com BCrypt
- ✅ Roles e claims

### SignalR
- ✅ `OrderHub` para notificações em tempo real
- ✅ Grupos por tenant
- ✅ Notificações de criação e atualização de pedidos

### Feature Flags
- ✅ `FeatureFlagsController` para gerenciar features
- ✅ Permite deploys graduais

---

## 🧪 Testes

### Executar Todos os Testes

Para executar todos os testes (unitários e integração) de uma vez, execute na raiz do projeto:

```bash
dotnet test
```

Ou execute os testes de um projeto específico:

### Executar Testes Unitários

```bash
cd tests/OrderManagement.UnitTests
dotnet test
```

Ou da raiz do projeto:
```bash
dotnet test tests/OrderManagement.UnitTests/OrderManagement.UnitTests.csproj
```

### Executar Testes de Integração

**Importante**: Os testes de integração requerem que os serviços do Docker Compose estejam rodando (PostgreSQL e RabbitMQ).

```bash
cd tests/OrderManagement.IntegrationTests
dotnet test
```

Ou da raiz do projeto:
```bash
dotnet test tests/OrderManagement.IntegrationTests/OrderManagement.IntegrationTests.csproj
```

### Executar Testes com Cobertura de Código

Para gerar relatório de cobertura (requer `coverlet.collector`):

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Cobertura de Testes

O projeto possui uma cobertura abrangente de testes:

- ✅ **Testes Unitários** (69 testes):
  - Testes de entidades de domínio (`Order`, `OrderItem`)
  - Testes de handlers (`CreateOrderCommandHandler`, `CompleteOrderCommandHandler`, `RegisterUserCommandHandler`, `LoginCommandHandler`)
  - Testes de services (`ShippingCalculationService`)
  
- ✅ **Testes de Integração** (16 testes):
  - Fluxo completo de criação de pedidos
  - Integração com banco de dados (multitenancy, transações)
  - Publicação/consumo de mensagens RabbitMQ
  - End-to-end (criação, atualização, cancelamento)
  - Autenticação e autorização
  - Cálculo de frete
  - Webhook de pagamento
  - Controle de concorrência otimista

- ✅ **Qualidade dos Testes**:
  - Uso de FluentAssertions para assertions legíveis
  - Mocks com Moq para dependências externas
  - Padrão AAA (Arrange, Act, Assert)
  - Testes isolados e independentes
    

---

## 📖 Questões Teóricas

### 1. Cache Distribuído

**Como implementaria um sistema de cache distribuído para melhorar a performance das consultas de pedidos?**

**Resposta:**

Implementei cache distribuído usando Redis com as seguintes estratégias:

1. **Cache de Consultas**: O `GetOrdersQueryHandler` utiliza `IDistributedCache` para cachear resultados de consultas de pedidos com chave baseada nos filtros aplicados.

2. **Estratégias de Invalidação**:
   - **TTL (Time To Live)**: Cache expira após 5 minutos
   - **Invalidação por Evento**: Quando um pedido é criado/atualizado, o cache relacionado pode ser invalidado
   - **Cache Keys Estruturadas**: `orders:{customerId}:{status}:{page}:{pageSize}` permite invalidação seletiva

3. **Padrões Utilizados**:
   - **Cache-Aside**: Aplicação verifica cache antes de consultar banco
   - **Write-Through**:  implementado para atualizar cache junto com banco

4. **Melhorias Futuras**:
   - Implementar invalidação automática via eventos de domínio
   - Cache de entidades individuais além de listagens
   - Cache warming para consultas frequentes

### 2. Consistência Eventual

**Como garantiria a consistência eventual entre o serviço de pedidos e o serviço de estoque em uma arquitetura distribuída?**

**Resposta:**

A consistência eventual é garantida através de:

1. **Event-Driven Architecture**: Utilizamos RabbitMQ para publicar eventos de mudança de status de pedidos. Quando um pedido é confirmado, o evento `OrderStatusChangedEvent` é publicado.

2. **Saga Pattern**: Para operações complexas, implementaria uma saga que orquestra múltiplos serviços:
   - Pedido criado → Reservar estoque → Confirmar pedido
   - Se reserva falhar → Compensar (cancelar pedido)

3. **Idempotência**: O `IdempotentMessageProcessor` garante que mensagens não sejam processadas duas vezes, evitando duplicação de atualizações de estoque.

4. **Retry e DLQ**: Mensagens com falha são retentadas e, após esgotar tentativas, enviadas para Dead Letter Queue para análise manual.

5. **Event Sourcing** (futuro): Poderia implementar Event Sourcing para rastrear todas as mudanças e permitir reconstrução do estado.

### 3. Retry Resiliente

**Como implementaria um mecanismo de retry resiliente para integrações externas que frequentemente falham?**

**Resposta:**

Implementei retry policies usando **Polly**:

1. **Retry Policy**:
   ```csharp
   .WaitAndRetryAsync(
       retryCount: 3,
       sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
   )
   ```
   - 3 tentativas com backoff exponencial (2s, 4s, 8s)

2. **Circuit Breaker**:
   ```csharp
   .CircuitBreakerAsync(
       handledEventsAllowedBeforeBreaking: 5,
       durationOfBreak: TimeSpan.FromSeconds(30)
   )
   ```
   - Abre circuito após 5 falhas consecutivas
   - Mantém aberto por 30 segundos
   - Evita sobrecarga de serviços externos

3. **Trade-offs**:
   - **Retry Imediato**: Baixa latência, mas pode sobrecarregar serviço
   - **Backoff Exponencial**: Reduz carga, mas aumenta latência total
   - **Circuit Breaker**: Protege serviço externo, mas pode causar falhas temporárias

4. **Melhorias**:
   - Jitter no backoff para evitar thundering herd
   - Retry apenas para erros transientes (5xx, timeouts)
   - Logging detalhado de tentativas

### 4. Refactoring de Método Monolítico

**Como faria o refactoring de um método monolítico de 500 linhas que processa pedidos?**

**Resposta:**

Seguiria os seguintes passos:

1. **Análise e Identificação**:
   - Mapear responsabilidades do método
   - Identificar dependências e acoplamentos
   - Listar regras de negócio envolvidas

2. **Extração de Métodos**:
   - Extrair validações para métodos privados
   - Extrair cálculos para métodos específicos
   - Extrair chamadas externas para serviços

3. **Aplicação de Padrões**:
   - **Strategy Pattern**: Para diferentes tipos de processamento
   - **Command Pattern**: Para operações complexas (já implementado com MediatR)
   - **Factory Pattern**: Para criação de objetos complexos
   - **Template Method**: Para fluxos similares com variações

4. **Separação de Responsabilidades**:
   - Mover validações para FluentValidation
   - Mover cálculos para services específicos
   - Mover persistência para repositories

5. **Domain-Driven Design**:
   - Mover lógica de negócio para entidades de domínio
   - Criar value objects para conceitos complexos
   - Usar domain events para comunicação

6. **Exemplo de Refactoring**:
   ```csharp
   // Antes: Método monolítico
   public void ProcessOrder(int orderId) { /* 500 linhas */ }
   
   // Depois: CQRS com handlers específicos
   public class ProcessOrderCommandHandler : IRequestHandler<ProcessOrderCommand>
   {
       // Delega para services específicos
   }
   ```

### 5. Deadlocks em Alta Concorrência

**Como abordaria o problema de deadlocks em um cenário de alta concorrência no processamento de pedidos?**

**Resposta:**

1. **Prevenção**:
   - **Controle de Concorrência Otimista**: Implementado com `RowVersion` no Entity Framework
   - **Ordem Consistente de Locks**: Sempre adquirir locks na mesma ordem
   - **Timeouts**: Configurar timeouts em transações

2. **Detecção**:
   - **Logging**: Log detalhado de transações e locks
   - **Monitoring**: Alertas quando transações excedem tempo esperado
   - **Application Insights**: Rastreamento de dependências e locks

3. **Resolução**:
   - **Retry com Jitter**: Retry automático com backoff exponencial e jitter
   - **Queue Pattern**: Processar pedidos em fila para evitar concorrência excessiva
   - **Partitioning**: Dividir processamento por tenant ou região

4. **Técnicas Específicas**:
   - **NOLOCK** (não recomendado): Apenas para leituras não críticas
   - **READ COMMITTED SNAPSHOT**: Reduz bloqueios de leitura
   - **Row-Level Locking**: Usar locks granulares

5. **Ferramentas**:
   - **SQL Server Profiler**: Para detectar deadlocks
   - **PostgreSQL Logging**: Configurar `log_lock_waits`
   - **Distributed Tracing**: Jaeger ou Zipkin para rastrear locks distribuídos

---

## 🔍 Code Review

**Arquivo**: `CODE_REVIEW.md`.

---

## 📊 Diagramas

### Arquitetura
- **Arquivo**: `docs/architecture-diagram.png`

### Sequência
- **Arquivo**: `docs/sequence-diagrams.png`
---

## 🚀 Deploy

### Docker

```bash
docker build -t order-management-api .
docker run -p 5000:80 order-management-api
```

### Kubernetes

```bash
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

---

## 📝 Melhorias Futuras e Débitos Técnicos

### Melhorias Planejadas

#### 1. Performance e Escalabilidade
- **Otimização de queries**: Adicionar índices estratégicos em colunas frequentemente consultadas
- **Paginação baseada em cursor**: Melhorar performance em grandes volumes de dados
- **Connection pooling**: Otimizar gerenciamento de conexões com PostgreSQL

#### 2. Funcionalidades de Negócio
- **Sistema de notificações por email real**: Integração com SendGrid, AWS SES, Azure Communication Services ou similar
- **Integração com gateway de pagamento real**: Stripe, PagSeguro, Mercado Pago, Cielo ou similar
- **Integração com Sistema de Anti Fraude**: ClearSale
- **Integração com APIs de frete reais**: Correios, JadLog, Loggi, Lalamovem entre outras
- **Desenvolvimento de distribuição para os marketplaces externos**: Mercado Livre, Centauro, Magalu, Shopee
- **Implementação de checkout completo**: Carrinho, cupons de desconto, múltiplos métodos de pagamento

#### 3. Observabilidade e Monitoramento
- **Integração com Application Insights ou Equivalentes**: Métricas e telemetria
- **Dashboard de monitoramento**: Grafana ou similar
- **Distributed Tracing**: Jaeger ou Zipkin para rastreamento de requisições
- **Alertas proativos**: Notificações para problemas críticos
- **Log aggregation**: ELK Stack ou similar

#### 4. Segurança
- **Rate limiting mais granular**: Por endpoint, por usuário, por IP
- **Implementar refresh tokens**: Renovação automática de tokens JWT
- **OAuth2/OpenID Connect**: Suporte a autenticação externa
- **Auditoria de segurança**: Log de tentativas de acesso não autorizado
- **Criptografia de dados sensíveis**: Criptografar informações críticas no banco

#### 5. Testes e Qualidade
- **Testes de carga e stress**: Usar k6, JMeter ou Artillery
- **Testes de segurança**: OWASP ZAP, testes de penetração
- **Cobertura de código automatizada**: Integrar no CI/CD

#### 6. Arquitetura e Design
- **Event Sourcing**: Rastreamento completo de eventos de domínio
- **Microserviços**: Dividir em serviços menores se necessário
- **API Gateway**: Centralizar roteamento e políticas

### Débitos Técnicos Conhecidos

#### 1. Frontend
- **Interface básica**: Dashboard Vue 3 funcional mas pode ser melhorado com:
  - Mais funcionalidades de visualização
  - Filtros avançados na interface
  - Gráficos e relatórios
  - Melhor tratamento de erros e loading states

#### 2. Código
- **Refatoração de serviços complexos**: `ShippingCalculationService` para implementação real, será dividido em serviços menores, e adicionados
- **Validações adicionais**: Algumas validações de negócio poderiam ser mais robustas

#### 3. Infraestrutura
- **CI/CD mais completo**: Adicionar stages de deploy, rollback automático
- **Ambientes de staging**: Ambiente de homologação antes de produção
- **Disaster recovery**: Plano de recuperação de desastres

---

## 👥 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto foi desenvolvido como teste técnico para nordware
---

## ✅ Checklist de Entrega

- [x] Repositório Git público
- [x] README.md completo com instruções de setup
- [x] Docker Compose funcional
- [x] Testes unitários executando com sucesso
- [x] Commits bem estruturados seguindo Gitflow
- [x] Swagger/OpenAPI acessível em /swagger
- [x] Análise de código aplicada diretamente no projeto (Clean Code e SOLID)
- [x] Respostas às questões teóricas no README
- [x] Postman Collection
- [x] Diagramas de arquitetura e sequência
- [x] Análise de performance e requisitos implementados (documentado no README)

---
