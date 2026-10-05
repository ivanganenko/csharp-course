using Module02.Tasks;

namespace Module02.Tests;

public class StringToolsTests
{
    [Theory]
    [InlineData("level")]
    [InlineData("Level")]
    [InlineData("А роза упала на лапу Азора")]
    [InlineData("Was it a car or a cat I saw?")]
    [InlineData("12321")]
    [InlineData("a")]
    [InlineData("")]
    [InlineData("!!! ...")]
    public void IsPalindrome_Palindromes_ReturnsTrue(string text)
    {
        Assert.True(StringTools.IsPalindrome(text));
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("ab")]
    [InlineData("12 3 4")]
    [InlineData("Not a palindrome")]
    public void IsPalindrome_NotPalindromes_ReturnsFalse(string text)
    {
        Assert.False(StringTools.IsPalindrome(text));
    }

    [Fact]
    public void IsPalindrome_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => StringTools.IsPalindrome(null!));
    }

    [Theory]
    [InlineData("Hello world", 2)]
    [InlineData("  Hello   world  ", 2)]
    [InlineData("one\ttwo\nthree", 3)]
    [InlineData("C# is fun", 3)]
    [InlineData("word", 1)]
    [InlineData("", 0)]
    [InlineData("   \t ", 0)]
    [InlineData(null, 0)]
    public void CountWords_VariousInput_ReturnsCount(string? text, int expected)
    {
        Assert.Equal(expected, StringTools.CountWords(text));
    }

    [Theory]
    [InlineData("  Hello \t\n  world ", "Hello world")]
    [InlineData("a  b   c", "a b c")]
    [InlineData("already normal", "already normal")]
    [InlineData("   ", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void NormalizeSpaces_VariousInput_ReturnsNormalized(string? text, string expected)
    {
        Assert.Equal(expected, StringTools.NormalizeSpaces(text));
    }

    [Theory]
    [InlineData("Admin", "admin", true)]
    [InlineData(" user1 ", "USER1", true)]
    [InlineData("anna", "anne", false)]
    [InlineData("anna", "anna2", false)]
    [InlineData(null, null, true)]
    [InlineData(null, "admin", false)]
    [InlineData("admin", null, false)]
    public void SameLogin_VariousPairs_ComparesIgnoringCaseAndSpaces(string? a, string? b, bool expected)
    {
        Assert.Equal(expected, StringTools.SameLogin(a, b));
    }

    [Theory]
    [InlineData("1234 5678 9012 3456", "**** **** **** 3456")]
    [InlineData("1234-5678", "****-5678")]
    [InlineData("12345", "*2345")]
    [InlineData("1234", "1234")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    public void MaskCardNumber_VariousInput_MasksAllButLastFourDigits(string card, string expected)
    {
        Assert.Equal(expected, StringTools.MaskCardNumber(card));
    }

    [Fact]
    public void MaskCardNumber_DoesNotChangeOriginalString()
    {
        var card = "1111 2222 3333 4444";

        _ = StringTools.MaskCardNumber(card);

        Assert.Equal("1111 2222 3333 4444", card); // строки immutable — исходная строка не может измениться
    }

    [Fact]
    public void MaskCardNumber_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => StringTools.MaskCardNumber(null!));
    }
}
