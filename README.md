# ?? FCG Payments - Microserviço de Pagamentos

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)
![Worker Service](https://img.shields.io/badge/Worker-Service-blue?style=flat-square)
![MassTransit](https://img.shields.io/badge/MassTransit-8.5.7-orange?style=flat-square)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Message%20Bus-red?style=flat-square)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-yellow?style=flat-square)

## ?? Índice

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

## ?? Sobre o Projeto

O **FCG Payments** é um microserviço de processamento de pagamentos desenvolvido em **.NET 8** utilizando o padrão **Worker Service**. Ele faz parte de uma arquitetura de microserviços orientada a eventos, responsável por processar transações de pagamento com cartão de crédito de forma assíncrona.

### Finalidade

- **Processar pagamentos** de pedidos recebidos via message broker (RabbitMQ)
- **Gerenciar transações** de cartão de crédito com retry automático
- **Persistir dados** de pagamentos e transações em banco SQL Server
- **Publicar eventos** de pagamentos processados para outros microserviços
- **Garantir resiliência** com políticas de retry e tratamento de falhas

## ??? Arquitetura

O microserviço segue os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**, com separação clara de responsabilidades:

```
???????????????????????????????????????????????????
?           FCG.Payments Service                  ?
???????????????????????????????????????????????????
                      ?
            ?????????????????????
            ?                   ?
      ???????????         ???????????
      ? RabbitMQ?         ?SQL Server?
      ?(Msg Bus)?         ?(Database)?
      ???????????         ???????????
            ?
      ??????????????
      ?            ?
????????????  ????????????
? Orders   ?  ? Payments ?
? Service  ?  ? Service  ?
????????????  ????????????
```

### Camadas

- **Application**: Handlers de eventos e orquestração
- **Domain**: Entidades, enums e contratos de domínio
- **Data**: Contexto EF Core, repositórios e mapeamentos
- **Services**: Lógica de negócio de processamento de pagamentos
- **Facade**: Abstração do provedor de pagamento
- **Consumers**: Consumidores de mensagens do RabbitMQ

## ? Funcionalidades

### 1. Processamento de Pagamentos
- Consumo de eventos `OrderPlacedEvent` do RabbitMQ
- Processamento de pagamentos com cartão de crédito
- Tentativas automáticas (até 3x) em caso de falha
- Validação de pagamentos já processados (idempotência)

### 2. Gerenciamento de Transações
- Criação de transações com status (Authorized/Denied)
- Armazenamento de detalhes da transação (NSU, TID, código de autorização)
- Histórico completo de tentativas

### 3. Persistência de Dados
- Banco de dados SQL Server com Entity Framework Core
- Migrations automáticas com retry na inicialização
- Unit of Work para garantir consistência transacional

### 4. Mensageria
- Consumo de eventos `OrderPlacedEvent`
- Publicação de eventos `PaymentProcessedEvent`
- Política de retry configurável para mensagens
- Dead Letter Queue automática (MassTransit)

### 5. Integração com Provedor de Pagamento
- Integração com `FCG.FakePaymentProvider` (simulador)
- Criptografia de dados do cartão (CardHash)
- Suporte a múltiplas bandeiras de cartão

## ??? Tecnologias

### Core
- **.NET 8.0** - Framework principal
- **C# 12** - Linguagem de programação
- **Worker Service** - Template para serviços em background

### Bibliotecas
- **MassTransit 8.5.7** - Framework de mensageria
- **MassTransit.RabbitMQ** - Integração com RabbitMQ
- **Entity Framework Core 8.0** - ORM
- **MediatR** - Mediator pattern para eventos de domínio
- **SQL Server** - Banco de dados relacional

### Infraestrutura
- **RabbitMQ** - Message broker
- **SQL Server** - Banco de dados
- **Docker** - Containerização

## ?? Pré-requisitos

### Ambiente de Desenvolvimento
- .NET SDK 8.0 ou superior
- SQL Server 2019+ ou LocalDB
- RabbitMQ 3.x
- Visual Studio 2022 / VS Code / Rider

### Ambiente Docker
- Docker 20.10+
- Docker Compose 2.0+

## ?? Configuração

### 1. Clone o Repositório

```bash
git clone https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI.git
cd App.Payments
```

### 2. Configure o appsettings.json

Crie ou edite o arquivo `src/FCG.Payments/appsettings.json`:

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

### 3. Configure Variáveis de Ambiente (Produção)

Para ambientes de produção, utilize variáveis de ambiente em vez de appsettings:

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

# Windows (PowerShell)
$env:ConnectionStrings__Core="Server=sqlserver;Database=FGC.Payments;..."
$env:RabbitMQ__Host="rabbitmq"
```

## ?? Variáveis de Ambiente

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

### Configuração via Docker Compose

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

## ?? Execução

### Desenvolvimento Local

```bash
# Restaurar dependências
dotnet restore

# Aplicar migrations
cd src/FCG.Payments
dotnet ef database update

# Executar o serviço
dotnet run
```

### Docker

```bash
# Build da imagem
docker build -t fcg-payments:latest -f src/FCG.Payments/Dockerfile .

# Executar container
docker run -d \
  -e ConnectionStrings__Core="Server=sqlserver;..." \
  -e RabbitMQ__Host="rabbitmq" \
  --name fcg-payments \
  fcg-payments:latest
```

### Docker Compose (Recomendado)

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  rabbitmq:
    image: rabbitmq:3-management
    environment:
      - RABBITMQ_DEFAULT_USER=admin
      - RABBITMQ_DEFAULT_PASS=admin123
    ports:
      - "5672:5672"
      - "15672:15672"

  fcg-payments:
    build:
      context: .
      dockerfile: src/FCG.Payments/Dockerfile
    environment:
      - ConnectionStrings__Core=Server=sqlserver;Database=FGC.Payments;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Username=admin
      - RabbitMQ__Password=admin123
      - PaymentConfig__DefaultApiKey=your-api-key-16chars
      - PaymentConfig__DefaultEncryptionKey=your-encrypt-16chars
    depends_on:
      - sqlserver
      - rabbitmq

volumes:
  sqlserver-data:
```

```bash
# Subir todos os serviços
docker-compose up -d

# Ver logs
docker-compose logs -f fcg-payments

# Parar serviços
docker-compose down
```

## ?? Estrutura do Projeto

```
FCG.Payments/
??? src/
?   ??? FCG.Payments/                    # Microserviço principal
?   ?   ??? Application/
?   ?   ?   ??? Handlers/                # Event handlers (MediatR)
?   ?   ??? Consumers/                   # Consumidores RabbitMQ
?   ?   ??? Data/
?   ?   ?   ??? Mappings/                # Configurações EF Core
?   ?   ?   ??? Repositories/            # Implementação de repositórios
?   ?   ?   ??? PaymentContext.cs        # DbContext
?   ?   ?   ??? UnitOfWork.cs            # Unit of Work
?   ?   ??? Domain/
?   ?   ?   ??? Contracts/               # Domain events
?   ?   ?   ??? Extensions/              # Extension methods
?   ?   ??? Facade/                      # Gateway para provedor de pagamento
?   ?   ??? Migrations/                  # EF Core Migrations
?   ?   ??? Models/
?   ?   ?   ??? Enums/                   # Enumeradores
?   ?   ?   ??? Interfaces/              # Interfaces de repositório
?   ?   ?   ??? Payment.cs               # Entidade de pagamento
?   ?   ?   ??? Transaction.cs           # Entidade de transação
?   ?   ?   ??? CreditCard.cs            # Value Object
?   ?   ??? Services/                    # Serviços de domínio
?   ?   ??? Settings/                    # Configurações e DI
?   ?   ??? Program.cs                   # Entry point
?   ?   ??? appsettings.json             # Configurações
?   ?   ??? Dockerfile                   # Container definition
?   ?
?   ??? FCG.Core/                        # Biblioteca compartilhada
?   ?   ??? Data/                        # Interfaces de dados
?   ?   ??? DomainObjects/               # Objetos de domínio base
?   ?   ??? Mediator/                    # Mediator handler
?   ?   ??? Messages/                    # Eventos de integração
?   ?   ??? Objects/                     # Objetos compartilhados
?   ?
?   ??? FCG.FakePaymentProvider/         # Simulador de gateway
?       ??? CardHash.cs
?       ??? FakePaymentService.cs
?       ??? TransactionFake.cs
?
??? tests/
?   ??? FCG.Payments.Tests/              # Testes unitários
?       ??? Application/
?       ??? Consumers/
?       ??? Domain/
?       ??? Facade/
?       ??? Models/
?       ??? Services/
?       ??? README_TESTES_FCG_Payments.md
?
??? README.md                            # Este arquivo
```

## ?? Fluxo de Processamento

### 1. Recebimento de Pedido

```
???????????????
?   Orders    ? Publica evento
?   Service   ? OrderPlacedEvent
???????????????
       ?
       ?
?????????????????????????????????
?      RabbitMQ Queue           ?
?  order-placed-event-queue     ?
?????????????????????????????????
       ?
       ?
?????????????????????????????????
? OrderPlacedEventConsumer      ?
? - Consome mensagem            ?
? - Chama PaymentService        ?
?????????????????????????????????
       ?
       ?
```

### 2. Processamento de Pagamento

```
PaymentService.ProcessPayment()
    ?
    ??? Verifica pagamento existente (idempotência)
    ?   ??? Se aprovado ? Publica PaymentProcessed
    ?
    ??? Cria Payment a partir do evento
    ?
    ??? Inicia transação no banco
    ?
    ??? Loop de tentativas (até 3x)
    ?   ?
    ?   ??? PaymentFacade.ProcessPayment()
    ?   ?   ?
    ?   ?   ??? Gera CardHash (criptografia)
    ?   ?   ?
    ?   ?   ??? Cria TransactionFake
    ?   ?   ?
    ?   ?   ??? AuthorizeCardTransaction()
    ?   ?   ?   ??? Simula aprovação/negação
    ?   ?   ?
    ?   ?   ??? Retorna Transaction
    ?   ?
    ?   ??? Se autorizado ? break
    ?       Senão ? adiciona tentativa
    ?
    ??? Payment.Process(transaction)
    ?   ?
    ?   ??? Adiciona transação final
    ?   ?
    ?   ??? Publica PaymentProcessedDomainEvent
    ?   ?
    ?   ??? Atualiza status (Approved/Denied)
    ?
    ??? Salva no repositório
    ?
    ??? Commit da transação
```

### 3. Publicação de Resultado

```
PaymentProcessedDomainEvent
    ?
    ?
PaymentProcessedEventHandler
    ?
    ??? Mapeia para PaymentProcessedEvent
    ?
    ??? Publica no RabbitMQ
            ?
            ?
    ???????????????????????
    ?   RabbitMQ Queue    ?
    ?  payment-processed  ?
    ???????????????????????
               ?
               ?
    ????????????????????????
    ? Outros Microserviços ?
    ?  (Orders, etc.)      ?
    ????????????????????????
```

## ?? Eventos e Mensageria

### Eventos Consumidos

#### OrderPlacedEvent

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

### Eventos Publicados

#### PaymentProcessedEvent

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
  "amount": 150.00,
  "status": 1,
  "reason": null
}
```

**Status**:
- `1` = Approved
- `2` = Denied

**Fila RabbitMQ**: `payment-processed-event-queue`

### Política de Retry

- **Tentativas**: Configurável via `RetrySettings:MaxRetryAttempts` (padrão: 5)
- **Delay**: Configurável via `RetrySettings:DelayBetweenRetriesInSeconds` (padrão: 10s)
- **Estratégia**: Interval retry (delay fixo entre tentativas)

## ?? Testes

### Executar Testes

```bash
# Todos os testes
dotnet test

# Com coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=opencover

# Testes específicos
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

### Cobertura de Testes

O projeto possui testes unitários abrangentes para:

- ? Models (Payment, Transaction, CreditCard)
- ? Services (PaymentService)
- ? Facade (CreditCardPaymentFacade)
- ? Consumers (OrderPlacedEventConsumer)
- ? Handlers (PaymentProcessedEventHandler)
- ? Extensions (OrderEventExtensions)

Para mais detalhes, consulte: `tests/FCG.Payments.Tests/README_TESTES_FCG_Payments.md`

## ?? Modelo de Dados

### Tabelas Principais

#### Payments
| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | UNIQUEIDENTIFIER | PK, Identificador único |
| OrderId | UNIQUEIDENTIFIER | ID do pedido |
| PaymentMethod | INT | Método de pagamento (1=CreditCard) |
| Amount | DECIMAL(18,2) | Valor do pagamento |
| Status | INT | Status (0=Pending, 1=Approved, 2=Denied) |
| CardName | VARCHAR(200) | Nome no cartão |
| CardNumber | VARCHAR(16) | Número do cartão |
| CardExpirationDate | VARCHAR(7) | Validade (MM/YY) |
| CVV | VARCHAR(4) | Código de segurança |

#### Transactions
| Coluna | Tipo | Descrição |
|--------|------|-----------|
| Id | UNIQUEIDENTIFIER | PK, Identificador único |
| PaymentId | UNIQUEIDENTIFIER | FK para Payments |
| Status | INT | Status (1=Authorized, 2=Denied) |
| TotalAmount | DECIMAL(18,2) | Valor total |
| TransactionCost | DECIMAL(18,2) | Custo da transação |
| CardBrand | VARCHAR(50) | Bandeira do cartão |
| AuthorizationCode | VARCHAR(100) | Código de autorização |
| Nsu | VARCHAR(100) | NSU |
| Tid | VARCHAR(100) | TID |
| TransactionDate | DATETIME2 | Data da transação |

## ?? Segurança

### Dados Sensíveis

- **Cartões de crédito**: Armazenados apenas para fins de teste/debug
  - ?? **Produção**: Implementar tokenização/PCI-DSS
- **API Keys**: Gerenciadas via variáveis de ambiente
- **Conexões**: TLS/SSL habilitado para RabbitMQ e SQL Server

### Boas Práticas

1. Nunca commitar `appsettings.json` com dados sensíveis
2. Usar **User Secrets** em desenvolvimento
3. Usar **Azure Key Vault** ou **AWS Secrets Manager** em produção
4. Implementar **rate limiting** no gateway
5. Validar todos os inputs no Consumer

## ?? Troubleshooting

### Problema: Migrations não aplicadas

```bash
# Solução 1: Aplicar manualmente
dotnet ef database update --project src/FCG.Payments

# Solução 2: Verificar connection string
# Certificar-se que o SQL Server está acessível
```

### Problema: RabbitMQ não conecta

```bash
# Verificar se o RabbitMQ está rodando
docker ps | grep rabbitmq

# Verificar logs do RabbitMQ
docker logs <container-id>

# Testar conexão
telnet localhost 5672
```

### Problema: Pagamentos duplicados

- O sistema implementa **idempotência** verificando `OrderId`
- Se o problema persistir, verificar logs de transação

### Problema: Retry infinito

- Verificar configuração `RetrySettings:MaxRetryAttempts`
- Analisar Dead Letter Queue no RabbitMQ
- Verificar logs de erro no `PaymentService`

## ?? Monitoramento

### Logs

O serviço registra logs em diferentes níveis:

- **Information**: Eventos normais de processamento
- **Warning**: Falhas de tentativas (retries)
- **Error**: Erros críticos (migrations, conexões)

### Métricas Importantes

1. **Taxa de sucesso de pagamentos**
2. **Tempo médio de processamento**
3. **Número de retries por pagamento**
4. **Mensagens em Dead Letter Queue**
5. **Tempo de aplicação de migrations**

### Healthcheck (Recomendado implementar)

```csharp
// Adicionar ao Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentContext>()
    .AddRabbitMQ(rabbitConnectionString);
```

## ?? Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? Licença

Este projeto é parte do Tech Challenge FIAP - Turma 58

## ?? Autores

Desenvolvido pela equipe Tech Challenge FIAP 58

## ?? Suporte

Para dúvidas e suporte:
- GitHub Issues: https://github.com/Tech-Challenge-FIAP-58/PaymentsAPI/issues
- Email: [seu-email@exemplo.com]

---

**Nota**: Este é um projeto acadêmico. Para uso em produção, implementar medidas adicionais de segurança, compliance PCI-DSS e auditoria.
