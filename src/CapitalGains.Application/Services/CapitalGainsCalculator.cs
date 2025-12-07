using System.Collections.ObjectModel;
using CapitalGains.Domain.Entities;
using CapitalGains.Domain.Services;

namespace CapitalGains.Application.Services;

public sealed class CapitalGainsCalculator : ICapitalGainsCalculator
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
        var portfolioState = PortfolioState.Empty;

        foreach (var operation in operations)
        {
            switch (operation.Type)
            {
                case OperationType.Buy:
                    {
                        portfolioState = ApplyBuy(portfolioState, operation);
                        taxes.Add(0m);
                        break;
                    }
                case OperationType.Sell:
                    {
                        var (nextState, tax) = ApplySell(portfolioState, operation);
                        portfolioState = nextState;
                        taxes.Add(tax);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unsupported operation type: {operation.Type}");
            }
        }

        return new ReadOnlyCollection<decimal>(taxes);
    }

    private static PortfolioState ApplyBuy(PortfolioState state, Operation operation)
    {
        var currentTotalQuantity = state.TotalQuantity;
        var buyQuantity = operation.Quantity;
        var newTotalQuantity = checked(currentTotalQuantity + buyQuantity);

        if (newTotalQuantity == 0)
        {
            return new PortfolioState(0, 0m, state.AccumulatedLoss);
        }

        var currentTotalCost = state.WeightedAveragePrice * currentTotalQuantity;
        var buyTotalCost = operation.UnitCost * buyQuantity;
        var newAveragePrice = (currentTotalCost + buyTotalCost) / newTotalQuantity;
        newAveragePrice = Round2(newAveragePrice);

        return new PortfolioState(newTotalQuantity, newAveragePrice, Round2(state.AccumulatedLoss));
    }

    private static (PortfolioState NextState, decimal Tax) ApplySell(PortfolioState portfolioState, Operation operation)
    {
        var sellQuantity = operation.Quantity;
        if (sellQuantity > portfolioState.TotalQuantity)
        {
            throw new InvalidOperationException("Cannot sell more shares than currently held.");
        }

        var totalSoldValue = operation.UnitCost * sellQuantity;
        var grossProfit = (operation.UnitCost - portfolioState.WeightedAveragePrice) * sellQuantity;
        grossProfit = Round2(grossProfit);

        var nextTotalQuantity = portfolioState.TotalQuantity - sellQuantity;
        var nextAveragePrice = portfolioState.WeightedAveragePrice; // avg remains after sell
        var nextAccumulatedLoss = portfolioState.AccumulatedLoss;

        decimal tax = 0m;

        if (grossProfit < 0m)
        {
            nextAccumulatedLoss = Round2(nextAccumulatedLoss + Math.Abs(grossProfit));
        }
        else if (grossProfit > 0m)
        {
            if (totalSoldValue <= ExemptionThreshold)
            {
                tax = 0m;
            }
            else
            {
                var lossUsed = Math.Min(nextAccumulatedLoss, grossProfit);
                var taxableProfit = Round2(grossProfit - lossUsed);
                nextAccumulatedLoss = Round2(nextAccumulatedLoss - lossUsed);

                if (taxableProfit > 0m)
                {
                    tax = Round2(taxableProfit * TaxRate);
                }
            }
        }

        return (new PortfolioState(nextTotalQuantity, nextAveragePrice, nextAccumulatedLoss), tax);
    }

    private static decimal Round2(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}


