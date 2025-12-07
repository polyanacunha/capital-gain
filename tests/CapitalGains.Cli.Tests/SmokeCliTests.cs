using System.Text;
using System.Text.Json;
using CapitalGains.Application.Dtos;
using CapitalGains.Application.Services;
using CapitalGains.Application.UseCases;
using CapitalGains.Cli;
using FluentAssertions;
using Xunit;

public class SmokeCliTests
{
	[Fact]
	public void CliRunner_should_process_multiple_lines_until_empty_line()
	{
		var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var runner = new CliRunner(new ProcessOperationsUseCase(new CapitalGainsCalculator()), jsonOptions);

		var input = new StringBuilder();
		input.AppendLine("""[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50},{"operation":"sell","unit-cost":15.00,"quantity":50}]""");
		input.AppendLine("""[{"operation":"buy","unit-cost":10.00,"quantity":10000},{"operation":"sell","unit-cost":20.00,"quantity":5000},{"operation":"sell","unit-cost":5.00,"quantity":5000}]""");
		input.AppendLine(""); // empty line ends input

		var reader = new StringReader(input.ToString());
		var writer = new StringWriter();

		runner.Run(reader, writer);

		var lines = writer.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		lines.Should().HaveCount(2);
		lines[0].Should().Be("""[{"tax":0},{"tax":0},{"tax":0}]""");
		lines[1].Should().Be("""[{"tax":0},{"tax":10000},{"tax":0}]""");
	}
}


