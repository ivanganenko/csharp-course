using Module12.Tasks;

namespace Module12.Tests;

public class SpanParserTests
{
    [Theory]
    [InlineData("10,20,30", 60)]
    [InlineData(" 10 , 20,  30 ", 60)]
    [InlineData("42", 42)]
    [InlineData("-5,5", 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    public void SumCsv_ValidInput_ReturnsSum(string line, int expected)
    {
        Assert.Equal(expected, SpanParser.SumCsv(line));
    }

    [Theory]
    [InlineData("1,,2")]
    [InlineData("1,x")]
    [InlineData("1,2,")]
    public void SumCsv_InvalidElement_ThrowsFormatException(string line)
    {
        Assert.Throws<FormatException>(() => SpanParser.SumCsv(line));
    }

    [Theory]
    [InlineData("hello", 1)]
    [InlineData("  hello   big world ", 3)]
    [InlineData("one\ttwo\nthree", 3)]
    [InlineData("", 0)]
    [InlineData("    ", 0)]
    public void CountWords_ReturnsNumberOfWords(string text, int expected)
    {
        Assert.Equal(expected, SpanParser.CountWords(text));
    }

    [Fact]
    public void TryParseKeyValue_ValidPair_ReturnsTrimmedParts()
    {
        var ok = SpanParser.TryParseKeyValue("  host = example.local ", out var key, out var value);

        Assert.True(ok);
        Assert.Equal("host", key.ToString());
        Assert.Equal("example.local", value.ToString());
    }

    [Fact]
    public void TryParseKeyValue_SplitsOnFirstEqualsSign()
    {
        var ok = SpanParser.TryParseKeyValue("a=b=c", out var key, out var value);

        Assert.True(ok);
        Assert.Equal("a", key.ToString());
        Assert.Equal("b=c", value.ToString());
    }

    [Fact]
    public void TryParseKeyValue_EmptyValue_IsAllowed()
    {
        var ok = SpanParser.TryParseKeyValue("timeout=", out var key, out var value);

        Assert.True(ok);
        Assert.Equal("timeout", key.ToString());
        Assert.True(value.IsEmpty);
    }

    [Theory]
    [InlineData("no separator")]
    [InlineData(" = value")]
    [InlineData("")]
    public void TryParseKeyValue_Invalid_ReturnsFalseAndEmptyParts(string input)
    {
        var ok = SpanParser.TryParseKeyValue(input, out var key, out var value);

        Assert.False(ok);
        Assert.True(key.IsEmpty);
        Assert.True(value.IsEmpty);
    }

    [Theory]
    [InlineData("[ERROR] Disk is full", "ERROR")]
    [InlineData("[Info]started", "Info")]
    [InlineData("[] empty level", "")]
    [InlineData("ERROR without brackets", "")]
    [InlineData("[WARN no closing bracket", "")]
    [InlineData("", "")]
    public void GetLogLevel_ReturnsTextInsideLeadingBrackets(string line, string expected)
    {
        Assert.Equal(expected, SpanParser.GetLogLevel(line).ToString());
    }
}
