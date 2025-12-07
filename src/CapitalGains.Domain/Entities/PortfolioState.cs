namespace CapitalGains.Domain.Entities;

public sealed class PortfolioState
{
    public static PortfolioState Empty { get; } = new(0, 0m, 0m);

    public PortfolioState(int totalQuantity, decimal weightedAveragePrice, decimal accumulatedLoss)
    {
        if (totalQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalQuantity), "Total quantity cannot be negative.");
        }
        if (weightedAveragePrice < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(weightedAveragePrice), "Weighted average cannot be negative.");
        }
        if (accumulatedLoss < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(accumulatedLoss), "Accumulated loss cannot be negative.");
        }

        TotalQuantity = totalQuantity;
        WeightedAveragePrice = weightedAveragePrice;
        AccumulatedLoss = accumulatedLoss;
    }

    public int TotalQuantity { get; }
    public decimal WeightedAveragePrice { get; }
    public decimal AccumulatedLoss { get; }
}


