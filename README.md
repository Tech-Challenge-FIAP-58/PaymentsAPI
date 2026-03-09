# FCG Payments - Microservico de Pagamentos

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)
![Worker Service](https://img.shields.io/badge/Worker-Service-blue?style=flat-square)
![MassTransit](https://img.shields.io/badge/MassTransit-8.5.7-orange?style=flat-square)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Bus-red?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-yellow?style=flat-square)
![Event Sourcing](https://img.shields.io/badge/Event%20Sourcing-Enabled-green?style=flat-square)

## Indice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Funcionalidades](#funcionalidades)
- [Event Sourcing](#event-sourcing)
- [Tecnologias](#tecnologias)
- [Pre-requisitos](#pre-requisitos)
- [Configuracao](#configuracao)
- [Variaveis de Ambiente](#variaveis-de-ambiente)
- [Execucao](#execucao)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Fluxo de Processamento](#fluxo-de-processamento)
- [Eventos e Mensageria](#eventos-e-mensageria)
- [Modelo de Dados](#modelo-de-dados)
- [Testes](#testes)
- [Seguranca](#seguranca)
- [Contribuindo](#contribuindo)

## Sobre o Projeto

O **FCG Payments** e um microservico de processamento de pagamentos desenvolvido em **.NET 8** utilizando o template **Worker Service**. Faz parte de uma arquitetura de microservicos orientada a eventos, responsavel por processar transacoes de pagamento com cartao de credito de forma assincrona.

### Finalidade

- Processar pagamentos de pedidos recebidos via message broker (RabbitMQ).
- Gerenciar transacoes de cartao de credito com retry automatico.
- Persistir dados de pagamentos e transacoes em banco SQL Server.
- Registrar o ciclo de vida completo dos pagamentos via **Event Sourcing**.
- Publicar eventos de pagamentos processados para outros microservicos.
- Garantir resiliencia com politicas de retry e tratamento de falhas.

## Arquitetura

O microservico segue principios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com separacao clara de responsabilidades e implementacao do padrao **Event Sourcing**.

### Camadas

| Camada | Responsabilidade |
|--------|------------------|
| **Domain** | Entidades, domain events, enums, regras de negocio |
| **Domain/EventSourcing** | Contratos do event store (StoredEvent, IEventStoreRepository) |
| **Application** | Handlers MediatR, servicos de aplicacao, mediator |
| **Infrastructure/Persistence** | EF Core, repositorios, PaymentContext (persiste eventos atomicamente) |
| **Infrastructure/EventSourcing** | EventStoreRepository (consultas ao event store) |
| **Facade** | Abstracao do provedor de pagamento externo |
| **Consumers** | Consumidores de mensagens RabbitMQ |

## Funcionalidades

### 1. Processamento de Pagamentos
- Consumo de eventos `OrderPlacedEvent` do RabbitMQ.
- Processamento de pagamentos com cartao de credito.
- Tentativas automaticas (ate 3x) em caso de falha no gateway.
- Validacao de pagamentos ja processados (idempotencia).

### 2. Gerenciamento de Transacoes
- Criacao de transacoes com status (Authorized / Declined).
- Armazenamento de detalhes da transacao (NSU, TID, codigo de autorizacao).
- Historico completo de tentativas.

### 3. Persistencia de Dados
- Banco de dados SQL Server com Entity Framework Core.
- Migrations automaticas com retry na inicializacao.
- Unit of Work para consistencia transacional.
- Tabela `StoredEvents` para auditoria imutavel dos domain events.

### 4. Mensageria
- Consumo de eventos `OrderPlacedEvent`.
- Publicacao de eventos `PaymentProcessedEvent` e `PaymentRefundedEvent`.
- Politica de retry configuravel para mensagens.
- Dead Letter Queue automatica (MassTransit).

### 5. Event Sourcing
- Todos os domain events sao serializados em JSON e gravados na tabela `StoredEvents`.
- Persistencia atomica: Payment + StoredEvent salvos na mesma transacao SQL.
- Publicacao no RabbitMQ somente apos confirmacao do banco.
- Ciclo de vida completo do Payment rastreado: criacao, tentativas falhas, resultado final e estorno.

### 6. Integracao com Provedor de Pagamento
- Integracao com `FCG.FakePaymentProvider` (simulador).
- Criptografia de dados do cartao (CardHash).
- Suporte a multiplas bandeiras de cartao.

## Event Sourcing

O servico implementa o padrao **Event Sourcing** para rastreabilidade completa do ciclo de vida dos pagamentos.

### Ciclo de vida do Payment

```
Payment criado       ->  PaymentCreatedDomainEvent         -> StoredEvents
Tentativa falhou     ->  PaymentAttemptFailedDomainEvent   -> StoredEvents
Aprovado / Negado    ->  PaymentProcessedDomainEvent       -> StoredEvents + RabbitMQ
Estornado            ->  PaymentRefundedDomainEvent        -> StoredEvents + RabbitMQ
```

### Onde cada evento e gerado

```csharp
// Payment.cs - construtor
AddEvent(new PaymentCreatedDomainEvent(orderId, Id, amount, paymentMethod));

// Payment.cs - AddTransaction(): cada tentativa negada
if (transaction.Status != TransactionStatus.Authorized)
    AddEvent(new PaymentAttemptFailedDomainEvent(OrderId, Id, transaction.Id, transaction.Status));

// Payment.cs - Process(): resultado final
AddEvent(new PaymentProcessedDomainEvent(...));

// Payment.cs - Refund(): valida que so aprovados podem ser estornados
if (Status != PaymentStatus.Approved) throw new DomainException(...);
AddEvent(new PaymentRefundedDomainEvent(OrderId, Id, Amount, reason));
```

### Fluxo de persistencia atomica

```
UnitOfWork.CommitAsync()
  |
  PaymentContext.SaveChangesAsync()
        |-- Coleta domain events das entidades rastreadas
        |-- Serializa cada evento -> StoredEvent (JSON)
        |-- base.SaveChangesAsync()  <- Payment + StoredEvents na mesma transacao
        |-- _mediatorHandler.PublishEvent()  <- so apos o commit do banco
```

### Tabela StoredEvents

| Campo | Tipo | Descricao |
|-------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Identificador unico do registro |
| `AggregateId` | UNIQUEIDENTIFIER | Id do agregado (ex: Payment.Id) |
| `AggregateType` | VARCHAR(100) | Tipo do agregado (ex: Payment) |
| `EventType` | VARCHAR(100) | Tipo do evento (ex: PaymentProcessedDomainEvent) |
| `Payload` | NVARCHAR(MAX) | Dados do evento serializados em JSON |
| `OccurredOn` | DATETIME2 | Timestamp UTC do evento |

### Consultando o historico

```csharp
// Por agregado (historico de um pagamento especifico)
var eventos = await _eventStoreRepository.GetEventsByAggregateId(paymentId);

// Por tipo de evento (todos os estornos, por exemplo)
var estornos = await _eventStoreRepository.GetEventsByType(nameof(PaymentRefundedDomainEvent));
```

### Aplicar migration

```bash
dotnet ef database update --project src/FCG.Payments
```

## Tecnologias

| Tecnologia | Versao | Uso |
|------------|--------|-----|
| .NET | 8.0 | Runtime |
| C# | 12 | Linguagem |
| MassTransit | 8.5.7 | Mensageria RabbitMQ |
| Entity Framework Core | 8.0.12 | ORM / Migrations |
| MediatR | 8.0 | Mediator pattern / Domain events |
| SQL Server | - | Banco de dados |
| Docker | - | Containerizacao |

## Pre-requisitos

- .NET SDK 8.0 ou superior
- SQL Server 2019+ ou LocalDB
- RabbitMQ 3.x
- Docker 20.10+ e Docker Compose 2.0+ (opcional)

## Configuracao

### 1. Clone o repositorio

```bash
git clone https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI.git
cd App.Payments
```

### 2. Configure appsettings.json

```json
{
  "ConnectionStrings": {
    "Core": "Server=localhost,1433;Database=FGC.Payments;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "VirtualHost": "/",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  },
  "PaymentConfig": {
    "DefaultApiKey": "your-api-key-here",
    "DefaultEncryptionKey": "your-encryption-key-here"
  },
  "RetrySettings": {
    "MaxRetryAttempts": 5,
    "DelayBetweenRetriesInSeconds": 10
  }
}
```

### 3. Aplicar migrations

```bash
dotnet ef database update --project src/FCG.Payments
```

## Variaveis de Ambiente

### Obrigatorias

| Variavel | Descricao |
|----------|-----------|
| `ConnectionStrings__Core` | String de conexao SQL Server |
| `RabbitMQ__Host` | Host do RabbitMQ |
| `RabbitMQ__Username` | Usuario RabbitMQ |
| `RabbitMQ__Password` | Senha RabbitMQ |
| `PaymentConfig__DefaultApiKey` | API Key do provedor de pagamento |
| `PaymentConfig__DefaultEncryptionKey` | Chave de criptografia |

### Opcionais

| Variavel | Padrao |
|----------|--------|
| `RabbitMQ__VirtualHost` | `/` |
| `RabbitMQ__Port` | `5672` |
| `RetrySettings__MaxRetryAttempts` | `5` |
| `RetrySettings__DelayBetweenRetriesInSeconds` | `10` |

## Execucao

### Desenvolvimento local

```bash
dotnet restore
dotnet ef database update --project src/FCG.Payments
dotnet run --project src/FCG.Payments
```

### Docker Compose (recomendado)

```bash
docker-compose up -d
docker-compose logs -f fcg-payments
docker-compose down
```

## Estrutura do Projeto

```
FCG.Payments/
  src/
    FCG.Payments/
      Application/
        Handlers/
          PaymentProcessedEventHandler.cs
          PaymentRefundedEventHandler.cs      # Novo
        Mediator/
          IMediatorHandler.cs
          MediatorHandler.cs
        Services/
          PaymentService.cs
      Consumers/
        OrderPlacedEventConsumer.cs
      Domain/
        Entities/
          Payment.cs                          # Modificado: dispara 4 domain events
          Transaction.cs
          CreditCard.cs
          Enums/
            PaymentStatus.cs                  # Pending/Approved/Denied/Refunded
            TransactionStatus.cs
            PaymentMethod.cs
        Events/
          PaymentCreatedDomainEvent.cs        # Novo
          PaymentAttemptFailedDomainEvent.cs  # Novo
          PaymentProcessedDomainEvent.cs
          PaymentRefundedDomainEvent.cs       # Novo
        EventSourcing/
          StoredEvent.cs                      # Novo
          IEventStoreRepository.cs            # Novo
      Facade/
        IPaymentFacade.cs
        CreditCardPaymentFacade.cs
      Infrastructure/
        EventSourcing/
          EventStoreRepository.cs             # Novo
        Persistence/
          Mappings/
            PaymentMapping.cs
            TransactionMapping.cs
            StoredEventMapping.cs             # Novo
          Repositories/
            PaymentRepository.cs
          PaymentContext.cs                   # Modificado: persistencia atomica
          UnitOfWork.cs
        Settings/
          DependencyInjectionConfig.cs        # Modificado
          MassTransitConfig.cs
      Migrations/
        ..._AddEventSourcing.cs               # Nova migration
      Program.cs

    FCG.Core/
      Integration/
        OrderPlacedEvent.cs
        PaymentProcessedEvent.cs
        PaymentRefundedEvent.cs               # Novo

    FCG.FakePaymentProvider/

  tests/
    FCG.Payments.Tests/
      Domain/
        Extensions/
          OrderEventExtensionsTests.cs
        Models/
          PaymentTests.cs                     # Atualizado: testa novos domain events
      README_TESTES_FCG_Payments.md

  README.md
```

## Fluxo de Processamento

```
[Orders API]  --publica OrderPlacedEvent-->  [RabbitMQ]
                                                  |
                                                  v
                                   [OrderPlacedEventConsumer]
                                                  |
                                                  v
                                   [PaymentService.ProcessPayment()]
                                      |
                                      |-- Idempotencia: ja aprovado? Re-publica e retorna.
                                      |-- Cria Payment -> PaymentCreatedDomainEvent (memoria)
                                      |-- Gateway (ate 3x)
                                      |       |-- Falha -> PaymentAttemptFailedDomainEvent (memoria)
                                      |-- payment.Process() -> PaymentProcessedDomainEvent (memoria)
                                      |
                                      v
                              [UnitOfWork.CommitAsync()]
                                      |
                                      v
                       [PaymentContext.SaveChangesAsync()]
                              |-- Serializa eventos -> StoredEvent (JSON)
                              |-- base.SaveChangesAsync()
                              |       |-- tabela Payments
                              |       |-- tabela Transactions
                              |       |-- tabela StoredEvents  <- atomico
                              |-- PublishEvent() -> MediatR
                                      |
                                      v
                       [PaymentProcessedEventHandler]
                                      |  MassTransit
                                      v
                                 [RabbitMQ]  ->  [Orders API]
```

## Eventos e Mensageria

### Visao geral

| Evento | Tipo | Destino | Descricao |
|--------|------|---------|-----------|
| `OrderPlacedEvent` | Integration (entrada) | RabbitMQ | Pedido realizado pelo cliente |
| `PaymentCreatedDomainEvent` | Domain | StoredEvents | Pagamento criado com status Pending |
| `PaymentAttemptFailedDomainEvent` | Domain | StoredEvents | Tentativa negada pelo gateway |
| `PaymentProcessedDomainEvent` | Domain | StoredEvents + MediatR | Resultado final (aprovado ou negado) |
| `PaymentProcessedEvent` | Integration (saida) | RabbitMQ | Notifica Orders API do resultado |
| `PaymentRefundedDomainEvent` | Domain | StoredEvents + MediatR | Pagamento estornado |
| `PaymentRefundedEvent` | Integration (saida) | RabbitMQ | Notifica Orders API do estorno |

### PaymentProcessedEvent (saida)

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "amount": 150.00,
  "status": 1,
  "reason": null
}
```

**Status**: `1` = Approved / `2` = Denied

### PaymentRefundedEvent (saida)

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "amount": 150.00,
  "reason": "Solicitado pelo cliente"
}
```

## Modelo de Dados

### Payments

| Coluna | Tipo | Descricao |
|--------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Chave primaria |
| `OrderId` | UNIQUEIDENTIFIER | Referencia ao pedido |
| `PaymentMethod` | INT | 1=CreditCard / 2=Invoice |
| `Amount` | DECIMAL | Valor total |
| `Status` | INT | 0=Pending / 1=Approved / 2=Denied / 3=Refunded |

### Transactions

| Coluna | Tipo | Descricao |
|--------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Chave primaria |
| `PaymentId` | UNIQUEIDENTIFIER | FK para Payments |
| `Status` | INT | 1=Authorized / 3=Declined / 4=Refunded / 5=Cancelled |
| `TotalAmount` | DECIMAL | Valor da transacao |
| `TransactionCost` | DECIMAL | Custo da transacao |
| `AuthorizationCode` | VARCHAR(100) | Codigo de autorizacao do gateway |
| `Nsu` | VARCHAR(100) | Numero Sequencial Unico |
| `Tid` | VARCHAR(100) | Transaction Identifier |
| `TransactionDate` | DATETIME2 | Data da transacao |

### StoredEvents

| Coluna | Tipo | Descricao |
|--------|------|-----------|
| `Id` | UNIQUEIDENTIFIER | Chave primaria |
| `AggregateId` | UNIQUEIDENTIFIER | Referencia ao Payment.Id |
| `AggregateType` | VARCHAR(100) | Tipo do agregado (ex: Payment) |
| `EventType` | VARCHAR(100) | Tipo do evento (ex: PaymentProcessedDomainEvent) |
| `Payload` | NVARCHAR(MAX) | JSON completo do evento |
| `OccurredOn` | DATETIME2 | Timestamp UTC do evento |

## Politica de Retry

- Tentativas no gateway: ate **3x** por pagamento (PaymentService).
- Retry de mensagens RabbitMQ: configuravel via `RetrySettings:MaxRetryAttempts` (padrao: 5).
- Delay: configuravel via `RetrySettings:DelayBetweenRetriesInSeconds` (padrao: 10s).

## Testes

```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"
dotnet test --filter "FullyQualifiedName~PaymentTests"
```

Consulte o [README de testes](tests/FCG.Payments.Tests/README_TESTES_FCG_Payments.md) para mais detalhes.

## Seguranca

- Cartoes de credito: armazenados apenas para testes (nao recomendado em producao).
- Producao: implementar tokenizacao/PCI-DSS.
- API Keys: gerenciadas via variaveis de ambiente.
- Nunca commitar `appsettings.json` com segredos.

## Troubleshooting

| Problema | Solucao |
|----------|---------|
| Migrations nao aplicadas | `dotnet ef database update --project src/FCG.Payments` |
| RabbitMQ nao conecta | `docker logs <container-id>` |
| Pagamentos duplicados | Verificar idempotencia pela validacao de OrderId |
| StoredEvents nao gravados | Confirmar que SaveChangesAsync e chamado via UnitOfWork |

## Contribuindo

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudancas (`git commit -m 'Add MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

Projeto academico - Tech Challenge FIAP
GitHub: https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI
