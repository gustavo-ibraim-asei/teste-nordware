# Order Management System - API RESTful

## 📋 Descrição do Projeto

Sistema completo de gestão de pedidos de e-commerce desenvolvido com **.NET 10** (ASP.NET Core Web API) no backend e **Vue 3** no frontend. O sistema implementa uma arquitetura **Clean Architecture/DDD**, processamento assíncrono com **RabbitMQ**, integrações externas, autenticação **JWT**, **multitenancy**, atualizações em tempo real com **SignalR** e diversas funcionalidades avançadas.

### 🎯 Objetivo

Desenvolver uma solução completa (API RESTful + SPA) para gerenciamento de pedidos de um e-commerce integrado com múltiplos marketplaces. O sistema processa pedidos de forma assíncrona, realiza integrações externas, oferece interface web moderna e responsiva, e garante alta disponibilidade.

### ✨ Principais Características

- **Arquitetura Limpa**: Separação clara de responsabilidades em camadas (Domain, Application, Infrastructure, API)
- **DDD**: Domain-Driven Design com entidades ricas, value objects e domain events
- **CQRS**: Separação de comandos e queries usando MediatR
- **Processamento Assíncrono**: RabbitMQ com Dead Letter Queue e idempotência
- **Multitenancy**: Isolamento completo de dados por tenant
- **Autenticação JWT**: Sistema completo de registro, login e autorização
- **Cache Distribuído**: Redis para melhorar performance de consultas
- **Concorrência Otimista**: Controle de conflitos com RowVersion
- **Testes Abrangentes**: 85+ testes unitários e 19+ testes de integração
- **Frontend Moderno**: SPA Vue 3 com interface responsiva tipo e-commerce
- **Tempo Real**: Atualizações automáticas via SignalR
- **Documentação Completa**: Swagger, Postman Collection, diagramas

---

## 🏗️ Arquitetura

O projeto segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, organizado em camadas bem definidas com dependências unidirecionais (camadas externas dependem das internas, nunca o contrário):

```
OrderManagement/
├── src/
│   ├── OrderManagement.Domain/          # Camada de Domínio (núcleo)
│   │   ├── Entities/                    # Entidades de negócio (Order, OrderItem, User, Role, Product, StockOffice, Color, Size, Sku, Stock, PriceTable, ProductPrice, Customer)
│   │   ├── ValueObjects/                 # Objetos de valor (Address, ShippingOption)
│   │   ├── Events/                      # Eventos de domínio
│   │   ├── Helpers/                     # Helpers de domínio (EanGenerator)
│   │   ├── Interfaces/                  # Contratos (IOrderRepository, IUnitOfWork, ISkuRepository, IStockRepository, IPriceTableRepository, IProductPriceRepository, ICustomerRepository)
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

## Questões Teóricas

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

### 4. Deadlocks em Alta Concorrência

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

## 📦 Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose
- PostgreSQL (ou usar Docker)
- RabbitMQ (ou usar Docker)
- Redis (opcional, para cache)

---

## 🔧 Setup e Execução

### 🚀 Início Rápido

A forma mais rápida de executar o sistema completo é usando Docker Compose:

```bash
# 1. Clonar o repositório
git clone <repository-url>
cd OrderManagement

# 2. Executar todos os serviços (PostgreSQL, RabbitMQ, Redis, API, Frontend)
docker-compose up -d

# 3. Aguardar alguns minutos para build inicial das imagens

# 4. Acessar a aplicação
# Frontend: http://localhost:3000
# API Swagger: https://localhost:60545/swagger
```

**Pronto!** O sistema está rodando. Você pode começar a usar o frontend em `http://localhost:3000`.

---

### 1. Clonar o Repositório

```bash
git clone <repository-url>
cd OrderManagement
```

### 2. Executar com Docker Compose

O projeto inclui um `docker-compose.yml` que configura automaticamente todos os serviços necessários:

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
- **Redis** na porta `6379` (cache distribuído)
- **API .NET 10** nas portas `60545` (HTTPS) e `60546` (HTTP)
  - Swagger: `https://localhost:60545/swagger`
  - Health Check: `https://localhost:60545/health`
- **Frontend Vue 3** na porta `3000`
  - Aplicação: `http://localhost:3000`

**Nota**: Na primeira execução, o Docker irá construir as imagens da API e do Frontend, o que pode levar alguns minutos.

Para verificar se os serviços estão rodando:
```bash
docker-compose ps
```

Para ver os logs de um serviço específico:
```bash
docker-compose logs -f order-management-api
docker-compose logs -f order-management-frontend
```

Para parar os serviços:
```bash
docker-compose down
```

Para parar e remover volumes (limpar dados):
```bash
docker-compose down -v
```

Para reconstruir as imagens após mudanças no código:
```bash
docker-compose up -d --build
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

**Migrações disponíveis:**
- `20251128182701_InitialCreate` - Criação inicial do banco de dados (Orders, OrderItems, Users, Roles, Products, StockOffices, Colors, Sizes, Skus, Stocks)
- `20251128182842_AddOrderItemQueryFilter` - Adiciona query filter para OrderItem (multitenancy)
- `20250115000000_AddPriceTablesAndProductPrices` - Adiciona tabelas PriceTables e ProductPrices
- `20251128194019_FixProductPriceRelationship` - Corrige relacionamento entre ProductPrice e Product
- `20251128195010_FixRowVersionDefaultValue` - Adiciona trigger e valor padrão para RowVersion em Orders (habilita extensão pgcrypto)
- `20251128201305_AddCustomersTable` - Adiciona tabela Customers e relacionamento com Orders
- `20251128202207_FixRowVersionInsert` - Adiciona trigger para garantir geração de RowVersion no INSERT

**O que é criado automaticamente:**
- Todas as tabelas do banco de dados (Orders, OrderItems, Users, Roles, Products, StockOffices, Colors, Sizes, Skus, Stocks, PriceTables, ProductPrices, Customers)
- Roles iniciais: `User` e `Admin`
- **Cores básicas**: Preto, Branco, Azul, Vermelho, Verde, Amarelo
- **Tamanhos básicos**: PP, P, M, G, GG, XG
- Índices e constraints necessários (incluindo índices únicos para Product Code, SkuCode, combinação SkuId/StockOfficeId, Email por tenant em Customers)
- Trigger para atualização automática de RowVersion em Orders (PostgreSQL)
- Extensão pgcrypto habilitada para geração de valores aleatórios

**Executar migrações manualmente:**
```bash
# Aplicar todas as migrações pendentes
dotnet ef database update --project src/OrderManagement.Infrastructure --startup-project src/OrderManagement.API --context OrderManagementDbContext

# Criar nova migration
dotnet ef migrations add NomeDaMigration --project src/OrderManagement.Infrastructure --startup-project src/OrderManagement.API --context OrderManagementDbContext

# Reverter última migration
dotnet ef migrations remove --project src/OrderManagement.Infrastructure --startup-project src/OrderManagement.API --context OrderManagementDbContext
```

**Nota**: Se precisar recriar o banco do zero, pare a aplicação, execute `docker-compose down -v` para remover os volumes, e reinicie os serviços.

### 5. Executar a API

**Opção 1: Via Docker Compose (Recomendado)**

Se você executou `docker-compose up -d`, a API já está rodando automaticamente. Acesse:
- **HTTPS**: `https://localhost:60545`
- **HTTP**: `http://localhost:60546`
- **Swagger**: `https://localhost:60545/swagger`
- **Health Check**: `https://localhost:60545/health`

**Opção 2: Executar diretamente com .NET CLI (Desenvolvimento Local)**

```bash
cd src/OrderManagement.API
dotnet run
```

A API estará disponível nas portas configuradas em `launchSettings.json`:
- **HTTPS**: `https://localhost:60545`
- **HTTP**: `http://localhost:60546`

**Opção 3: Executar com Docker (Standalone)**

```bash
docker build -t order-management-api .
docker run -p 60545:8080 -p 60546:8081 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=OrderManagement;Username=postgres;Password=postgres;Port=5432" \
  -e RabbitMQ__HostName=host.docker.internal \
  order-management-api
```

**Nota**: Certifique-se de que os serviços do Docker Compose (PostgreSQL, RabbitMQ, Redis) estão rodando antes de iniciar a API.

### 6. Executar o Frontend

**Opção 1: Via Docker Compose (Recomendado)**

Se você executou `docker-compose up -d`, o frontend já está rodando automaticamente. Acesse:
- **Frontend**: `http://localhost:3000`

O frontend em Docker usa Nginx que faz proxy automático para a API.

**Opção 2: Desenvolvimento Local (Vite Dev Server)**

Para desenvolvimento local sem Docker:

```bash
cd frontend
npm install
npm run dev
```

O frontend estará disponível em `http://localhost:3000`

**Detecção Automática de Ambiente:**
O frontend detecta automaticamente o ambiente e configura as URLs da API adequadamente:

- **Desenvolvimento (Vite)**: Usa proxy do Vite (`/api` → `https://localhost:60545/api`)
- **Produção com Nginx (Docker)**: Usa proxy do Nginx (`/api` → `http://order-management-api:8080/api`)
- **Produção sem Nginx (build local)**: Usa URL direta da API (`https://localhost:60545/api`)

**Importante**: Certifique-se de que a API está rodando antes de acessar o frontend em desenvolvimento local.

---

## 📚 Documentação da API

### Swagger/OpenAPI

