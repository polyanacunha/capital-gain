using System.Text;
using System.Text.Json;
using CapitalGains.Application.Services;
using CapitalGains.Application.UseCases;
using CapitalGains.Cli;

Console.OutputEncoding = Encoding.UTF8;

var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var useCase = new ProcessOperationsUseCase(new CapitalGainsCalculator());

if (!Console.IsInputRedirected)
{
	Console.Error.WriteLine("Enter one JSON array of operations per line. Press Enter to finish.");
	Console.Error.WriteLine(@"Example: [{""operation"":""buy"",""unit-cost"":10.00,""quantity"":100},{""operation"":""sell"",""unit-cost"":15.00,""quantity"":50}]");
}

new CliRunner(useCase, json).Run(Console.In, Console.Out);