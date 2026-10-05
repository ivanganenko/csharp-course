using Module03.Tasks;

namespace Module03.Tests;

public class DurationParserTests
{
    [Theory]
    [InlineData("1h30m", 90)]
    [InlineData("2h", 120)]
    [InlineData("45m", 45)]
    [InlineData("90m", 90)]
    [InlineData("0m", 0)]
    [InlineData("0h0m", 0)]
    [InlineData("1H30M", 90)]
    [InlineData("  3h5m  ", 185)]
    [InlineData("10h59m", 659)]
    [InlineData("35791394h", 2147483640)]
    public void TryParse_ValidInput_ReturnsTrueAndMinutes(string input, int expected)
    {
        var ok = DurationParser.TryParse(input, out var minutes);

        Assert.True(ok);
        Assert.Equal(expected, minutes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("h")]
    [InlineData("m")]
    [InlineData("abc")]
    [InlineData("-1h")]
    [InlineData("1h30")]
    [InlineData("1h70m")]
    [InlineData("1h60m")]
    [InlineData("1h 30m")]
    [InlineData("30m1h")]
    [InlineData("1h2h")]
    [InlineData("30mm")]
    [InlineData("1.5h")]
    [InlineData("35791395h")]
    public void TryParse_InvalidInput_ReturnsFalseAndZero(string? input)
    {
        var ok = DurationParser.TryParse(input, out var minutes);

        Assert.False(ok);
        Assert.Equal(0, minutes);
    }
}
