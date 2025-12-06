using System.Collections.ObjectModel;

namespace CapitalGains.Domain.Trading;

public sealed class CapitalGainsCalculator
{
    private const decimal TaxRate = 0.20m;
    private const decimal ExemptionThreshold = 20000.00m;

    public IReadOnlyList<decimal> CalculateTaxes(IReadOnlyList<Operation> operations)
    {
        if (operations is null)
        {
            throw new ArgumentNullException(nameof(operations));
        }

        var taxes = new List<decimal>(capacity: operations.Count);
        var state = PortfolioState.Empty;

        foreach (var op in operations)
        {
            switch (op.Type)
            {
                case OperationType.Buy:
                    {
                        state = ApplyBuy(state, op);
                        taxes.Add(0m);
                        break;
                    }
                case OperationType.Sell:
                    {
                        var (nextState, tax) = ApplySell(state, op);
                        state = nextState;
                        taxes.Add(tax);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unsupported operation type: {op.Type}");
            }
        }

        return new ReadOnlyCollection<decimal>(taxes);
    }

    private static PortfolioState ApplyBuy(PortfolioState state, Operation op)
    {
        var currentQty = state.TotalQuantity;
        var buyQty = op.Quantity;
        var newQty = checked(currentQty + buyQty);

        if (newQty == 0)
        {
            return new PortfolioState(0, 0m, state.AccumulatedLoss);
        }

        var currentTotalCost = state.WeightedAveragePrice * currentQty;
        var buyTotalCost = op.UnitCost * buyQty;
        var newAvg = (currentTotalCost + buyTotalCost) / newQty;
        newAvg = Round2(newAvg);

        return new PortfolioState(newQty, newAvg, Round2(state.AccumulatedLoss));
    }

    private static (PortfolioState NextState, decimal Tax) ApplySell(PortfolioState state, Operation op)
    {
        var sellQty = op.Quantity;
        if (sellQty > state.TotalQuantity)
        {
            throw new InvalidOperationException("Cannot sell more shares than currently held.");
        }

        var totalValue = op.UnitCost * sellQty;
        var grossProfit = (op.UnitCost - state.WeightedAveragePrice) * sellQty;
        grossProfit = Round2(grossProfit);

        var nextQty = state.TotalQuantity - sellQty;
        var nextAvg = state.WeightedAveragePrice; // avg remains after sell
        var nextLoss = state.AccumulatedLoss;

        decimal tax = 0m;

        if (grossProfit < 0m)
        {
            // Accumulate loss regardless of operation total value
            nextLoss = Round2(nextLoss + Math.Abs(grossProfit));
        }
        else if (grossProfit > 0m)
        {
            if (totalValue <= ExemptionThreshold)
            {
                // Exempt: no tax and do NOT consume accumulated loss
                tax = 0m;
            }
            else
            {
                // Use accumulated loss to offset profit
                var applied = Math.Min(nextLoss, grossProfit);
                var taxableProfit = Round2(grossProfit - applied);
                nextLoss = Round2(nextLoss - applied);

                if (taxableProfit > 0m)
                {
                    tax = Round2(taxableProfit * TaxRate);
                }
            }
        }

        return (new PortfolioState(nextQty, nextAvg, nextLoss), tax);
    }

    private static decimal Round2(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}


