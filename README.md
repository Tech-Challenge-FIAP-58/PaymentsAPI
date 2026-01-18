# FCG Payments - Microserviço de Pagamentos

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)
![Worker Service](https://img.shields.io/badge/Worker-Service-blue?style=flat-square)
![MassTransit](https://img.shields.io/badge/MassTransit-8.5.7-orange?style=flat-square)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Bus-red?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-yellow?style=flat-square)

## Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Funcionalidades](#funcionalidades)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Variáveis de Ambiente](#variáveis-de-ambiente)
- [Execução](#execução)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Fluxo de Processamento](#fluxo-de-processamento)
- [Eventos e Mensageria](#eventos-e-mensageria)
- [Testes](#testes)
- [Segurança](#segurança)
- [Contribuindo](#contribuindo)
- [Licença](#licença)
- [Autores](#autores)
- [Suporte](#suporte)

## Sobre o Projeto

O **FCG Payments** é um microserviço de processamento de pagamentos desenvolvido em **.NET 8** utilizando o template **Worker Service**. Faz parte de uma arquitetura de microserviços orientada a eventos, responsável por processar transações de pagamento com cartão de crédito de forma assíncrona.

### Finalidade

- Processar pagamentos de pedidos recebidos via message broker (RabbitMQ).
- Gerenciar transações de cartão de crédito com retry automático.
- Persistir dados de pagamentos e transações em banco SQL Server.
- Publicar eventos de pagamentos processados para outros microserviços.
- Garantir resiliência com políticas de retry e tratamento de falhas.

## Arquitetura

O microserviço segue princípios de Clean Architecture e Domain-Driven Design (DDD), com separação clara de responsabilidades.

Arquitetura simplificada:

```
+---------------------------------------------+
|               FCG.Payments Service          |
+---------------------------------------------+
                |
    +---------------------------+
    |        Infra / Broker     |
    |  RabbitMQ  <->  SQL Server|
    +---------------------------+
                |
 +----------------+     +-----------------+
 | Orders Service |     | Payments Service|
 +----------------+     +-----------------+
```

### Camadas

- **Application**: Handlers de eventos e orquestração.
- **Domain**: Entidades, enums e contratos de domínio.
- **Data**: Contexto EF Core, repositórios e mapeamentos.
- **Services**: Lógica de negócio de processamento de pagamentos.
- **Facade**: Abstração do provedor de pagamento.
- **Consumers**: Consumidores de mensagens do RabbitMQ.

## Funcionalidades

### 1. Processamento de Pagamentos
- Consumo de eventos `OrderPlacedEvent` do RabbitMQ.
- Processamento de pagamentos com cartão de crédito.
- Tentativas automáticas (configuráveis) em caso de falha.
- Validação de pagamentos já processados (idempotência).

### 2. Gerenciamento de Transações
- Criação de transações com status (Authorized/Denied).
- Armazenamento de detalhes da transação (NSU, TID, código de autorização).
- Histórico de tentativas.

### 3. Persistência de Dados
- Banco de dados SQL Server com Entity Framework Core.
- Migrations automáticas com retry na inicialização.
- Unit of Work para consistência transacional.

### 4. Mensageria
- Consumo de eventos `OrderPlacedEvent`.
- Publicação de eventos `PaymentProcessedEvent`.
- Política de retry configurável para mensagens.
- Dead Letter Queue automática (MassTransit).

### 5. Integração com Provedor de Pagamento
- Integração com `FCG.FakePaymentProvider` (simulador).
- Criptografia de dados do cartão (CardHash).
- Suporte a múltiplas bandeiras de cartão.

## Tecnologias

### Core
- .NET 8.0
- C# 12
- Worker Service

### Bibliotecas e Infra
- MassTransit 8.5.7
- MassTransit.RabbitMQ
- Entity Framework Core 8.0
- MediatR
- SQL Server
- Docker

## Pré-requisitos

### Desenvolvimento
- .NET SDK 8.0 ou superior
- SQL Server 2019+ ou LocalDB
- RabbitMQ 3.x
- Visual Studio 2022 / VS Code / Rider

### Docker
- Docker 20.10+
- Docker Compose 2.0+

## Configuração

### 1. Clone o repositório

```bash
git clone https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI.git
cd App.Payments
```

### 2. Configure `appsettings.json`

Exemplo de `src/FCG.Payments/appsettings.json`:

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
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### 3. Variáveis de ambiente (produção)

Use variáveis de ambiente em vez de commitar segredos:

```bash
# Linux/Mac
export ConnectionStrings__Core="Server=sqlserver;Database=FGC.Payments;User Id=sa;Password=..."
export RabbitMQ__Host="rabbitmq"
export RabbitMQ__Username="admin"
export RabbitMQ__Password="admin123"
export PaymentConfig__DefaultApiKey="your-api-key"
export PaymentConfig__DefaultEncryptionKey="your-encryption-key"
export RetrySettings__MaxRetryAttempts=5
export RetrySettings__DelayBetweenRetriesInSeconds=10
```

## Variáveis de Ambiente

### Obrigatórias

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `ConnectionStrings__Core` | String de conexão SQL Server | `Server=localhost;Database=FGC.Payments;User Id=sa;Password=Pass@123;TrustServerCertificate=True` |
| `RabbitMQ__Host` | Host do RabbitMQ | `localhost` ou `rabbitmq` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `PaymentConfig__DefaultApiKey` | API Key do provedor de pagamento | `your-api-key-here` |
| `PaymentConfig__DefaultEncryptionKey` | Chave de criptografia | `your-encryption-key-here` |

### Opcionais

| Variável | Descrição | Padrão | Exemplo |
|----------|-----------|--------|---------|
| `RabbitMQ__VirtualHost` | Virtual host RabbitMQ | `/` | `/payments` |
| `RabbitMQ__Port` | Porta RabbitMQ | `5672` | `5672` |
| `RetrySettings__MaxRetryAttempts` | Máximo de tentativas de retry | `5` | `10` |
| `RetrySettings__DelayBetweenRetriesInSeconds` | Delay entre retries (segundos) | `10` | `15` |
| `Logging__LogLevel__Default` | Nível de log padrão | `Information` | `Debug` |

### Exemplo Docker Compose (trecho)

```yaml
version: '3.8'
services:
  fcg-payments:
    image: fcg-payments:latest
    environment:
      - ConnectionStrings__Core=Server=sqlserver;Database=FGC.Payments;User Id=sa;Password=Pass@123;TrustServerCertificate=True
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=admin123
      - PaymentConfig__DefaultApiKey=your-api-key-16chars
      - PaymentConfig__DefaultEncryptionKey=your-encrypt-16chars
      - RetrySettings__MaxRetryAttempts=5
      - RetrySettings__DelayBetweenRetriesInSeconds=10
    depends_on:
      - sqlserver
      - rabbitmq
```

## Execução

### Desenvolvimento local

```bash
dotnet restore
cd src/FCG.Payments
dotnet ef database update
dotnet run
```

### Docker

```bash
docker build -t fcg-payments:latest -f src/FCG.Payments/Dockerfile .
docker run -d \
  -e ConnectionStrings__Core="Server=sqlserver;..." \
  -e RabbitMQ__Host="rabbitmq" \
  --name fcg-payments \
  fcg-payments:latest
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
    FCG.Payments/                    # Microserviço principal
      Application/
        Handlers/                    # Event handlers (MediatR)
      Consumers/                     # Consumidores RabbitMQ
      Data/
        Mappings/
        Repositories/
        PaymentContext.cs
        UnitOfWork.cs
      Domain/
        Contracts/
        Models/
      Facade/
      Services/
      Settings/
      Program.cs
      appsettings.json
      Dockerfile
    FCG.Core/                        # Biblioteca compartilhada
    FCG.FakePaymentProvider/         # Simulador de gateway
  tests/
    FCG.Payments.Tests/
      README_TESTES_FCG_Payments.md
  README.md
```

## Fluxo de Processamento

1. Orders Service publica `OrderPlacedEvent`.
2. Mensagem chega na fila do RabbitMQ (`order-placed-event-queue`).
3. `OrderPlacedEventConsumer` consome a mensagem e chama `PaymentService`.
4. `PaymentService` processa o pagamento, realiza tentativas, persiste transações e publica `PaymentProcessedEvent`.

## Eventos e Mensageria

### OrderPlacedEvent (exemplo)

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
  "amount": 150.00,
  "paymentMethod": 1,
  "creditCard": {
    "cardName": "JOHN DOE",
    "cardNumber": "4111111111111111",
    "cardExpirationDate": "12/25",
    "cvv": "123"
  }
}
```

**Fila RabbitMQ**: `order-placed-event-queue`

### PaymentProcessedEvent (exemplo)

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "amount": 150.00,
  "status": 1,
  "reason": null
}
```

**Status**
- `1` = Approved
- `2` = Denied

**Fila RabbitMQ**: `payment-processed-event-queue`

## Política de Retry

- Tentativas: configurável via `RetrySettings:MaxRetryAttempts` (padrão: 5).
- Delay: configurável via `RetrySettings:DelayBetweenRetriesInSeconds` (padrão: 10s).
- Estratégia: Interval retry (delay fixo entre tentativas).

## Testes

### Executar testes

```bash
dotnet test
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

## Modelo de Dados (resumo)

### Payments
- Id (UNIQUEIDENTIFIER)
- OrderId (UNIQUEIDENTIFIER)
- PaymentMethod (INT)
- Amount (DECIMAL(18,2))
- Status (INT)
- CardName (VARCHAR(200))
- CardNumber (VARCHAR(16))
- CardExpirationDate (VARCHAR(7))
- CVV (VARCHAR(4))

### Transactions
- Id (UNIQUEIDENTIFIER)
- PaymentId (UNIQUEIDENTIFIER)
- Status (INT)
- TotalAmount (DECIMAL(18,2))
- TransactionCost (DECIMAL(18,2))
- CardBrand (VARCHAR(50))
- AuthorizationCode (VARCHAR(100))
- Nsu (VARCHAR(100))
- Tid (VARCHAR(100))
- TransactionDate (DATETIME2)

## Segurança

- Cartões de crédito: armazenados apenas para testes (não recomendado em produção).
- Produção: implementar tokenização/PCI-DSS.
- API Keys: gerenciadas via variáveis de ambiente.
- Conexões: TLS/SSL para RabbitMQ e SQL Server.

Boas práticas:
1. Nunca commitar `appsettings.json` com segredos.
2. Usar User Secrets em desenvolvimento.
3. Usar Key Vault / Secrets Manager em produção.
4. Implementar rate limiting no gateway.
5. Validar todos os inputs no Consumer.

## Troubleshooting

- Migrations não aplicadas:
  - `dotnet ef database update --project src/FCG.Payments`
  - Verificar connection string e disponibilidade do SQL Server.

- RabbitMQ não conecta:
  - `docker ps | grep rabbitmq`
  - `docker logs <container-id>`
  - Testar conexão com `telnet localhost 5672`

- Pagamentos duplicados:
  - Verificar idempotência pela validação de `OrderId`.

## Monitoramento

- Logs: Information, Warning, Error.
- Métricas recomendadas:
  1. Taxa de sucesso de pagamentos
  2. Tempo médio de processamento
  3. Número de retries por pagamento
  4. Mensagens em Dead Letter Queue
  5. Tempo de aplicação de migrations

Healthcheck (exemplo):

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentContext>()
    .AddRabbitMQ(rabbitConnectionString);
```

## Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Projeto acadêmico — Tech Challenge FIAP.

## Autores

Desenvolvido pela equipe Tech Challenge FIAP.

## Suporte

- GitHub Issues: https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI/issues

---
Nota: Este é um projeto acadêmico. Para uso em produção, implemente medidas adicionais de segurança, compliance PCI-DSS e auditoria.
