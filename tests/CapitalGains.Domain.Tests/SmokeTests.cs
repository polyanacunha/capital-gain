using FluentAssertions;
using Xunit;

public class SmokeTests
{
    [Fact]
    public void True_should_be_true()
    {
        true.Should().BeTrue();
    }
}


