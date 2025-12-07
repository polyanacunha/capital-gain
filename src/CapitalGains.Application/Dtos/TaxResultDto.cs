using System.Text.Json.Serialization;

namespace CapitalGains.Application.Dtos;

public sealed class TaxResultDto
{
    [JsonPropertyName("tax")]
    public decimal Tax { get; init; }
}


