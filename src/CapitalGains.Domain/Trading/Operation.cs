namespace CapitalGains.Domain.Trading;

public sealed class Operation
{
    public Operation(OperationType type, decimal unitCost, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }
        if (unitCost < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitCost), "Unit cost cannot be negative.");
        }

        Type = type;
        UnitCost = unitCost;
        Quantity = quantity;
    }

    public OperationType Type { get; }
    public decimal UnitCost { get; }
    public int Quantity { get; }
}