A documentação completa e interativa da API está disponível via Swagger em:
- **URL**: `https://localhost:60545/swagger`

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
- `POST /api/orders` - Criar novo pedido (com validação automática de estoque e seleção de filial)
- `GET /api/orders` - Listar pedidos com filtros (`customerId`, `status`, `startDate`, `endDate`), paginação (`page`, `pageSize`) e ordenação (`sortBy`, `sortDescending`)
- `GET /api/orders/{id}` - Obter pedido por ID
- `PUT /api/orders/{id}/status` - Atualizar status do pedido
- `DELETE /api/orders/{id}?reason={reason}` - Cancelar pedido (com motivo opcional)
- `POST /api/orders/{id}/complete` - Finalizar pedido com frete (efetua baixa de estoque de forma atômica)
- `POST /api/orders/batch` - Processar múltiplos pedidos em paralelo

#### Gestão de Estoque

**Filiais de Estoque:**
- `GET /api/stockoffices` - Listar filiais de estoque
- `POST /api/stockoffices` - Criar filial de estoque
- `GET /api/stockoffices/{id}` - Obter filial por ID
- `PUT /api/stockoffices/{id}` - Atualizar filial
- `DELETE /api/stockoffices/{id}` - Deletar filial

**Cores:**
- `GET /api/colors` - Listar cores
- `POST /api/colors` - Criar cor
- `GET /api/colors/{id}` - Obter cor por ID
- `PUT /api/colors/{id}` - Atualizar cor
- `DELETE /api/colors/{id}` - Deletar cor

**Tamanhos:**
- `GET /api/sizes` - Listar tamanhos
- `POST /api/sizes` - Criar tamanho
- `GET /api/sizes/{id}` - Obter tamanho por ID
- `PUT /api/sizes/{id}` - Atualizar tamanho
- `DELETE /api/sizes/{id}` - Deletar tamanho

**Produtos:**
- `GET /api/products` - Listar produtos
- `POST /api/products` - Criar produto (Code obrigatório)
- `GET /api/products/{id}` - Obter produto por ID
- `PUT /api/products/{id}` - Atualizar produto
- `DELETE /api/products/{id}` - Deletar produto

**SKUs:**
- `GET /api/skus` - Listar SKUs (com filtro opcional `?productId={id}`)
- `GET /api/skus/with-stock` - Listar SKUs com estoque disponível (usado no frontend para criação de pedidos)
- `POST /api/skus` - Criar SKU (combinação de Produto + Cor + Tamanho, Barcode gerado automaticamente em EAN-13)
- `GET /api/skus/{id}` - Obter SKU por ID
- `PUT /api/skus/{id}` - Atualizar SKU (Barcode deve ser EAN válido se fornecido)
- `DELETE /api/skus/{id}` - Deletar SKU

**Estoques:**
- `GET /api/stocks` - Listar estoques (com filtros opcionais `?skuId={id}&stockOfficeId={id}`)
- `POST /api/stocks` - Criar registro de estoque
- `GET /api/stocks/{id}` - Obter estoque por ID
- `PUT /api/stocks/{id}` - Atualizar quantidade de estoque
- `POST /api/stocks/{id}/reserve` - Reservar estoque (requer `quantity` no body)
- `POST /api/stocks/{id}/decrease` - Baixar estoque (requer `quantity` no body)
- `DELETE /api/stocks/{id}` - Deletar estoque

**Tabelas de Preços:**
- `GET /api/pricetables` - Listar tabelas de preços (com filtro opcional `?onlyActive=true`)
- `POST /api/pricetables` - Criar tabela de preços
- `GET /api/pricetables/{id}` - Obter tabela de preços por ID
- `PUT /api/pricetables/{id}` - Atualizar tabela de preços
- `DELETE /api/pricetables/{id}` - Deletar tabela de preços

**Preços de Produtos:**
- `GET /api/productprices` - Listar preços de produtos (com filtros opcionais `?productId={id}&priceTableId={id}`)
- `POST /api/productprices` - Criar preço de produto
- `GET /api/productprices/{id}` - Obter preço de produto por ID
- `GET /api/productprices/product/{productId}/pricetable/{priceTableId}` - Obter preço de produto por produto e tabela de preços
- `PUT /api/productprices/{id}` - Atualizar preço de produto
- `DELETE /api/productprices/{id}` - Deletar preço de produto

**Clientes:**
- `GET /api/customers` - Listar clientes (com filtros opcionais `?name={nome}&email={email}`)
- `POST /api/customers` - Criar cliente
- `GET /api/customers/{id}` - Obter cliente por ID
- `PUT /api/customers/{id}` - Atualizar cliente
- `DELETE /api/customers/{id}` - Deletar cliente

#### Autenticação
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/login` - Autenticar e obter token JWT
- `GET /api/auth/me` - Obter informações do usuário autenticado

#### Consulta de CEP
- `GET /api/cep/{zipCode}` - Consultar endereço por CEP (integração com ViaCEP)

#### Frete
- `POST /api/shipping/calculate` - Calcular opções de frete (requer `zipCode`, `orderTotal`, `totalWeight`)

#### Webhook
- `POST /api/paymentwebhook/payment-update` - Receber atualizações de pagamento (público, sem autenticação JWT)

#### Feature Flags
- `GET /api/featureflags/{featureName}` - Obter status de feature flag
- `POST /api/featureflags/{featureName}` - Atualizar feature flag (requer role Admin)

#### Health Check
- `GET /health` - Verificar saúde da aplicação e dependências (PostgreSQL, RabbitMQ, Redis)

### Exemplos de Consumo das APIs

#### 1. Autenticação

**Registrar Usuário:**
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

**Login:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUserName": "usuario@exemplo.com",
  "password": "senhaSegura123",
  "tenantId": "tenant1"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "usuario",
  "email": "usuario@exemplo.com",
  "tenantId": "tenant1",
  "roles": ["User"],
  "expiresIn": 28800
}
```

#### 2. Gestão de Produtos

**Criar Produto:**
```http
POST /api/products
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Camiseta Básica",
  "code": "CAM001",
  "description": "Camiseta básica de algodão"
}
```

**Resposta:**
```json
{
  "id": 1,
  "name": "Camiseta Básica",
  "code": "CAM001",
  "description": "Camiseta básica de algodão",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

**Listar Produtos:**
```http
GET /api/products
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Obter Produto por ID:**
```http
GET /api/products/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Atualizar Produto:**
```http
PUT /api/products/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Camiseta Básica Atualizada",
  "code": "CAM001",
  "description": "Nova descrição"
}
```

**Deletar Produto:**
```http
DELETE /api/products/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

#### 3. Gestão de Cores e Tamanhos

**Criar Cor:**
```http
POST /api/colors
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Preto",
  "code": "BLK"
}
```

**Listar Cores:**
```http
GET /api/colors
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Atualizar Cor:**
```http
PUT /api/colors/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Preto Escuro",
  "code": "BLK"
}
```

**Criar Tamanho:**
```http
POST /api/sizes
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "M",
  "code": "M"
}
```

**Listar Tamanhos:**
```http
GET /api/sizes
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

#### 4. Gestão de SKUs

**Criar SKU:**
```http
POST /api/skus
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "productId": 1,
  "colorId": 1,
  "sizeId": 1
}
```

**Resposta:**
```json
{
  "id": 1,
  "productId": 1,
  "colorId": 1,
  "sizeId": 1,
  "skuCode": "CAM001-BLK-M",
  "barcode": "7891234567890",
  "product": { "id": 1, "name": "Camiseta Básica", "code": "CAM001" },
  "color": { "id": 1, "name": "Preto", "code": "BLK" },
  "size": { "id": 1, "name": "M", "code": "M" }
}
```

**Listar SKUs com Estoque:**
```http
GET /api/skus/with-stock
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Resposta:**
```json
[
  {
    "sku": {
      "id": 1,
      "productId": 1,
      "colorId": 1,
      "sizeId": 1,
      "skuCode": "CAM001-BLK-M",
      "barcode": "7891234567890",
      "product": { "name": "Camiseta Básica", "code": "CAM001" },
      "color": { "name": "Preto", "code": "BLK" },
      "size": { "name": "M", "code": "M" }
    },
    "totalAvailableQuantity": 50
  }
]
```

#### 5. Gestão de Filiais de Estoque

**Criar Filial de Estoque:**
```http
POST /api/stockoffices
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Filial São Paulo",
  "code": "SP01"
}
```

**Listar Filiais:**
```http
GET /api/stockoffices
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Obter Filial por ID:**
```http
GET /api/stockoffices/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

#### 6. Gestão de Estoque

**Criar Estoque:**
```http
POST /api/stocks
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "skuId": 1,
  "stockOfficeId": 1,
  "quantity": 100
}
```

**Resposta:**
```json
{
  "id": 1,
  "skuId": 1,
  "stockOfficeId": 1,
  "quantity": 100,
  "reserved": 0,
  "availableQuantity": 100
}
```

**Reservar Estoque:**
```http
POST /api/stocks/1/reserve
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "quantity": 10
}
```

**Baixar Estoque:**
```http
POST /api/stocks/1/decrease
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "quantity": 5
}
```

**Listar Estoques:**
```http
GET /api/stocks?skuId=1&stockOfficeId=1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Resposta:**
```json
[
  {
    "id": 1,
    "skuId": 1,
    "stockOfficeId": 1,
    "quantity": 100,
    "reserved": 10,
    "availableQuantity": 90,
    "sku": { "skuCode": "CAM001-BLK-M" },
    "stockOffice": { "name": "Filial São Paulo", "code": "SP01" }
  }
]
```

#### 7. Gestão de Clientes

