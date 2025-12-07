using System.Text.Json.Serialization;

namespace CapitalGains.Application.Dtos;

public sealed class OperationDto
{
    [JsonPropertyName("operation")]
    public string Operation { get; init; } = string.Empty;

    [JsonPropertyName("unit-cost")]
    public decimal UnitCost { get; init; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }
}


