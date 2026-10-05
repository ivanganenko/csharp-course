using Module01.Tasks;

namespace Module01.Tests;

public class GreeterTests
{
    [Theory]
    [InlineData("Anna", "Hello, Anna!")]
    [InlineData("  Anna ", "Hello, Anna!")]
    [InlineData(null, "Hello, World!")]
    [InlineData("", "Hello, World!")]
    [InlineData("   ", "Hello, World!")]
    public void Greet_ReturnsExpectedGreeting(string? name, string expected)
    {
        Assert.Equal(expected, Greeter.Greet(name));
    }

    [Theory]
    [InlineData(5, "Good morning")]
    [InlineData(11, "Good morning")]
    [InlineData(12, "Good afternoon")]
    [InlineData(17, "Good afternoon")]
    [InlineData(18, "Good evening")]
    [InlineData(22, "Good evening")]
    [InlineData(23, "Good night")]
    [InlineData(0, "Good night")]
    [InlineData(4, "Good night")]
    public void GreetingForHour_ReturnsExpected(int hour, string expected)
    {
        Assert.Equal(expected, Greeter.GreetingForHour(hour));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(24)]
    public void GreetingForHour_OutOfRange_Throws(int hour)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Greeter.GreetingForHour(hour));
    }
}
