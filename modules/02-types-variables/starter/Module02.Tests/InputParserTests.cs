using Module02.Tasks;

namespace Module02.Tests;

public class InputParserTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData(" -42 ", -42)]
    [InlineData("+7", 7)]
    [InlineData("2147483647", int.MaxValue)]
    public void ParseInt_ValidNumber_ReturnsValue(string input, int expected)
    {
        Assert.Equal(expected, InputParser.ParseInt(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("12abc")]
    [InlineData("2147483648")]
    public void ParseInt_InvalidInput_ReturnsNull(string? input)
    {
        Assert.Null(InputParser.ParseInt(input));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("35", 35)]
    [InlineData("150", 150)]
    public void ParseAge_InRange_ReturnsAge(string input, int expected)
    {
        Assert.Equal(expected, InputParser.ParseAge(input));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("151")]
    [InlineData("old")]
    [InlineData(null)]
    public void ParseAge_OutOfRangeOrInvalid_ReturnsNull(string? input)
    {
        Assert.Null(InputParser.ParseAge(input));
    }

    [Fact]
    public void ParseAmount_DotSeparator_ReturnsDecimal()
    {
        Assert.Equal(12.5m, InputParser.ParseAmount("12.5"));
    }

    [Fact]
    public void ParseAmount_CommaSeparator_ReturnsDecimal()
    {
        Assert.Equal(12.5m, InputParser.ParseAmount("12,5"));
    }

    [Fact]
    public void ParseAmount_NegativeWithSpaces_ReturnsDecimal()
    {
        Assert.Equal(-3m, InputParser.ParseAmount(" -3 "));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.2.3")]
    [InlineData("1,2.3")]
    public void ParseAmount_Invalid_ReturnsNull(string? input)
    {
        Assert.Null(InputParser.ParseAmount(input));
    }

    [Theory]
    [InlineData("yes", true)]
    [InlineData(" Y ", true)]
    [InlineData("TRUE", true)]
    [InlineData("1", true)]
    [InlineData("Да", true)]
    [InlineData("no", false)]
    [InlineData("N", false)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("НЕТ", false)]
    public void ParseYesNo_KnownAnswer_ReturnsBool(string input, bool expected)
    {
        Assert.Equal(expected, InputParser.ParseYesNo(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("maybe")]
    [InlineData("yess")]
    public void ParseYesNo_UnknownAnswer_ReturnsNull(string? input)
    {
        Assert.Null(InputParser.ParseYesNo(input));
    }
}
