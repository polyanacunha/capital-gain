using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CapitalGains.Application.Dtos;
using FluentAssertions;
using Xunit;

public class ProcessIntegrationTests
{
	[Fact]
	public void Dotnet_process_should_accept_input_and_print_expected_output()
	{
		// Locate CLI assembly next to test binaries (copied via ProjectReference)
		var baseDir = AppContext.BaseDirectory;
		var cliDllPath = Path.Combine(baseDir, "CapitalGains.Cli.dll");
		File.Exists(cliDllPath).Should().BeTrue($"CLI assembly not found at {cliDllPath}");

		var psi = new ProcessStartInfo
		{
			FileName = "dotnet",
			Arguments = $"\"{cliDllPath}\"",
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using var process = Process.Start(psi)!;
		process.Should().NotBeNull();

		var input = new StringBuilder();
		input.AppendLine("""[{"operation":"buy","unit-cost":10.00,"quantity":100},{"operation":"sell","unit-cost":15.00,"quantity":50},{"operation":"sell","unit-cost":15.00,"quantity":50}]""");
		input.AppendLine("""[{"operation":"buy","unit-cost":10.00,"quantity":10000},{"operation":"sell","unit-cost":20.00,"quantity":5000},{"operation":"sell","unit-cost":5.00,"quantity":5000}]""");
		input.AppendLine(""); // end

		process.StandardInput.Write(input.ToString());
		process.StandardInput.Flush();
		process.StandardInput.Close();

		var stdout = process.StandardOutput.ReadToEnd();
		var stderr = process.StandardError.ReadToEnd();
		process.WaitForExit(10_000).Should().BeTrue($"CLI did not exit. stderr: {stderr}");
		process.ExitCode.Should().Be(0, $"stderr: {stderr}");

		var lines = stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
		lines.Should().HaveCount(2);

		var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var first = JsonSerializer.Deserialize<List<TaxResultDto>>(lines[0], jsonOptions)!;
		var second = JsonSerializer.Deserialize<List<TaxResultDto>>(lines[1], jsonOptions)!;
		first.Select(x => x.Tax).Should().Equal(new[] { 0m, 0m, 0m });
		second.Select(x => x.Tax).Should().Equal(new[] { 0m, 10000m, 0m });
	}
}


