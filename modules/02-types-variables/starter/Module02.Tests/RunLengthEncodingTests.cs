using Module02.Tasks;

namespace Module02.Tests;

public class RunLengthEncodingTests
{
    [Theory]
    [InlineData("aaabccdddd", "a3b1c2d4")]
    [InlineData("aaaaaaaaaaaa", "a12")]
    [InlineData("aAa", "a1A1a1")]
    [InlineData("x", "x1")]
    [InlineData("  ", " 2")]
    [InlineData("", "")]
    public void Encode_VariousInput_ReturnsEncoded(string text, string expected)
    {
        Assert.Equal(expected, RunLengthEncoding.Encode(text));
    }

    [Fact]
    public void Encode_TextWithDigits_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => RunLengthEncoding.Encode("abc1"));
    }

    [Fact]
    public void Encode_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RunLengthEncoding.Encode(null!));
    }

    [Theory]
    [InlineData("a3b1c2d4", "aaabccdddd")]
    [InlineData("x12", "xxxxxxxxxxxx")]
    [InlineData("a1A1a1", "aAa")]
    [InlineData("", "")]
    public void Decode_ValidInput_ReturnsDecoded(string encoded, string expected)
    {
        Assert.Equal(expected, RunLengthEncoding.Decode(encoded));
    }

    [Theory]
    [InlineData("3a")]
    [InlineData("ab3")]
    [InlineData("a")]
    [InlineData("a0")]
    [InlineData("a2b")]
    public void Decode_InvalidFormat_ThrowsFormatException(string encoded)
    {
        Assert.Throws<FormatException>(() => RunLengthEncoding.Decode(encoded));
    }

    [Fact]
    public void Decode_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => RunLengthEncoding.Decode(null!));
    }

    [Theory]
    [InlineData("Hello, Wooorld!!!")]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void EncodeThenDecode_ReturnsOriginal(string text)
    {
        Assert.Equal(text, RunLengthEncoding.Decode(RunLengthEncoding.Encode(text)));
    }
}
