using FluentAssertions;
using Xunit;

public class SmokeCliTests
{
    [Fact]
    public void Placeholder_should_hold()
    {
        true.Should().BeTrue();
    }
}


