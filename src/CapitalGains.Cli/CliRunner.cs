using System.Text.Json;
using CapitalGains.Application.Dtos;
using CapitalGains.Application.UseCases;

namespace CapitalGains.Cli;

public sealed class CliRunner
{
    private readonly ProcessOperationsUseCase _useCase;
    private readonly JsonSerializerOptions _json;

    public CliRunner(ProcessOperationsUseCase useCase, JsonSerializerOptions json)
    {
        _useCase = useCase;
        _json = json;
    }

    public void Run(TextReader input, TextWriter output)
    {
        string? line;
        while (!string.IsNullOrEmpty(line = input.ReadLine()))
        {
            var operations = JsonSerializer.Deserialize<List<OperationDto>>(line, _json) ?? new();
            var result = _useCase.Handle(operations);
            output.WriteLine(JsonSerializer.Serialize(result));
        }
    }
}