**Criar Cliente:**
```http
POST /api/customers
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao.silva@example.com",
  "phone": "(11) 98765-4321",
  "document": "123.456.789-00"
}
```

**Resposta:**
```json
{
  "id": 1,
  "name": "João Silva",
  "email": "joao.silva@example.com",
  "phone": "(11) 98765-4321",
  "document": "123.456.789-00",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

**Listar Clientes:**
```http
GET /api/customers?name=João
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Obter Cliente por ID:**
```http
GET /api/customers/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Atualizar Cliente:**
```http
PUT /api/customers/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "João Silva Santos",
  "email": "joao.silva@example.com",
  "phone": "(11) 98765-4321",
  "document": "123.456.789-00"
}
```

**Deletar Cliente:**
```http
DELETE /api/customers/1
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

#### 8. Gestão de Pedidos

**Criar Pedido:**
```http
POST /api/orders
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "customerId": 1,
  "shippingAddress": {
    "street": "Rua Exemplo",
    "number": "123",
    "complement": "Apto 45",
    "neighborhood": "Centro",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01310-100"
  },
  "items": [
    {
      "skuId": 1,
      "productId": 1,
      "colorId": 1,
      "sizeId": 1,
      "productName": "Camiseta Básica",
      "quantity": 2,
      "unitPrice": 29.90
    }
  ]
}
```

**Resposta:**
```json
{
  "id": 123,
  "customerId": 1,
  "status": "Pending",
  "totalAmount": 59.80,
  "shippingCost": 0,
  "createdAt": "2025-01-15T10:30:00Z",
  "items": [
    {
      "id": 1,
      "skuId": 1,
      "stockOfficeId": 1,
      "productName": "Camiseta Básica",
      "quantity": 2,
      "unitPrice": 29.90,
      "totalPrice": 59.80
    }
  ]
}
```

**Listar Pedidos com Filtros:**
```http
GET /api/orders?customerId=1&status=Pending&page=1&pageSize=10&sortBy=CreatedAt&sortDescending=true
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Atualizar Status do Pedido:**
```http
PUT /api/orders/123/status
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "status": "Processing"
}
```

**Finalizar Pedido (com baixa de estoque):**
```http
POST /api/orders/123/complete
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "carrierId": 1,
  "shippingTypeId": 1
}
```

**Cancelar Pedido:**
```http
DELETE /api/orders/123?reason=Cliente solicitou cancelamento
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

#### 9. Consulta de CEP

```http
GET /api/cep/01310100
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
```

**Resposta:**
```json
{
  "cep": "01310-100",
  "logradouro": "Avenida Paulista",
  "complemento": "",
  "bairro": "Bela Vista",
  "localidade": "São Paulo",
  "uf": "SP",
  "ibge": "3550308"
}
```

#### 10. Cálculo de Frete

```http
POST /api/shipping/calculate
Authorization: Bearer {seu_token_jwt}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "zipCode": "01310-100",
  "orderTotal": 100.00,
  "totalWeight": 2.5
}
```

**Resposta:**
```json
{
  "zipCode": "01310-100",
  "orderTotal": 100.00,
  "options": [
    {
      "carrierId": 1,
      "carrierName": "Correios",
      "shippingTypeId": 1,
      "shippingType": "Padrão",
      "price": 15.50,
      "estimatedDays": 5
    },
    {
      "carrierId": 1,
      "carrierName": "Correios",
      "shippingTypeId": 2,
      "shippingType": "Expresso",
      "price": 25.00,
      "estimatedDays": 2
    },
    {
      "carrierId": 2,
      "carrierName": "Loggi",
      "shippingTypeId": 3,
      "shippingType": "Imediato",
      "price": 35.00,
      "estimatedDays": 1
    }
  ]
}
```

#### 11. Webhook de Pagamento

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

**Nota**: Este endpoint é público e não requer autenticação JWT, pois é chamado por gateways de pagamento externos.

#### 12. Exemplo Completo: Fluxo de Criação de Pedido

**Passo 1: Criar Produto**
```http
POST /api/products
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "Camiseta Básica",
  "code": "CAM001",
  "description": "Camiseta básica de algodão"
}
```

**Passo 2: Criar SKU (combinação Produto + Cor + Tamanho)**
```http
POST /api/skus
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "productId": 1,
  "colorId": 1,
  "sizeId": 1
}
```

**Passo 3: Criar Estoque**
```http
POST /api/stocks
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "skuId": 1,
  "stockOfficeId": 1,
  "quantity": 100
}
```

**Passo 4: Consultar SKUs Disponíveis**
```http
GET /api/skus/with-stock
Authorization: Bearer {token}
X-Tenant-Id: tenant1
```

**Passo 5: Criar Cliente**
```http
POST /api/customers
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao.silva@example.com",
  "phone": "(11) 98765-4321",
  "document": "123.456.789-00"
}
```

**Passo 6: Criar Pedido (validação automática de estoque)**
```http
POST /api/orders
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "customerId": 1,
  "shippingAddress": {
    "street": "Rua Exemplo",
    "number": "123",
    "neighborhood": "Centro",
    "city": "São Paulo",
    "state": "SP",
    "zipCode": "01310-100"
  },
  "items": [
    {
      "skuId": 1,
      "productId": 1,
      "colorId": 1,
      "sizeId": 1,
      "productName": "Camiseta Básica",
      "quantity": 2,
      "unitPrice": 29.90
    }
  ]
}
```

**Passo 7: Finalizar Pedido (baixa de estoque)**
```http
POST /api/orders/123/complete
Authorization: Bearer {token}
X-Tenant-Id: tenant1
Content-Type: application/json

{
  "carrierId": 1,
  "shippingTypeId": 1
}
```

### Postman Collection

Uma collection completa do Postman está disponível em:
- **Arquivo**: `postman/OrderManagement.postman_collection.json`

**Como usar:**
1. Importe a collection no Postman
2. Configure as variáveis de ambiente:
   - `base_url`: `https://localhost:60545`
   - `jwt_token`: (será preenchido automaticamente após login)
   - `tenant_id`: (será preenchido automaticamente após login)
3. Execute o request "Register User" ou "Login" primeiro para obter o token
4. Os próximos requests usarão automaticamente o token JWT

---

## 🎯 Funcionalidades Implementadas

### 1. Gestão de Pedidos (CRUD Completo)

- ✅ **Criar pedido** com múltiplos itens e validação de estoque
  - `POST /api/orders`
  - **Validação automática de estoque**: O sistema verifica automaticamente a disponibilidade de estoque para cada item
  - **Seleção automática de filial**: O sistema escolhe automaticamente a filial com estoque disponível
  - **SKU obrigatório**: Cada item deve ter ProductId + ColorId + SizeId (combinação única)
  - Suporta frete opcional na criação
  - Validações com FluentValidation
  - **Regra de negócio**: Se não houver estoque suficiente, o pedido não é criado

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

- ✅ **Finalizar pedido com frete e baixa de estoque**
  - `POST /api/orders/{id}/complete`
  - Frete obrigatório na finalização
  - **Baixa automática de estoque**: Efetua baixa definitiva do estoque de forma atômica (transação)
  - **Validação de concorrência**: Previne estoque negativo mesmo em alta concorrência

- ✅ **Processar pedidos em lote** (paralelo)
  - `POST /api/orders/batch`

- ✅ **Webhook de pagamento**
  - `POST /api/paymentwebhook/payment-update`
  - Recebe atualizações de gateway de pagamento
  - Atualiza status do pedido automaticamente

### 1.1. Sistema de Estoque (DDD/Clean Architecture)

O sistema de estoque foi implementado seguindo os mesmos princípios de DDD e Clean Architecture da entidade `Order`:

#### Entidades de Domínio (Sequência de Implementação)

**1. StockOffice (Filial de Estoque)**
- ✅ Representa uma filial onde os produtos são armazenados
- ✅ Validações de domínio: Nome obrigatório, TenantId obrigatório
- ✅ Métodos de domínio: `UpdateName()`, `UpdateCode()`
- ✅ Seed inicial: Filiais podem ser criadas via API
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/stockoffices`
- ✅ **Testes**: `StockOfficeTests` (domínio), `CreateStockOfficeCommandHandlerTests` (handler)

**2. Stock (Estoque)**
- ✅ Representa o estoque de um SKU em uma filial específica
- ✅ **Propriedade calculada**: `AvailableQuantity` (Quantity - Reserved)
- ✅ Validações de domínio: Quantidade não pode ser negativa, StockOfficeId obrigatório
- ✅ **Métodos de domínio ricos**:
  - `Reserve(quantity)` - Reserva estoque (valida disponibilidade)
  - `ReleaseReservation(quantity)` - Libera reserva
  - `Decrease(quantity)` - Baixa definitiva (valida disponibilidade)
  - `Increase(quantity)` - Incrementa estoque
  - `UpdateQuantity(quantity)` - Atualiza quantidade total
- ✅ **Índice único composto**: `(SkuId, StockOfficeId)` garante um registro por SKU/Filial
- ✅ **Prevenção de estoque negativo**: Todos os métodos validam disponibilidade antes de operar
- ✅ **Inclusão de propriedades de navegação**: StockOffice e Sku são sempre incluídos nas consultas
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/stocks`, `POST /api/stocks/{id}/reserve`, `POST /api/stocks/{id}/decrease`
- ✅ **Testes**: `StockTests` (domínio), `CreateStockCommandHandlerTests`, `UpdateStockCommandHandlerTests`, `ReserveStockCommandHandlerTests`, `DecreaseStockCommandHandlerTests` (handlers), `StockManagementIntegrationTests` (integração)

