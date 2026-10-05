using Module01.Tasks;

namespace Module01.Tests;

public class FizzBuzzTests
{
    [Theory]
    [InlineData(1, "1")]
    [InlineData(8, "8")]
    [InlineData(3, "Fizz")]
    [InlineData(5, "Buzz")]
    [InlineData(7, "Whizz")]
    [InlineData(15, "FizzBuzz")]
    [InlineData(21, "FizzWhizz")]
    [InlineData(35, "BuzzWhizz")]
    [InlineData(98, "Whizz")]
    [InlineData(105, "FizzBuzzWhizz")]
    public void ForNumber_PositiveNumber_ReturnsExpectedWord(int n, string expected)
    {
        Assert.Equal(expected, FizzBuzz.ForNumber(n));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ForNumber_NotPositive_Throws(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FizzBuzz.ForNumber(n));
    }

    [Fact]
    public void Sequence_Seven_ReturnsFirstSevenValues()
    {
        string[] expected = ["1", "2", "Fizz", "4", "Buzz", "Fizz", "Whizz"];

        Assert.Equal(expected, FizzBuzz.Sequence(7));
    }

    [Fact]
    public void Sequence_Zero_ReturnsEmptyArray()
    {
        Assert.Empty(FizzBuzz.Sequence(0));
    }

    [Fact]
    public void Sequence_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FizzBuzz.Sequence(-1));
    }
}
