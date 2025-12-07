## Regras de Negócio — Cálculo de IR sobre Ganho de Capital

### Objetivo
Calcular, para cada operação recebida na ordem informada, o imposto devido (IR) sobre ganho de capital em negociações de um único ativo, aplicando isenção por operação e compensação de prejuízos acumulados.

### Escopo
- Processamento sequencial de operações de compra e venda do mesmo ativo.
- Cálculo por operação (não há consolidação por mês).
- Não considera taxas, emolumentos, corretagem ou múltiplos ativos.

### Entradas e Saídas
- Entrada: lista de operações no formato JSON, cada item com:
  - `operation`: `"buy"` ou `"sell"`
  - `unit-cost`: preço unitário (decimal, ≥ 0)
  - `quantity`: quantidade (inteiro, > 0)
- Saída: lista, na mesma ordem, de objetos `{ "tax": decimal }` com o imposto devido por operação.

Exemplo de entrada e saída:

```json
[{"operation":"buy","unit-cost":10.00,"quantity":100},
 {"operation":"sell","unit-cost":20.00,"quantity":50}]
```

```json
[{"tax":0.00},
 {"tax":1000.00}]
```

### Definições
- Preço Médio Ponderado (PMP): custo médio do ativo após compras sucessivas.
- Lucro Bruto da Venda: `(preço_de_venda - PMP) * quantidade_vendida` (arredondado a 2 casas).
- Valor Total da Venda: `preço_de_venda * quantidade_vendida` (base para isenção por operação).
- Prejuízo Acumulado: soma dos prejuízos gerados em vendas anteriores ainda não compensados.

### Parâmetros de negócio
- Alíquota de IR: 20% sobre o lucro tributável.
- Isenção por operação: quando o valor total da venda é ≤ 20.000,00 (por operação).

### Regras de processamento
1) Ordem importa: as operações são processadas na sequência informada, mantendo um estado da carteira com:
   - `TotalQuantity` (quantidade total em carteira)
   - `WeightedAveragePrice` (PMP)
   - `AccumulatedLoss` (prejuízo acumulado não compensado)

2) Compra (`buy`):
   - Não gera imposto (`tax = 0`).
   - Atualiza o PMP: `novo_PMP = (custo_atual_total + custo_compra) / nova_quantidade_total`.
   - O PMP é arredondado para 2 casas decimais (MidpointAwayFromZero).

3) Venda (`sell`):
   - Validação: não é permitido vender mais do que a quantidade em carteira.
   - Calcula lucro bruto: `(preço_venda - PMP) * quantidade_vendida`, arredondado a 2 casas.
   - Atualiza `TotalQuantity` subtraindo a quantidade vendida; o PMP permanece inalterado.
   - Tratamento do resultado da venda:
     - Se houver prejuízo (lucro bruto < 0): adiciona-se o valor absoluto ao `Prejuízo Acumulado` (arredondado a 2 casas). Imposto = 0.
     - Se houver lucro (lucro bruto > 0):
       - Verifica isenção por operação: se `Valor Total da Venda` ≤ 20.000,00, imposto = 0 e não há consumo de prejuízo acumulado.
       - Caso contrário: compensa o prejuízo acumulado até o limite do lucro.
         - `lossUsed = min(AccumulatedLoss, lucro_bruto)`
         - `lucro_tributável = round2(lucro_bruto - lossUsed)`
         - Atualiza `AccumulatedLoss = round2(AccumulatedLoss - lossUsed)`
         - Se `lucro_tributável > 0`, `imposto = round2(lucro_tributável * 0.20)`; senão, `imposto = 0`.

### Arredondamentos
- Sempre para 2 casas decimais usando MidpointRounding.AwayFromZero nos pontos em que o sistema aplica arredondamento:
  - PMP após compras
  - Lucro bruto de vendas
  - Prejuízo acumulado após atualização
  - Lucro tributável e imposto resultante

### Validações e erros
- `quantity` deve ser > 0; `unit-cost` deve ser ≥ 0.
- Não é permitido vender mais do que a quantidade em carteira (erro de negócio).
- Tipos de operação válidos: `"buy"` e `"sell"` (qualquer outro valor é inválido).

### Exemplos de negócio (extraídos dos testes)
- Exemplo — lucro isento por operação:
  - Entradas: `buy 10 x 100`, `sell 15 x 50`, `sell 15 x 50`
  - Todos os impostos: `0, 0, 0` (cada venda ≤ 20.000,00).

- Exemplo — lucro tributável com alíquota de 20%:
  - Entradas: `buy 10 x 10000`, `sell 20 x 5000`
  - Lucro bruto: `(20 - 10) * 5000 = 50.000,00`; venda total = 100.000,00 (> 20.000,00) → não isento.
  - Imposto: `50.000,00 * 20% = 10.000,00` → saída `0, 10000`.

- Exemplo — compensação de prejuízo:
  - Entradas: `buy 10 x 10000`, `sell 5 x 5000` (gera prejuízo), `sell 20 x 3000` (gera lucro).
  - O lucro da segunda venda é reduzido pelo prejuízo acumulado antes de aplicar 20%.

### Limitações atuais
- Modelo considera um único ativo (não há segregação por papel/ativo).
- Isenção é avaliada por operação individual, não há fechamento mensal.
- Não há suporte a custos/ taxas operacionais.

### Referência de implementação (alto nível)
- Cálculo principal: `CapitalGains.Application/Services/CapitalGainsCalculator.cs`
- Entidades: `CapitalGains.Domain/Entities/*`
- Caso de uso: `CapitalGains.Application/UseCases/ProcessOperationsUseCase.cs`
- Formatos de entrada/saída (DTOs): `CapitalGains.Application/Dtos/*`
- Exemplos verificáveis: testes em `tests/CapitalGains.Application.Tests/*` e `tests/CapitalGains.Cli.Tests/*`