**3. Color (Cor)**
- ✅ Representa uma cor de produto
- ✅ Validações de domínio: Nome obrigatório, TenantId obrigatório
- ✅ Métodos de domínio: `UpdateName()`, `UpdateCode()`
- ✅ **Seed inicial**: Cores básicas criadas automaticamente (Preto, Branco, Azul, Vermelho, Verde, Amarelo)
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/colors`
- ✅ **Testes**: `ColorTests` (domínio), `CreateColorCommandHandlerTests` (handler)

**4. Size (Tamanho)**
- ✅ Representa um tamanho de produto
- ✅ Validações de domínio: Nome obrigatório, TenantId obrigatório
- ✅ Métodos de domínio: `UpdateName()`, `UpdateCode()`
- ✅ **Seed inicial**: Tamanhos básicos criados automaticamente (PP, P, M, G, GG, XG)
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/sizes`
- ✅ **Testes**: `SizeTests` (domínio), `CreateSizeCommandHandlerTests` (handler)

**5. Product (Produto)**
- ✅ Representa um produto no catálogo
- ✅ Validações de domínio: Nome obrigatório, **Code obrigatório**, TenantId obrigatório
- ✅ Métodos de domínio: `UpdateName()`, `UpdateCode()`, `UpdateDescription()`
- ✅ **Relacionamento**: Um produto pode ter múltiplos SKUs (combinações de cor e tamanho)
- ✅ **Índice único**: `(Code, TenantId)` garante unicidade do código por tenant
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/products`
- ✅ **Testes**: `ProductTests` (domínio), `CreateProductCommandHandlerTests` (handler)

**6. Sku (Stock Keeping Unit)**
- ✅ Representa uma combinação única de **Produto + Cor + Tamanho**
- ✅ **Geração automática de códigos**:
  - `SkuCode`: `{ProductCode}-{ColorCode}-{SizeCode}` (ex: "CAM001-BLK-M")
  - `Barcode`: Gerado automaticamente no formato **EAN-13** válido usando `EanGenerator`
- ✅ Validações de domínio: ProductId, ColorId, SizeId obrigatórios e maiores que zero
- ✅ **Validação de tenant**: Product, Color e Size devem pertencer ao mesmo tenant
- ✅ Índice único no banco: `SkuCode` garante unicidade
- ✅ Métodos de domínio: `UpdateBarcode()` (valida formato EAN se fornecido)
- ✅ **Formato EAN**: O código de barras é gerado automaticamente no formato EAN-13 (13 dígitos) com dígito verificador válido
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/skus`, `GET /api/skus/with-stock`
- ✅ **Testes**: `CreateSkuCommandHandlerTests` (handler)

**7. Order (Pedido)**
- ✅ Integração completa com sistema de estoque
- ✅ Validação automática de estoque na criação
- ✅ Seleção automática de filial com estoque disponível
- ✅ Baixa atômica de estoque na finalização
- ✅ Endpoints: `GET/POST/PUT/DELETE /api/orders`, `POST /api/orders/{id}/complete`
- ✅ **Testes**: `OrderTests`, `OrderItemTests` (domínio), `CreateOrderCommandHandlerTests`, `CompleteOrderCommandHandlerTests` (handlers), `OrderCreationFlowTests`, `EndToEndOrderFlowTests` (integração)

#### Integração com Order

- ✅ **OrderItem** atualizado
  - Campos adicionados: `SkuId`, `StockOfficeId`
  - Método de domínio: `SetStockInfo(skuId, stockOfficeId)`
  - Relacionamentos: Navegação para `Sku` e `StockOffice`

- ✅ **Criação de Pedido com Validação de Estoque**
  - Para cada item do pedido:
    1. Sistema busca SKU existente (ProductId + ColorId + SizeId) - **SKU deve existir previamente**
    2. Sistema verifica disponibilidade de estoque
    3. Sistema seleciona automaticamente a filial com estoque suficiente
    4. Sistema atribui `SkuId` e `StockOfficeId` ao item
    5. Se não houver estoque suficiente ou SKU não existir, lança exceção de validação
  - **Regra de negócio**: Pedido não recebe `StockOfficeId` do cliente - o sistema escolhe automaticamente
  - **Regra de negócio**: SKUs devem ser criados antes da criação de pedidos

- ✅ **Finalização de Pedido com Baixa de Estoque**
  - Ao finalizar pedido (`POST /api/orders/{id}/complete`):
    1. Sistema inicia transação (atomicidade)
    2. Para cada item do pedido:
       - Baixa estoque usando `Stock.Decrease()`
       - Valida disponibilidade antes de baixar
    3. Atualiza status do pedido para `Confirmed`
    4. Commit da transação (ou rollback em caso de erro)
  - **Garantia de atomicidade**: Se qualquer item não tiver estoque, toda a operação é revertida

#### Repositórios e Serviços

- ✅ **IRepository<Product>** e **Repository<Product>**
  - CRUD completo para produtos
  - Índice único por Code e TenantId

- ✅ **ISkuRepository** e **SkuRepository**
  - `GetByProductColorSizeAsync()` - Busca SKU pela combinação única (inclui Product, Color, Size)
  - `GetByProductIdAsync()` - Lista SKUs de um produto (inclui Product, Color, Size)
  - `GetBySkuCodeAsync()` - Busca por código SKU (inclui Product, Color, Size)

- ✅ **IStockRepository** e **StockRepository**
  - `GetBySkuAsync()` - Lista estoques de um SKU
  - `GetAvailableStockAsync()` - Busca filial com estoque disponível (ordena por maior disponibilidade)
  - `GetBySkuAndOfficeAsync()` - Busca estoque específico
  - `GetByStockOfficeAsync()` - Lista estoques de uma filial

- ✅ **IStockService** e **StockService**
  - `CheckAvailabilityAsync()` - Verifica disponibilidade e retorna filial com estoque
  - `ReserveStockAsync()` - Reserva estoque
  - `ReleaseReservationAsync()` - Libera reserva
  - `DecreaseStockAsync()` - Baixa definitiva de estoque

#### Frontend - Criação de Pedidos

- ✅ **Seleção de SKUs com Estoque**
  - Endpoint `/api/skus/with-stock` retorna apenas SKUs que possuem estoque disponível
  - Interface mostra: Produto ID, Cor, Tamanho e quantidade disponível
  - Validação em tempo real: Quantidade máxima limitada ao estoque disponível
  - **UX melhorada**: Usuário não precisa selecionar filial - sistema escolhe automaticamente

#### Padrões DDD Aplicados

Todas as novas entidades seguem os mesmos padrões da entidade `Order`:

- ✅ **Encapsulamento**: Propriedades com `private set`, acesso apenas via métodos de domínio
- ✅ **Validações no construtor**: Invariantes garantidas na criação
- ✅ **Métodos de domínio ricos**: Lógica de negócio nas entidades, não em services
- ✅ **TenantId obrigatório**: Multitenancy garantido no construtor
- ✅ **Construtor privado para EF Core**: `private Entity() { }`
- ✅ **Domain Events** (futuro): Preparado para eventos de domínio se necessário
- ✅ **Value Objects** (futuro): Preparado para extrair conceitos complexos se necessário

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
  - **PostgreSQL**: Triggers automáticos para geração de `RowVersion` no INSERT e UPDATE
  - **Extensão pgcrypto**: Habilitada para geração de valores aleatórios (`gen_random_bytes(8)`)
  - **Valor padrão**: `RowVersion` gerado automaticamente pelo banco de dados

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

O frontend é uma **SPA (Single Page Application)** desenvolvida com **Vue 3**, **Vite**, **Pinia** e **Vue Router**, oferecendo uma interface moderna e totalmente responsiva para gestão completa do sistema de pedidos.

#### Tecnologias Utilizadas
- **Vue 3** com Composition API
- **Vite** (build tool e dev server)
- **Pinia** (state management)
- **Vue Router** (roteamento)
- **Axios** (cliente HTTP)
- **@microsoft/signalr** (comunicação em tempo real)
- **Nginx** (servidor web para produção)

#### Funcionalidades Implementadas

**1. Autenticação e Autorização**
- ✅ Tela de registro de usuários (`Register.vue`)
- ✅ Modal de login (`LoginModal.vue`)
- ✅ Autenticação JWT com armazenamento no Pinia store
- ✅ Proteção de rotas (guards)
- ✅ Exibição de informações do usuário autenticado (nome, tenant)

**2. Dashboard Principal (`Dashboard.vue`)**
- ✅ Listagem de pedidos em tempo real
- ✅ Integração com SignalR para atualizações automáticas
- ✅ Filtros por status, cliente e período
- ✅ Visualização de detalhes dos pedidos
- ✅ Atualização automática quando novos pedidos são criados ou status alterados

**3. Gestão de Produtos (`Products.vue`)**
- ✅ Listagem de produtos com paginação
- ✅ Criação, edição e exclusão de produtos
- ✅ Validação de código único por tenant
- ✅ Interface com modais para CRUD

**4. Gestão de Filiais de Estoque (`StockOffices.vue`)**
- ✅ Listagem de filiais
- ✅ Criação, edição e exclusão de filiais
- ✅ Validação de código único

**5. Gestão de Cores (`Colors.vue`)**
- ✅ Listagem de cores
- ✅ Criação, edição e exclusão de cores
- ✅ Validação de código único

**6. Gestão de Tamanhos (`Sizes.vue`)**
- ✅ Listagem de tamanhos
- ✅ Criação, edição e exclusão de tamanhos
- ✅ Validação de código único

