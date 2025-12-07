using CapitalGains.Application.Dtos;
using CapitalGains.Domain.Entities;

namespace CapitalGains.Application.Mappers;

public static class OperationMapper
{
    public static Operation ToDomain(OperationDto dto)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var type = ParseType(dto.Operation);
        return new Operation(type, dto.UnitCost, dto.Quantity);
    }

    private static OperationType ParseType(string value)
    {
        if (string.Equals(value, "buy", StringComparison.OrdinalIgnoreCase))
        {
            return OperationType.Buy;
        }
        if (string.Equals(value, "sell", StringComparison.OrdinalIgnoreCase))
        {
            return OperationType.Sell;
        }
        throw new ArgumentException($"Invalid operation type '{value}'. Expected 'buy' or 'sell'.", nameof(value));
    }
}


