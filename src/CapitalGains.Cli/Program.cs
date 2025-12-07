using System.Text;
using System.Text.Json;
using CapitalGains.Application.Services;
using CapitalGains.Application.UseCases;
using CapitalGains.Cli;

Console.OutputEncoding = Encoding.UTF8;

var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());
new CliRunner(useCase, json).Run(Console.In, Console.Out);