**7. Gestão de SKUs (`Skus.vue` e `CreateSku.vue`)**
- ✅ Listagem de SKUs com informações de produto, cor e tamanho
- ✅ Criação de SKUs (combinação Produto + Cor + Tamanho)
- ✅ Validação proativa de duplicidade antes de criar
- ✅ Exibição de estoque disponível por SKU
- ✅ Geração automática de código SKU e barcode EAN-13

**8. Gestão de Estoques (`Stocks.vue` e `CreateStock.vue`)**
- ✅ Listagem de estoques com informações de SKU, filial e quantidades
- ✅ Criação de registros de estoque (associação SKU + Filial + Quantidade)
- ✅ Validação proativa de duplicidade antes de criar
- ✅ Exibição de quantidade total, reservada e disponível
- ✅ Atualização de quantidades

**9. Gestão de Tabelas de Preços (`PriceTables.vue`)**
- ✅ Listagem de tabelas de preços
- ✅ Criação, edição e exclusão de tabelas
- ✅ Ativação/desativação de tabelas
- ✅ Filtro por tabelas ativas

**10. Gestão de Preços de Produtos (`ProductPrices.vue`)**
- ✅ Listagem de preços com filtros por produto e tabela
- ✅ Criação, edição e exclusão de preços
- ✅ Validação de preço único por produto/tabela/tenant
- ✅ Interface com modais para CRUD

**11. Gestão de Clientes (`Customers.vue`)**
- ✅ Listagem de clientes com filtros por nome e email
- ✅ Criação, edição e exclusão de clientes
- ✅ Validação de email único por tenant
- ✅ Interface responsiva com modais

**12. Criação de Pedidos (`CreateOrder.vue`)**
- ✅ **Seleção dinâmica de SKUs**: Carrega apenas SKUs com estoque disponível
- ✅ **Seleção de cliente**: Dropdown com todos os clientes cadastrados
- ✅ **Seleção de tabela de preços**: Dropdown para escolher tabela de preços
- ✅ **Preenchimento automático de preço**: Ao selecionar produto e tabela, preço unitário é preenchido automaticamente
- ✅ **Consulta automática de CEP**: 
  - Consulta ao atingir 8 caracteres
  - Consulta no evento blur do campo
  - Preenchimento automático de endereço completo
- ✅ **Cálculo automático de frete**: 
  - Calcula frete ao selecionar CEP válido
  - Exibe múltiplas opções de transporte
  - Atualiza valor total do pedido
- ✅ **Validação em tempo real**: Validações de estoque, quantidades e campos obrigatórios
- ✅ **Interface intuitiva**: Formulário organizado com múltiplos itens, totais calculados automaticamente
- ✅ **Nomenclatura clara**: Colunas da tabela de itens nomeadas adequadamente

**13. Interface e UX**
- ✅ **Design moderno tipo e-commerce**: Interface limpa e profissional
- ✅ **Totalmente responsivo**: Adaptação para desktop, tablet e mobile
- ✅ **Menu hambúrguer**: Menu lateral retrátil para dispositivos móveis
- ✅ **Sidebar de navegação**: Menu lateral com links para todas as seções
- ✅ **Feedback visual**: Mensagens de sucesso/erro, loading states
- ✅ **Validação proativa**: Verificação de duplicidade antes de criar (SKUs, Estoque)
- ✅ **Integração SignalR**: Atualizações em tempo real sem refresh da página

#### Estrutura do Frontend

```
frontend/
├── src/
│   ├── components/          # Componentes reutilizáveis
│   │   └── LoginModal.vue   # Modal de login
│   ├── config/              # Configurações
│   │   └── api.js           # URLs da API e SignalR
│   ├── router/              # Configuração de rotas
│   │   └── index.js         # Rotas e guards
│   ├── stores/              # Pinia stores (state management)
│   │   ├── auth.js          # Autenticação
│   │   ├── orders.js        # Pedidos + SignalR
│   │   ├── stock.js         # Estoque (produtos, SKUs, etc)
│   │   ├── prices.js        # Tabelas e preços
│   │   └── customers.js     # Clientes
│   ├── views/               # Páginas/Views
│   │   ├── Dashboard.vue    # Dashboard principal
│   │   ├── Products.vue      # Gestão de produtos
│   │   ├── StockOffices.vue # Gestão de filiais
│   │   ├── Colors.vue       # Gestão de cores
│   │   ├── Sizes.vue        # Gestão de tamanhos
│   │   ├── Skus.vue         # Listagem de SKUs
│   │   ├── CreateSku.vue    # Criação de SKU
│   │   ├── Stocks.vue       # Listagem de estoques
│   │   ├── CreateStock.vue  # Criação de estoque
│   │   ├── PriceTables.vue  # Gestão de tabelas de preços
│   │   ├── ProductPrices.vue # Gestão de preços
│   │   ├── Customers.vue    # Gestão de clientes
│   │   ├── CreateOrder.vue  # Criação de pedidos
│   │   └── Register.vue     # Registro de usuários
│   ├── App.vue              # Componente raiz
│   └── main.js              # Entry point
├── Dockerfile               # Dockerfile para produção
├── nginx.conf               # Configuração Nginx
├── package.json            # Dependências
├── vite.config.js           # Configuração Vite
└── index.html               # HTML principal
```

#### Execução do Frontend

**Desenvolvimento:**
```bash
cd frontend
npm install
npm run dev
```
Acesse: `http://localhost:3000`

**Produção (Docker):**
O frontend é servido via Nginx em container Docker. Veja seção "Setup e Execução" para instruções completas.

### Autenticação JWT
- ✅ Sistema completo de registro e login
- ✅ Password hashing com BCrypt
- ✅ Roles e claims

### SignalR
- ✅ `OrderHub` para notificações em tempo real
- ✅ Grupos por tenant (`JoinTenantGroup`, `LeaveTenantGroup`)
- ✅ Notificações de criação e atualização de pedidos (`OrderCreated`, `OrderStatusUpdated`)
- ✅ Autenticação JWT via query string (`access_token`)
- ✅ Configuração de CORS para SignalR
- ✅ Reconexão automática com retry exponencial
- ✅ Integração frontend com `@microsoft/signalr` e atualização automática do Dashboard

### Feature Flags
- ✅ `FeatureFlagsController` para gerenciar features
- ✅ Permite deploys graduais

### Sistema de Preços (DDD/Clean Architecture)

- ✅ **PriceTable (Tabela de Preços)**
  - Representa uma tabela de preços (ex: "Atacado", "Varejo", "Promoção")
  - Validações de domínio: Nome obrigatório, TenantId obrigatório
  - Métodos de domínio: `UpdateName()`, `UpdateDescription()`, `Activate()`, `Deactivate()`
  - Índice único: `(Name, TenantId)` garante unicidade do nome por tenant
  - Endpoints: `GET/POST/PUT/DELETE /api/pricetables`
  - **Testes**: `PriceTableTests` (domínio), `CreatePriceTableCommandHandlerTests` (handler)

- ✅ **ProductPrice (Preço de Produto)**
  - Representa o preço de um produto em uma tabela de preços específica
  - Validações de domínio: ProductId, PriceTableId, UnitPrice obrigatórios e maiores que zero
  - Métodos de domínio: `UpdatePrice(unitPrice)`
  - Índice único composto: `(ProductId, PriceTableId, TenantId)` garante um preço por produto/tabela/tenant
  - Relacionamentos: Navegação para `Product` e `PriceTable`
  - Endpoints: `GET/POST/PUT/DELETE /api/productprices`, `GET /api/productprices/product/{productId}/pricetable/{priceTableId}`
  - **Testes**: `ProductPriceTests` (domínio), `CreateProductPriceCommandHandlerTests` (handler)

### Sistema de Clientes (DDD/Clean Architecture)

- ✅ **Customer (Cliente)**
  - Representa um cliente do sistema
  - Validações de domínio: Nome e Email obrigatórios, TenantId obrigatório
  - Métodos de domínio: `UpdateName()`, `UpdateEmail()`, `UpdatePhone()`, `UpdateDocument()`
  - Índice único: `(Email, TenantId)` garante unicidade do email por tenant
  - Relacionamentos: Um cliente pode ter múltiplos pedidos (`Orders`)
  - Endpoints: `GET/POST/PUT/DELETE /api/customers`
  - **Testes**: `CreateCustomerCommandHandlerTests` (handler)

### Correções e Melhorias Técnicas

- ✅ **RowVersion em PostgreSQL**
  - Correção do problema de `null value in column "RowVersion"` ao criar pedidos
  - Implementação de triggers PostgreSQL para geração automática de `RowVersion`:
    - Trigger `update_orders_row_version`: Atualiza `RowVersion` automaticamente no UPDATE
    - Trigger `set_orders_row_version_on_insert`: Gera `RowVersion` automaticamente no INSERT
  - Extensão `pgcrypto` habilitada para `gen_random_bytes(8)`
  - Valor padrão configurado no EF Core: `HasDefaultValueSql("gen_random_bytes(8)")`

- ✅ **SignalR - Atualizações em Tempo Real**
  - Configuração completa de SignalR com autenticação JWT via query string
  - Grupos por tenant para isolamento de notificações
  - Frontend Vue.js integrado com `@microsoft/signalr`
  - Atualização automática do Dashboard quando pedidos são criados ou atualizados
  - Reconexão automática com retry exponencial
  - Proxy Vite configurado para SignalR (WebSocket e LongPolling)

