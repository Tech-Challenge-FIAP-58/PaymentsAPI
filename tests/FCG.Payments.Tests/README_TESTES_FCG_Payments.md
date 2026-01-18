# 🧪 Projeto de Testes --- FCG.Payments

Este projeto contém os **testes automatizados da solução FCG.Payments**.

Ele garante a qualidade das regras de negócio, evita regressões e
fornece métricas de cobertura de código.

------------------------------------------------------------------------

## 🛠 Stack de Testes

Tecnologias utilizadas:

-   .NET 8 SDK
-   xUnit
-   Moq
-   FluentAssertions
-   Coverlet (Code Coverage)
-   ReportGenerator (Relatório HTML)

------------------------------------------------------------------------

## 🚀 Primeiros Passos (Novo Desenvolvedor)

Siga este passo a passo **apenas na primeira vez** que for rodar o
projeto na sua máquina.

### 1) Instalar .NET SDK

Baixe em:

https://dotnet.microsoft.com/download

Verifique:

dotnet --version

Resultado esperado:

8.x.x

------------------------------------------------------------------------

### 2) Clonar o repositório

git clone `<URL_DO_REPOSITORIO>`{=html}

Entrar na pasta:

cd App.Payments

------------------------------------------------------------------------

### 3) Restaurar dependências

dotnet restore

------------------------------------------------------------------------

### 4) Instalar ReportGenerator (uma vez por máquina)

dotnet tool install -g dotnet-reportgenerator-globaltool

Verificar:

reportgenerator --version

------------------------------------------------------------------------

## ▶ Executando os testes

### Via Visual Studio

Menu:

Test → Run All Tests

Atalho:

Ctrl + R, A

------------------------------------------------------------------------

### Via Terminal

dotnet test

------------------------------------------------------------------------

## 📊 Executando testes com Coverage

dotnet test --collect:"XPlat Code Coverage"

Após executar será criada a pasta:

TestResults/

Com o arquivo:

coverage.cobertura.xml

------------------------------------------------------------------------

## 🌐 Gerando relatório HTML

### Windows

reportgenerator
"-reports:TestResults\*\*`\coverage`{=tex}.cobertura.xml"
"-targetdir:coveragereport" "-reporttypes:Html"

### Linux / Mac

reportgenerator -reports:TestResults/\*\*/coverage.cobertura.xml
-targetdir:coveragereport -reporttypes:Html

------------------------------------------------------------------------

### Abrir relatório

Abra:

coveragereport/index.html

------------------------------------------------------------------------

## 🧪 O que deve ser testado

-   Domain
-   Application / UseCases
-   CommandHandlers
-   EventHandlers
-   Services com regra de negócio

------------------------------------------------------------------------

## ❌ O que NÃO deve ser testado

Já excluídos automaticamente:

-   Program.cs
-   Settings
-   Dependency Injection
-   Message Bus Config
-   Migrations
-   Infraestrutura
-   Repositórios EF

------------------------------------------------------------------------

## 🧱 Estrutura esperada

FCG.Payments.Test ├── Tests │ ├── Domain │ ├── Application │ ├──
Handlers │ ├── Services └── README.md

------------------------------------------------------------------------

## 🧪 Padrão de Testes

Utilize Arrange / Act / Assert:

Exemplo:

\[Fact\] public void ProcessPayment_WhenValid_ShouldReturnSuccess() { //
Arrange

    // Act

    // Assert

}

------------------------------------------------------------------------

## 🧹 Limpeza de cache

dotnet clean

Apagar pastas:

TestResults/ coveragereport/

------------------------------------------------------------------------

## ⚠ Problemas comuns

### ReportGenerator não encontrado

dotnet tool install -g dotnet-reportgenerator-globaltool

------------------------------------------------------------------------

### Program.cs aparecendo no coverage

Verifique se contém:

\[assembly: ExcludeFromCodeCoverage\]

------------------------------------------------------------------------

## 🚀 CI/CD

O arquivo:

coverage.cobertura.xml

é compatível com:

-   GitLab CI
-   GitHub Actions
-   Azure DevOps
-   SonarQube

------------------------------------------------------------------------

## 📌 Observação Final

Coverage é apoio, não objetivo final.

Priorize qualidade, regras de negócio e testes confiáveis.

------------------------------------------------------------------------

Equipe FCG Payments
