# Capital Gains CLI – Architecture

## Visão geral
- Aplicação de linha de comando (CLI) em C#/.NET 8.
- Entrada: uma lista JSON de operações por linha via stdin.
- Saída: lista JSON com `tax` por operação via stdout.
- Sem banco de dados; estado é mantido em memória por simulação (linha).

## Clean Architecture – Camadas e Responsabilidades

- Regra de Dependência (Dependency Rule)
  - Dependências sempre “apontam” para dentro. Nada fora do domínio conhece detalhes internos do domínio.
  - O domínio não depende de Application ou CLI; Application depende do domínio; CLI depende de Application.

- Camadas neste projeto
  - Domain (Core)
    - O que contém: Entidades (`Operation`, `PortfolioState`, `OperationType`) e contratos (`ICapitalGainsCalculator`).
    - Responsabilidade: modelar o problema e definir comportamentos/contratos estáveis. Sem IO, sem JSON, sem infraestrutura.
    - Por que a interface aqui: a interface é parte do modelo de negócio (“o que” precisa ser feito). A implementação concreta pode variar sem afetar quem consome o contrato.
  - Application
    - O que contém: casos de uso (`ProcessOperationsUseCase`), implementação do cálculo (`CapitalGainsCalculator`), DTOs e mapeamentos.
    - Responsabilidade: orquestrar fluxo entre DTOs ↔ domínio, aplicar regras através do contrato do domínio, coordenar objetos.
    - Depende do domínio via `ICapitalGainsCalculator` e entidades; não conhece CLI ou detalhes de IO.
  - CLI (Interface do Usuário)
    - O que contém: `Program.cs`, `CliRunner`, configuração de serialização.
    - Responsabilidade: interagir com o mundo externo (stdin/stdout), desserializar/serializar JSON e invocar o caso de uso.
    - Não conhece regras de negócio; trata apenas de IO e composição.

- Inversão de Dependência (DIP) na prática
  - Compilação
    - Application referencia Domain para usar `ICapitalGainsCalculator` e entidades.
    - CLI referencia Application e Domain (para compor a implementação concreta).
  - Execução (runtime)
    - Em `Program.cs`, criamos a implementação concreta `CapitalGainsCalculator` (Application) e a injetamos no `ProcessOperationsUseCase` (Application) que depende apenas da interface do Domain.
    - Assim, o caso de uso depende de uma abstração do domínio, não da implementação concreta.

- Fluxo típico
  1) CLI lê linha (JSON) e cria `List<OperationDto>`.
  2) CLI chama `ProcessOperationsUseCase.Handle`.
  3) Use case mapeia `OperationDto` → `Operation` (Domain).
  4) Use case chama `_calculator.CalculateTaxes(operations)` usando `ICapitalGainsCalculator`.
  5) Implementação (`CapitalGainsCalculator`) processa regras e devolve `decimal[]` (taxes).
  6) Use case transforma em `List<TaxResultDto>` e devolve ao CLI.
  7) CLI serializa para JSON e escreve no stdout.

- Benefícios
  - Testabilidade: casos de uso e calculador podem ser testados sem IO.
  - Evolução: mudar CLI (ex.: GUI, API) não toca domínio/aplicação.
  - Extensibilidade: novas estratégias de cálculo podem ser adicionadas como novas implementações de `ICapitalGainsCalculator`.

## Estrutura
- `CapitalGains.Domain` (núcleo):
  - Entities: `Operation`, `PortfolioState`, `OperationType`
  - Interfaces: `ICapitalGainsCalculator`
- `CapitalGains.Application`:
  - Services: `CapitalGainsCalculator` (implementa `ICapitalGainsCalculator`)
  - Use case: `ProcessOperationsUseCase`
  - DTOs: `OperationDto`, `TaxResultDto`
  - Mapper: `OperationMapper`
- `CapitalGains.Cli`:
  - `Program.cs` (composition root)
  - `CliRunner` (loop stdin/stdout)


## Modelo de domínio
- `Operation`:
  - `OperationType Type` (Buy|Sell)
  - `decimal UnitCost` (>= 0)
  - `int Quantity` (> 0)
- `PortfolioState` (imutável):
  - `int TotalQuantity`
  - `decimal WeightedAveragePrice`
  - `decimal AccumulatedLoss`
- `ICapitalGainsCalculator`:
  - `IReadOnlyList<decimal> CalculateTaxes(IReadOnlyList<Operation> operations)`

## Regras de Ganho de Capital (resumo)
- Imposto: 20% sobre lucro após abatimento de prejuízos acumulados.
- Compra:
  - Não taxa
  - Atualiza média ponderada:
    - `newAvg = ((qAtual * avgAtual) + (qCompra * preçoCompra)) / (qAtual + qCompra)`
- Venda:
  - `grossProfit = (preçoVenda - avg) * quantidade`
  - Se `grossProfit < 0`: acumula em `AccumulatedLoss`
  - Se `grossProfit >= 0`:
    - Se `total da venda <= 20000`: isento e NÃO consome prejuízo
    - Senão: abate `AccumulatedLoss` do lucro e taxa 20% do restante
- Arredondamento: 2 casas decimais, `MidpointRounding.AwayFromZero`
- Estado reinicia a cada linha (simulações independentes)

## Testes
- Unit (Application):
  - Casos simples (lucro, prejuízo, isenção)
  - Golden tests reproduzindo #1–#9 do enunciado
- Domain:
  - Invariantes de `Operation`, `PortfolioState`, `OperationType`
- Integração:
  - `CliRunner` em memória
  - Processo real (`dotnet` + stdin/stdout)

## Decisões
- Clean Architecture enxuta para clareza e testabilidade
- Sem frameworks extras (alinhado ao desafio)
- JSON via `System.Text.Json`
- CLI imprime apenas JSON no stdout;