- ✅ **Otimização de Queries**
  - Refatoração de `GetOrdersQueryHandler` para usar `IQueryable` diretamente do banco
  - Implementação de `GetQueryable()` em `IOrderRepository` e `OrderRepository`
  - Queries executadas diretamente no banco de dados (não em memória)
  - Melhor performance e suporte a paginação eficiente

- ✅ **Frontend - Melhorias de UX**
  - Validação proativa de SKUs e Estoque existentes antes de criar
  - Mensagens de sucesso/erro melhoradas
  - Design responsivo tipo e-commerce
  - Menu hambúrguer para dispositivos móveis
  - Interface moderna e intuitiva

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

O projeto possui uma cobertura abrangente de testes organizados seguindo a hierarquia do sistema:

- ✅ **Testes Unitários** (85+ testes):

  #### Testes de Entidades de Domínio
  
  **StockOffice (Filial de Estoque):**
  - `StockOfficeTests`: Testes de criação, validações de domínio, métodos de atualização
  
  **Stock (Estoque):**
  - `StockTests`: Testes de criação, reserva, liberação de reserva, baixa, incremento, atualização de quantidade, validações de estoque negativo
  
  **Color (Cor):**
  - `ColorTests`: Testes de criação, validações de domínio, métodos de atualização
  
  **Size (Tamanho):**
  - `SizeTests`: Testes de criação, validações de domínio, métodos de atualização
  
  **Product (Produto):**
  - `ProductTests`: Testes de criação, validações de domínio, métodos de atualização
  
  **Sku (Stock Keeping Unit):**
  - Testes de geração automática de códigos (SkuCode e Barcode EAN-13)
  
  **Order (Pedido):**
  - `OrderTests`: Testes de criação, transições de status, validações de negócio
  - `OrderItemTests`: Testes de criação de itens, cálculo de totais
  
  #### Testes de Handlers (Application Layer)
  
  **StockOffice Handlers:**
  - `CreateStockOfficeCommandHandlerTests`: Testa criação de filial com validação de tenant
  
  **Stock Handlers:**
  - `CreateStockCommandHandlerTests`: Testa criação de estoque, validação de duplicidade (SKU + Filial), inclusão de propriedades de navegação
  - `UpdateStockCommandHandlerTests`: Testa atualização de quantidade, validação de estoque inexistente
  - `ReserveStockCommandHandlerTests`: Testa reserva de estoque, validação de disponibilidade
  - `DecreaseStockCommandHandlerTests`: Testa baixa de estoque, validação de estoque inexistente
  
  **Color Handlers:**
  - `CreateColorCommandHandlerTests`: Testa criação de cor com validação de tenant
  
  **Size Handlers:**
  - `CreateSizeCommandHandlerTests`: Testa criação de tamanho com validação de tenant
  
  **Product Handlers:**
  - `CreateProductCommandHandlerTests`: Testa criação de produto, invalidação de cache, validação de tenant
  
  **Sku Handlers:**
  - `CreateSkuCommandHandlerTests`: Testa criação de SKU, validação de combinação única (Product + Color + Size), geração automática de códigos, validação de entidades relacionadas
  
  **Order Handlers:**
  - `CreateOrderCommandHandlerTests`: Testa criação de pedido com validação de estoque, seleção automática de filial
  - `CompleteOrderCommandHandlerTests`: Testa finalização de pedido com baixa de estoque atômica
  - `RegisterUserCommandHandlerTests`: Testa registro de usuário, hash de senha
  - `LoginCommandHandlerTests`: Testa autenticação, geração de token JWT
  
  #### Testes de Services
  
  - `StockServiceTests`: Testa lógica de verificação de disponibilidade, reserva, liberação, baixa de estoque
  - `ShippingCalculationServiceTests`: Testa cálculo de frete com regras de negócio (frete grátis, múltiplas transportadoras)
  
- ✅ **Testes de Integração** (19+ testes):
  
  **Gestão de Estoque:**
  - `StockManagementIntegrationTests`: 
    - Fluxo completo de criação (StockOffice → Color → Size → Product → SKU → Stock)
    - Testa criação, listagem, reserva, baixa e atualização de estoque
    - Validação de duplicidade (SKU + Filial)
    - Filtros de listagem (por SkuId, StockOfficeId)
    - Verificação de inclusão de propriedades de navegação (StockOffice, Sku) nas respostas
  
  **Pedidos:**
  - `OrderCreationFlowTests`: Fluxo completo de criação de pedidos com validação de estoque
  - `EndToEndOrderFlowTests`: Teste end-to-end completo (criação, atualização, cancelamento)
  - `CompleteOrderIntegrationTests`: Testa finalização de pedido com baixa de estoque atômica
  - `OrderCreationFlowTests`: Testa criação de pedido com múltiplos itens
  
  **Infraestrutura:**
  - `DatabaseIntegrationTests`: Testa multitenancy, transações, isolamento de dados
  - `ConcurrencyIntegrationTests`: Testa controle de concorrência otimista com RowVersion
  - `MessagingIntegrationTests`: Testa publicação/consumo de mensagens RabbitMQ, DLQ, idempotência
  
  **Autenticação:**
  - `AuthIntegrationTests`: Testa registro, login, validação de token JWT
  
  **Integrações Externas:**
  - `ShippingIntegrationTests`: Testa cálculo de frete com integração externa
  - `PaymentWebhookIntegrationTests`: Testa webhook de pagamento, atualização automática de status
  
- ✅ **Qualidade dos Testes**:
  - Uso de FluentAssertions para assertions legíveis
  - Mocks com Moq para dependências externas
  - Padrão AAA (Arrange, Act, Assert)
  - Testes isolados e independentes
  - Cobertura de casos de sucesso e falha
  - Testes de validação de regras de negócio
  - Testes de integração com banco de dados real (PostgreSQL via Docker)

---

## 🔍 Code Review

**Arquivo**: `CODE_REVIEW.md`.

---

## 📊 Diagramas

O projeto inclui diagramas principais em formato PNG para visualização direta.

### Diagrama de Arquitetura
- **Arquivo**: `docs/architecture-diagram.png`
- **Conteúdo**:
  - Visão geral da arquitetura Clean Architecture/DDD
  - Camadas: Frontend, API, Application, Domain, Infrastructure, Messaging
  - Componentes principais e suas interações
  - Fluxo de dependências entre camadas
  - Integrações externas (ViaCEP, Shipping API, Payment Gateway)
  - Serviços de infraestrutura (PostgreSQL, Redis, RabbitMQ)

### Diagramas de Sequência
- **Arquivo**: `docs/sequence-diagrams.png`
- **Conteúdo**: Fluxos principais do sistema:
  1. **Criação de Pedido**: Validação de estoque, criação de pedido, publicação de eventos, notificação SignalR
---

## 🚀 Deploy

### Docker

```bash
docker build -t order-management-api .
docker run -p 5000:80 order-management-api
```

### Kubernetes

O projeto inclui manifests Kubernetes completos para deploy em cluster:

**1. Aplicar ConfigMap:**
```bash
kubectl apply -f k8s/configmap.yaml
```

**2. Criar Secrets:**
```bash
# Copie o arquivo de exemplo e preencha com seus valores
cp k8s/secret.yaml.example k8s/secret.yaml
# Edite k8s/secret.yaml com seus valores reais
kubectl apply -f k8s/secret.yaml
```

