using CapitalGains.Application.Dtos;
using CapitalGains.Application.Mappers;
using CapitalGains.Domain.Entities;
using CapitalGains.Domain.Services;

namespace CapitalGains.Application.UseCases;

public sealed class ProcessOperationsUseCase
{
    private readonly ICapitalGainsCalculator _calculator;

    public ProcessOperationsUseCase(ICapitalGainsCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public IReadOnlyList<TaxResultDto> Handle(IReadOnlyList<OperationDto> operationDtos)
    {
        if (operationDtos is null)
        {
            throw new ArgumentNullException(nameof(operationDtos));
        }

        var operations = new List<Operation>(operationDtos.Count);
        foreach (var dto in operationDtos)
        {
            operations.Add(OperationMapper.ToDomain(dto));
        }

        var taxes = _calculator.CalculateTaxes(operations);
        var result = new List<TaxResultDto>(taxes.Count);
        foreach (var tax in taxes)
        {
            result.Add(new TaxResultDto { Tax = tax });
        }
        return result;
    }
}


