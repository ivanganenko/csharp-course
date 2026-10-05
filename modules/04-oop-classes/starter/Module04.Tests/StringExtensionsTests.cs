using Module04.Tasks;

namespace Module04.Tests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" \t\n", true)]
    [InlineData("a", false)]
    [InlineData("  a  ", false)]
    public void IsNullOrBlank_ReturnsExpected(string? value, bool expected)
    {
        Assert.Equal(expected, value.IsNullOrBlank());
    }

    [Fact]
    public void IsNullOrBlank_CalledOnNullReference_DoesNotThrow()
    {
        string? nothing = null;

        // Обычный метод экземпляра на null бросил бы NullReferenceException,
        // а extension method — это статический вызов StringExtensions.IsNullOrBlank(nothing).
        Assert.True(nothing.IsNullOrBlank());
    }

    [Theory]
    [InlineData("Hello world", 5, "Hello...")]
    [InlineData("Hi", 5, "Hi")]
    [InlineData("Hello", 5, "Hello")]
    [InlineData("Hello", 0, "...")]
    [InlineData("", 3, "")]
    public void Truncate_ReturnsExpected(string value, int maxLength, string expected)
    {
        Assert.Equal(expected, value.Truncate(maxLength));
    }

    [Fact]
    public void Truncate_InvalidArguments_Throws()
    {
        string nothing = null!;

        Assert.Throws<ArgumentNullException>(() => nothing.Truncate(3));
        Assert.Throws<ArgumentOutOfRangeException>(() => "text".Truncate(-1));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("   ", 0)]
    [InlineData("one", 1)]
    [InlineData("  two   words ", 2)]
    [InlineData("a\tb\nc", 3)]
    public void WordCount_ReturnsExpected(string? value, int expected)
    {
        Assert.Equal(expected, value.WordCount());
    }

    [Theory]
    [InlineData("1234567812345678", 4, "************5678")]
    [InlineData("secret", 2, "****et")]
    [InlineData("123", 4, "123")]
    [InlineData("1234", 4, "1234")]
    [InlineData("abcd", 0, "****")]
    public void Mask_ReturnsExpected(string value, int visibleSuffix, string expected)
    {
        Assert.Equal(expected, value.Mask(visibleSuffix));
    }

    [Fact]
    public void Mask_DefaultArgumentsAndCustomChar()
    {
        Assert.Equal("****5678", "12345678".Mask());
        Assert.Equal("###45", "12345".Mask(2, '#'));
    }

    [Fact]
    public void Mask_InvalidArguments_Throws()
    {
        string nothing = null!;

        Assert.Throws<ArgumentNullException>(() => nothing.Mask());
        Assert.Throws<ArgumentOutOfRangeException>(() => "1234".Mask(-1));
    }
}
