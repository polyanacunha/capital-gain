using CapitalGains.Application.Services;
using CapitalGains.Application.UseCases;
using CapitalGains.Application.Dtos;
using FluentAssertions;
using Xunit;
using System.Text.Json;

public class CapitalGainsCalculatorUnitTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void Buy_does_not_generate_tax_and_updates_average()
    {
        var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());
        var input = """
        [{"operation":"buy","unit-cost":10.00,"quantity":100}]
        """;
        var operation = JsonSerializer.Deserialize<List<OperationDto>>(input, JsonOptions)!;

        var result = useCase.Handle(operation);

        result.Should().HaveCount(1);
        result[0].Tax.Should().Be(0m);
    }

    [Fact]
    public void Sell_with_profit_above_threshold_generates_20_percent_tax()
    {
        var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());
        var input = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":20.00,"quantity":5000}]
        """;
        var operation = JsonSerializer.Deserialize<List<OperationDto>>(input, JsonOptions)!;

        var result = useCase.Handle(operation);

        result.Should().Equal(
            new[] { new TaxResultDto { Tax = 0m }, new TaxResultDto { Tax = 10000m } },
            (a, b) => a.Tax == b.Tax
        );
    }

    [Fact]
    public void Sell_with_loss_accumulates_loss_and_no_tax()
    {
        var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());
        var input = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":5.00,"quantity":5000}]
        """;
        var operation = JsonSerializer.Deserialize<List<OperationDto>>(input, JsonOptions)!;

        var result = useCase.Handle(operation);

        result.Should().Equal(
            new[] { new TaxResultDto { Tax = 0m }, new TaxResultDto { Tax = 0m } },
            (a, b) => a.Tax == b.Tax
        );
    }
}


