Capital Gains (CLI) - C#/.NET 8

Visão geral
Aplicação de linha de comando que lê simulações de operações de ações via stdin (uma lista JSON por linha) e imprime, para cada simulação, a lista de impostos (“tax”) calculados conforme as regras do desafio.

Tecnologias e arquitetura
- Linguagem/plataforma: C# (.NET 8)
- JSON: System.Text.Json
- Testes: xUnit + FluentAssertions
- Arquitetura: Clean Architecture enxuta
  - src/CapitalGains.Domain: entidades de domínio e contratos (sem IO)
  - src/CapitalGains.Application: serviços de aplicação, mapeamentos, casos de uso
  - src/CapitalGains.Cli: composition root e IO (stdin/stdout)

Decisões
- Domínio sem dependências de IO/frameworks. O cálculo reside em Application através de ICapitalGainsCalculator (contrato no Domain).
- Isenção (≤ 20000) não consome prejuízo acumulado; prejuízos sempre acumulam.
- Arredondamento financeiro: 2 casas decimais com MidpointRounding.AwayFromZero.
- CLI minimalista: nenhuma mensagem extra, apenas JSON por linha.

Como executar
Pré-requisitos: .NET 8 SDK

1) Build:
dotnet build

2) Rodar com redirecionamento de arquivo:
dotnet run --project src/CapitalGains.Cli/CapitalGains.Cli.csproj < examples/input.txt

3) Rodar interativo:
dotnet run --project src/CapitalGains.Cli/CapitalGains.Cli.csproj
[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50}]
[{"tax":0.0},{"tax":0.0}]

Como rodar os testes
dotnet test

Estrutura dos projetos
src/
  CapitalGains.Domain/
    Entities/ (Operation, OperationType, PortfolioState)
    Services/ICapitalGainsCalculator.cs
  CapitalGains.Application/
    Dtos/ (OperationDto, TaxResultDto)
    Mappers/OperationMapper.cs
    Services/CapitalGainsCalculator.cs
    UseCases/ProcessOperationsUseCase.cs
  CapitalGains.Cli/
    Program.cs
    CliRunner.cs
tests/
  CapitalGains.Application.Tests/ (unit + golden tests)
  CapitalGains.Cli.Tests/ (teste de integração usando CliRunner)
examples/
  input.txt
  expected-output.txt

Diagramas
- Arquitetura (Design): [docs/diagrams/design-diagram.png](docs/diagrams/design-diagram.png)
- Sequência: [docs/diagrams/sequence-diagram.png](docs/diagrams/sequence-diagram.png)
- Atividade: [docs/diagrams/activity-diagram.png](docs/diagrams/activity-diagram.png)

Docker (opcional)
Build da imagem:
docker build -t capital-gains-cli .

Executar com input de arquivo:
docker run --rm -i capital-gains-cli < examples/input.txt

Anonimização para envio
git archive --format=zip --output=./capital-gains.zip HEAD

Notas
- Não há dependências externas (DB, API).
- Cada linha da entrada é processada como simulação independente.
- O output contém apenas o JSON dos impostos por operação.