**3. Deploy da API:**
```bash
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

**4. Deploy do Frontend:**
```bash
kubectl apply -f k8s/frontend-deployment.yaml
kubectl apply -f k8s/frontend-service.yaml
```

**5. Verificar status:**
```bash
kubectl get pods
kubectl get services
kubectl get deployments
```

**Arquivos Kubernetes incluídos:**
- `configmap.yaml` - Configurações da aplicação
- `secret.yaml.example` - Template de secrets (JWT, senhas)
- `deployment.yaml` - Deployment da API .NET
- `service.yaml` - Service da API
- `frontend-deployment.yaml` - Deployment do Frontend Vue
- `frontend-service.yaml` - Service do Frontend

**Nota**: Antes de aplicar os manifests, certifique-se de:
1. Ter as imagens Docker disponíveis no registry (ou ajustar `imagePullPolicy` para `Never` se usar imagens locais)
2. Configurar os secrets com valores reais
3. Ajustar as configurações de conexão no ConfigMap conforme seu ambiente

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
- **Sistema de reserva de estoque**: Reservar estoque ao criar pedido, liberar se cancelado
- **Relatórios de estoque**: Dashboard com movimentações, estoque mínimo, alertas
- **Transferência entre filiais**: Mover estoque entre filiais
- **Histórico de movimentações**: Auditoria completa de todas as operações de estoque
- **Geração de códigos de barras**: Sistema já implementa EAN-13, pode ser estendido para outros formatos (EAN-8, UPC, Code128)

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
- **Separação em Microsserviços**: 
  - **Contexto**: O sistema atual é monolítico modular, o que facilita manutenção mas pode limitar escalabilidade independente
  - **Benefícios esperados**:
    - Escalabilidade independente por serviço (ex: serviço de estoque pode escalar separadamente do serviço de pedidos)
    - Deploy independente de funcionalidades
    - Tecnologias diferentes por serviço (se necessário)
    - Isolamento de falhas (falha em um serviço não derruba todo o sistema)
  - **Serviços candidatos para separação**:
    - **Order Service**: Gerenciamento de pedidos
    - **Stock Service**: Gestão de estoque (Product, SKU, Stock, StockOffice)
    - **Catalog Service**: Catálogo de produtos (Product, Color, Size)
    - **Auth Service**: Autenticação e autorização
    - **Notification Service**: Notificações e comunicação
    - **Shipping Service**: Cálculo de frete
    - **Payment Service**: Processamento de pagamentos
  - **Desafios a considerar**:
    - Comunicação entre serviços (síncrona via HTTP/REST ou assíncrona via mensageria)
    - Consistência distribuída (Saga Pattern, Event Sourcing)
    - Observabilidade distribuída (tracing, logging centralizado)
    - Gerenciamento de transações distribuídas
    - Complexidade operacional (múltiplos deploys, monitoramento)
  - **Estratégia de migração**: Migração gradual usando Strangler Fig Pattern
- **API Gateway**: Centralizar roteamento e políticas

---

## 🚀 Estratégias de Escalabilidade (Monolito Modular)

O projeto atual utiliza uma arquitetura **monolítica modular** (Clean Architecture/DDD), que pode ser escalada significativamente sem necessidade imediata de migração para microsserviços. A arquitetura Clean Architecture/DDD já implementada facilita várias estratégias de escalabilidade. Abaixo estão estratégias práticas e imediatamente aplicáveis:

### 1. Escalabilidade Horizontal da Aplicação

#### Load Balancing
- **Implementação**: Usar Nginx, HAProxy ou Azure Load Balancer na frente de múltiplas instâncias da API
- **Benefício**: Distribui carga entre instâncias, aumenta throughput e disponibilidade
- **Configuração sugerida**:
  ```nginx
  upstream order_management {
      least_conn;  # Balanceamento por menor conexão
      server api1:5000;
      server api2:5000;
      server api3:5000;
  }
  ```

#### Containerização e Orquestração
- **Docker Swarm ou Kubernetes**: Executar múltiplas réplicas da aplicação
- **Auto-scaling**: Configurar HPA (Horizontal Pod Autoscaler) no Kubernetes baseado em CPU/memória
- **Exemplo Kubernetes**:
  ```yaml
  apiVersion: apps/v1
  kind: Deployment
  spec:
    replicas: 3
    template:
      spec:
        containers:
        - name: order-management-api
          resources:
            requests:
              cpu: 500m
              memory: 512Mi
            limits:
              cpu: 2000m
              memory: 2Gi
  ```

### 2. Otimização de Banco de Dados

#### Connection Pooling
- **Configuração EF Core**: Otimizar tamanho do pool de conexões
  ```csharp
  services.AddDbContext<OrderManagementDbContext>(options =>
      options.UseNpgsql(connectionString, npgsqlOptions =>
      {
          npgsqlOptions.MaxPoolSize(100);  // Ajustar conforme necessidade
          npgsqlOptions.MinPoolSize(10);
      }));
  ```

#### Read Replicas (PostgreSQL)
- **Estratégia**: Separar leituras e escritas
- **Implementação**: 
  - Múltiplas réplicas de leitura
  - Usar `DbContext` separado para queries (read-only)
  - Aplicar automaticamente em `GetOrdersQueryHandler` e outras queries
- **Benefício**: Reduz carga no banco principal, melhora performance de consultas

#### Particionamento (Sharding)
- **Por Tenant**: Cada tenant em banco/schema separado (já tem multitenancy)
- **Por Data**: Particionar tabelas grandes (Orders) por período (mensal/trimestral)
- **Por Região**: Separar por região geográfica se aplicável

#### Índices Estratégicos
- **Análise de queries**: Usar `EXPLAIN ANALYZE` no PostgreSQL
- **Índices compostos**: Criar índices para queries frequentes
  ```sql
  CREATE INDEX idx_orders_customer_status_date 
  ON "Orders" ("CustomerId", "Status", "CreatedAt");
  
  CREATE INDEX idx_stock_sku_office_available 
  ON "Stocks" ("SkuId", "StockOfficeId", "Quantity", "Reserved");
  ```

### 3. Cache Distribuído (Redis)

#### Cache de Consultas
- **Já implementado**: `GetOrdersQueryHandler` usa Redis
- **Expandir para**:
  - Cache de produtos, cores, tamanhos (dados pouco mutáveis)
  - Cache de cálculos de frete
  - Cache de SKUs com estoque disponível
- **Estratégia Cache-Aside**: Aplicação gerencia cache manualmente

#### Cache de Sessão
- **JWT em Redis**: Armazenar tokens revogados (blacklist)
- **Sessões de usuário**: Cache de dados de usuário autenticado

#### Cache de Resultados Complexos
- **Agregações**: Cache de relatórios e dashboards
- **TTL inteligente**: Cache de 5-15 minutos para dados que mudam pouco

### 4. Processamento Assíncrono (RabbitMQ)

#### Expandir Uso de Mensageria
- **Já implementado**: Eventos de domínio publicados via RabbitMQ
- **Oportunidades**:
  - Processar criação de pedidos de forma assíncrona (após validação inicial)
  - Cálculo de frete assíncrono
  - Geração de relatórios em background
  - Envio de emails/notificações assíncrono

#### Workers Dedicados
- **Separar consumers**: Executar consumers em processos/containers separados
- **Escalabilidade independente**: Escalar workers sem escalar API
- **Exemplo**:
  ```yaml
  # Kubernetes: API e Workers separados
  - Deployment: order-management-api (3 réplicas)
  - Deployment: order-consumers (5 réplicas)
  - Deployment: notification-workers (2 réplicas)
  ```

### 5. Otimização de Código e Queries

#### Queries Eficientes
- **Eager Loading**: Usar `Include()` adequadamente para evitar N+1
- **Projeções**: Retornar apenas campos necessários (DTOs já fazem isso)
- **Pagination**: Sempre paginar listagens grandes (já implementado)

#### Processamento em Lote
- **Já implementado**: `ProcessOrdersBatchCommandHandler`
- **Expandir para**: 
  - Processamento em lote de atualizações de estoque
  - Importação de produtos em massa
  - Geração de relatórios em lote

#### Paralelização
- **Task.WhenAll**: Usar para operações independentes (já usado em batch)
- **Async/Await**: Garantir que todas operações I/O sejam assíncronas

### 6. CDN e Assets Estáticos

#### Frontend
- **CDN para assets**: Servir Vue.js build via CDN (CloudFlare, AWS CloudFront)
- **Cache de assets**: Headers de cache apropriados

#### API Responses
- **Compressão**: Habilitar gzip/brotli no servidor web
- **HTTP/2**: Suportar HTTP/2 para multiplexing

### 7. Monitoramento e Observabilidade

#### Métricas
- **Application Insights / Prometheus**: Coletar métricas de performance
- **Alertas**: Configurar alertas para latência alta, erro rate, etc.

#### Logging Estruturado
- **Já implementado**: Serilog
- **Centralizar**: Enviar logs para ELK Stack ou similar
- **Correlation IDs**: Rastrear requisições através de serviços

#### Health Checks
- **Já implementado**: Endpoint `/health`
- **Expandir**: Health checks mais granulares (banco, RabbitMQ, Redis)

### 8. Otimização de Infraestrutura

#### Database Tuning
- **PostgreSQL**: Ajustar `shared_buffers`, `work_mem`, `maintenance_work_mem`
- **Vacuum automático**: Configurar autovacuum adequadamente
- **Connection limits**: Ajustar `max_connections` conforme pool da aplicação

#### Resource Limits
- **CPU/Memória**: Definir limites apropriados em containers
- **Evitar over-provisioning**: Monitorar uso real e ajustar

### 9. Estratégias de Escalabilidade por Componente

#### Pedidos (Alto Volume)
- **Particionamento**: Sharding por data ou tenant
- **Arquivamento**: Mover pedidos antigos para storage frio
- **Read Replicas**: Todas consultas em réplicas

#### Estoque (Alta Concorrência)
- **Otimistic Locking**: Já implementado com `RowVersion`
- **Cache de disponibilidade**: Cache de estoque disponível por SKU
- **Queue para atualizações**: Processar atualizações de estoque via fila

#### Autenticação (Alto Tráfego)
- **JWT stateless**: Já implementado (escalável)
- **Rate limiting**: Já implementado, pode ser mais granular
- **Cache de usuários**: Cache de dados de usuário em Redis

### 10. Limites e Quando Considerar Microsserviços

#### Limites do Monolito Modular
- **Escalabilidade**: ~10-50 instâncias da aplicação (depende da complexidade)
- **Deploy**: Deploy único afeta todo sistema
- **Tecnologia**: Limitado a stack .NET

#### Sinais para Migração
- Necessidade de escalar componentes específicos independentemente
- Equipes grandes trabalhando em paralelo (conflitos de deploy)
- Necessidade de tecnologias diferentes
- Isolamento de falhas crítico

### 11. Plano de Implementação Sugerido

**Fase 1 (Curto Prazo - 1-3 meses)**:
1. ✅ Implementar load balancing (Nginx/HAProxy)
2. ✅ Configurar múltiplas réplicas no Kubernetes
3. ✅ Expandir cache Redis para mais endpoints
4. ✅ Otimizar queries com índices estratégicos
5. ✅ Configurar read replicas do PostgreSQL

**Fase 2 (Médio Prazo - 3-6 meses)**:
1. ✅ Separar workers de mensageria
2. ✅ Implementar particionamento de tabelas grandes
3. ✅ Expandir processamento assíncrono
4. ✅ Melhorar observabilidade (tracing, métricas)

**Fase 3 (Longo Prazo - 6-12 meses)**:
1. ✅ Avaliar necessidade de microsserviços
2. ✅ Se necessário, iniciar migração gradual (Strangler Fig)

### Conclusão

A arquitetura monolítica modular atual **pode escalar significativamente** (suportar milhões de requisições/dia) com as otimizações acima, **sem necessidade imediata de microsserviços**. A migração para microsserviços deve ser considerada apenas quando:
- Os limites do monolito forem atingidos
- Houver necessidade clara de escalabilidade independente
- Os benefícios superarem os custos de complexidade operacional

### Estratégias Práticas e Imediatas

#### 1. Escalabilidade Horizontal

**O que fazer:**
```yaml
# docker-compose.yml - Múltiplas instâncias
services:
  api:
    build: .
    deploy:
      replicas: 5  # 5 instâncias da mesma aplicação
    environment:
      - ConnectionStrings__DefaultConnection=...
