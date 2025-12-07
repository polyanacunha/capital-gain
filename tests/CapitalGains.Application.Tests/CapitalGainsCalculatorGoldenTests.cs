using CapitalGains.Application.Dtos;
using CapitalGains.Application.Services;
using CapitalGains.Application.UseCases;
using FluentAssertions;
using Xunit;
using System.Text.Json;

public class CapitalGainsCalculatorGoldenTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static IReadOnlyList<decimal> ExtractTaxes(IReadOnlyList<TaxResultDto> results) => results.Select(result => result.Tax).ToList();

    [Fact]
    public void Case1()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":100},
         {"operation":"sell","unit-cost":15.00,"quantity":50},
         {"operation":"sell","unit-cost":15.00,"quantity":50}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case2()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":20.00,"quantity":5000},
         {"operation":"sell","unit-cost":5.00,"quantity":5000}]
        """;
        var expected = new decimal[] { 0m, 10000m, 0m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case3()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":5.00,"quantity":5000},
         {"operation":"sell","unit-cost":20.00,"quantity":3000}]
        """;
        var expected = new decimal[] { 0m, 0m, 1000m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case4()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"buy","unit-cost":25.00,"quantity":5000},
         {"operation":"sell","unit-cost":15.00,"quantity":10000}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case5()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"buy","unit-cost":25.00,"quantity":5000},
         {"operation":"sell","unit-cost":15.00,"quantity":10000},
         {"operation":"sell","unit-cost":25.00,"quantity":5000}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m, 10000m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case6()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":2.00,"quantity":5000},
         {"operation":"sell","unit-cost":20.00,"quantity":2000},
         {"operation":"sell","unit-cost":20.00,"quantity":2000},
         {"operation":"sell","unit-cost":25.00,"quantity":1000}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m, 0m, 3000m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case7()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":2.00,"quantity":5000},
         {"operation":"sell","unit-cost":20.00,"quantity":2000},
         {"operation":"sell","unit-cost":20.00,"quantity":2000},
         {"operation":"sell","unit-cost":25.00,"quantity":1000},
         {"operation":"buy","unit-cost":20.00,"quantity":10000},
         {"operation":"sell","unit-cost":15.00,"quantity":5000},
         {"operation":"sell","unit-cost":30.00,"quantity":4350},
         {"operation":"sell","unit-cost":30.00,"quantity":650}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m, 0m, 3000m, 0m, 0m, 3700m, 0m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case8()
    {
        var json = """
        [{"operation":"buy","unit-cost":10.00,"quantity":10000},
         {"operation":"sell","unit-cost":50.00,"quantity":10000},
         {"operation":"buy","unit-cost":20.00,"quantity":10000},
         {"operation":"sell","unit-cost":50.00,"quantity":10000}]
        """;
        var expected = new decimal[] { 0m, 80000m, 0m, 60000m };
        AssertCase(json, expected);
    }

    [Fact]
    public void Case9()
    {
        var json = """
        [{"operation":"buy","unit-cost":5000.00,"quantity":10},
         {"operation":"sell","unit-cost":4000.00,"quantity":5},
         {"operation":"buy","unit-cost":15000.00,"quantity":5},
         {"operation":"buy","unit-cost":4000.00,"quantity":2},
         {"operation":"buy","unit-cost":23000.00,"quantity":2},
         {"operation":"sell","unit-cost":20000.00,"quantity":1},
         {"operation":"sell","unit-cost":12000.00,"quantity":10},
         {"operation":"sell","unit-cost":15000.00,"quantity":3}]
        """;
        var expected = new decimal[] { 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 2400m };
        AssertCase(json, expected);
    }

    private static void AssertCase(string jsonArray, decimal[] expectedTaxes)
    {
        var operation = JsonSerializer.Deserialize<List<OperationDto>>(jsonArray, JsonOptions)!;
        var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());
        var result = useCase.Handle(operation);
        ExtractTaxes(result).Should().Equal(expectedTaxes);
    }
}


