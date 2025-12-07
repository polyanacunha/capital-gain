using CapitalGains.Domain.Entities;

namespace CapitalGains.Domain.Services;

public interface ICapitalGainsCalculator
{
    IReadOnlyList<decimal> CalculateTaxes(IReadOnlyList<Operation> operations);
}