```

**Resultado**: 5x mais capacidade de processamento com zero mudança de código.

**Como funciona**: Load balancer distribui requisições entre instâncias. Cada instância é stateless (JWT, sem sessão), então qualquer instância pode atender qualquer requisição.

**Limitação**: Banco de dados pode se tornar gargalo. Solução: Read Replicas (próximo item).

---

#### 2. Read Replicas

**O que fazer:**
```csharp
// Program.cs - Configurar múltiplos DbContext
services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseNpgsql(connectionStringWrite)); // Banco principal (writes)

services.AddDbContext<OrderManagementReadDbContext>(options =>
    options.UseNpgsql(connectionStringRead)); // Read replica (reads only)
```

**Modificar handlers de Query:**
```csharp
// GetOrdersQueryHandler.cs
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResultDto<OrderDto>>
{
    private readonly OrderManagementReadDbContext _readContext; // Usa read replica
    
    // Todas queries usam read replica
    // Commands continuam usando write context
}
```

**Resultado**: 
- Escalabilidade de leitura independente (10+ réplicas)
- Reduz carga no banco principal em 80-90%
- Zero impacto em escritas

**Custo**: Configuração de PostgreSQL streaming replication (nativo, sem custo adicional).

---

#### 3. Cache Distribuído

**Expandir uso atual:**
```csharp
// Já existe em GetOrdersQueryHandler
// Adicionar em mais handlers:

// GetProductByIdQueryHandler.cs
public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
{
    string cacheKey = $"product:{request.Id}";
    var cached = await _cache.GetStringAsync(cacheKey);
    if (cached != null) return JsonSerializer.Deserialize<ProductDto>(cached);
    
    var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
    var dto = _mapper.Map<ProductDto>(product);
    
    await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), 
        new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });
    
    return dto;
}
```

**Onde aplicar:**
- ✅ Produtos (mudam pouco)
- ✅ Cores, Tamanhos (mudam raramente)
- ✅ SKUs (mudam pouco)
- ✅ Cálculos de frete (cache por CEP)

**Resultado**: 
- Reduz queries ao banco em 70-90%
- Latência reduzida de 50-200ms para 1-5ms
- Menor carga no banco

---

#### 4. Processamento Assíncrono

**O que já existe:**
- ✅ Domain Events publicados via RabbitMQ
- ✅ Consumers para processamento assíncrono

**Expandir para:**
```csharp
// CreateOrderCommandHandler.cs - Tornar mais assíncrono
public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
{
    // 1. Validação síncrona (rápida)
    // 2. Criar pedido no banco (síncrono - necessário)
    // 3. Publicar evento (assíncrono)
    
    var order = await _orderFactory.CreateOrder(...);
    await _unitOfWork.Orders.AddAsync(order, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    // Processamento pesado em background
    await _eventDispatcher.DispatchAsync(new OrderCreatedEvent(order.Id));
    
    return _mapper.Map<OrderDto>(order);
}

// OrderCreatedConsumer.cs - Processar em background
public async Task ConsumeAsync(OrderCreatedEvent message)
{
    // Cálculo de frete (pode ser pesado)
    // Envio de email
    // Atualização de relatórios
    // Integração com marketplaces
}
```

**Resultado**:
- API responde mais rápido (não espera processamento pesado)
- Workers podem escalar independentemente
- Melhor experiência do usuário

---

#### 5. Particionamento por Tenant

**O que já existe:**
- ✅ Multitenancy com `TenantId` em todas entidades
- ✅ Global query filters no EF Core

**Otimizar com Sharding:**
```csharp
// Sharding por tenant (futuro)
public class TenantAwareDbContext : OrderManagementDbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenantId = _tenantProvider.GetCurrentTenant();
        var connectionString = GetConnectionStringForTenant(tenantId);
        optionsBuilder.UseNpgsql(connectionString);
    }
}
```

**Resultado**:
- Cada tenant em banco separado (escalabilidade horizontal)
- Isolamento completo de dados
- Performance melhorada (bancos menores)

---

#### 6. Otimização de Queries

**Adicionar índices estratégicos:**
```sql
-- Já existe índice único em SkuCode
-- Adicionar índices para queries frequentes:

CREATE INDEX CONCURRENTLY idx_orders_customer_status 
ON "Orders" ("CustomerId", "Status") 
WHERE "Status" IN (1, 2, 3); -- Apenas status ativos

CREATE INDEX CONCURRENTLY idx_stock_available 
ON "Stocks" ("SkuId", "StockOfficeId", ("Quantity" - "Reserved")) 
WHERE ("Quantity" - "Reserved") > 0; -- Apenas estoque disponível

CREATE INDEX CONCURRENTLY idx_orderitems_order_sku 
ON "OrderItems" ("OrderId", "SkuId");
```

**Otimizar queries com projeções:**
```csharp
// Já implementado com DTOs, mas pode melhorar:
var orders = await _context.Orders
    .Where(o => o.CustomerId == customerId)
    .Select(o => new OrderDto  // Projeção direta (não carrega entidade completa)
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        TotalAmount = o.TotalAmount,
        // ... apenas campos necessários
    })
    .ToListAsync();
```

**Resultado**:
- Queries 10-100x mais rápidas
- Menor uso de memória
- Menor carga no banco

---

#### 7. Connection Pooling Otimizado

**Configuração atual pode ser otimizada:**
```csharp
// Program.cs
services.AddDbContext<OrderManagementDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MaxPoolSize(100);  // Aumentar conforme necessidade
        npgsqlOptions.MinPoolSize(10);   // Manter conexões ativas
        npgsqlOptions.CommandTimeout(30); // Timeout adequado
    }));
```

**Resultado**:
- Reutilização de conexões (menos overhead)
- Melhor performance em alta concorrência

---

### Capacidade de Escala do Formato Atual

**Com as estratégias acima, o monolito modular pode suportar:**

| Métrica | Capacidade |
|---------|-----------|
| **Requisições/segundo** | 5.000 - 50.000+ (com load balancer + múltiplas instâncias) |
| **Usuários simultâneos** | 10.000 - 100.000+ |
| **Pedidos/dia** | 1 milhão - 10 milhões+ |
| **Instâncias da aplicação** | 10 - 100+ (horizontal scaling) |
| **Read replicas** | Ilimitadas (PostgreSQL suporta 10+ réplicas) |

**Limitações do formato atual:**
- ❌ Escalabilidade independente por componente (ex: escalar apenas estoque)
- ❌ Deploy independente (deploy afeta todo sistema)
- ❌ Tecnologia única (.NET)

**Quando migrar para microsserviços:**
- Quando atingir limites acima E precisar escalar componentes independentemente
- Quando equipes grandes precisarem de deploys independentes
- Quando precisar de tecnologias diferentes por componente

### Resumo: O formato atual JÁ permite escalabilidade!

✅ **Escalabilidade horizontal**: Múltiplas instâncias  
✅ **Escalabilidade de leitura**: Read replicas  
✅ **Cache distribuído**: Redis (já implementado)  
✅ **Processamento assíncrono**: RabbitMQ (já implementado)  
✅ **Otimização de queries**: Índices, projeções  
✅ **Multitenancy**: Sharding por tenant (futuro)  

**Conclusão**: O projeto pode escalar para milhões de requisições/dia **mantendo a estrutura atual**, sem necessidade imediata de microsserviços. A migração deve ser considerada apenas quando os limites acima forem atingidos ou quando houver necessidade específica de escalabilidade independente por componente.

### Débitos Técnicos Conhecidos

#### 1. Frontend
- **Interface básica**: Dashboard Vue 3 funcional mas pode ser melhorado com:
  - Mais funcionalidades de visualização
  - Filtros avançados na interface
  - Gráficos e relatórios
  - Melhor tratamento de erros e loading states

#### 2. Código
- **Melhorias em serviços**: `ShippingCalculationService` pode ser expandido com integrações reais de APIs de frete
- **Validações adicionais**: Algumas validações de negócio poderiam ser mais robustas

#### 3. Infraestrutura
- **CI/CD mais completo**: Adicionar stages de deploy, rollback automático
- **Ambientes de staging**: Ambiente de homologação antes de produção
- **Disaster recovery**: Plano de recuperação de desastres
- **Preparação para Microsserviços**: 
  - **Débito técnico atual**: Sistema monolítico modular facilita manutenção mas limita escalabilidade independente
  - **Impacto**: 
    - Todos os serviços compartilham a mesma infraestrutura (banco, mensageria)
    - Deploy único afeta todas as funcionalidades
    - Escalabilidade é "tudo ou nada"
  - **Preparação necessária**:
    - Identificar bounded contexts claros (já parcialmente feito com DDD)
    - Definir contratos de API entre contextos
    - Implementar comunicação assíncrona robusta (já tem RabbitMQ)
    - Preparar observabilidade distribuída
    - Documentar estratégia de migração

---

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
