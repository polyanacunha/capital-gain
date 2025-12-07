using CapitalGains.Domain.Entities;

namespace CapitalGains.Domain.Interfaces;

public interface ICapitalGainsCalculator
{
    IReadOnlyList<decimal> CalculateTaxes(IReadOnlyList<Operation> operations);
}


