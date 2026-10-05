using Module09.Tasks;

namespace Module09.Tests;

public class WordFrequencyTests
{
    [Fact]
    public void Count_EmptyString_ReturnsEmptyDictionary()
    {
        Assert.Empty(WordFrequency.Count(""));
    }

    [Fact]
    public void Count_OnlySeparators_ReturnsEmptyDictionary()
    {
        Assert.Empty(WordFrequency.Count("  ,.!?\n\t - "));
    }

    [Fact]
    public void Count_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => WordFrequency.Count(null!));
    }

    [Fact]
    public void Count_IgnoresCase()
    {
        var result = WordFrequency.Count("The the THE tHe");

        Assert.Single(result);
        Assert.Equal(4, result["the"]);
    }

    [Fact]
    public void Count_SplitsByPunctuationAndNewLines()
    {
        var result = WordFrequency.Count("Hello, world!\nHello... World2 world");

        Assert.Equal(3, result.Count);
        Assert.Equal(2, result["hello"]);
        Assert.Equal(2, result["world"]);
        Assert.Equal(1, result["world2"]);
    }

    [Fact]
    public void Count_SupportsCyrillic()
    {
        var result = WordFrequency.Count("Кот и кот. КОТ!");

        Assert.Equal(3, result["кот"]);
        Assert.Equal(1, result["и"]);
    }

    [Fact]
    public void Top_ReturnsMostFrequentFirst_TiesAlphabetically()
    {
        var top = WordFrequency.Top("b a c b a b d", 3);

        Assert.Equal(3, top.Count);
        Assert.Equal(("b", 3), top[0]);
        Assert.Equal(("a", 2), top[1]);
        Assert.Equal(("c", 1), top[2]);
    }

    [Fact]
    public void Top_NGreaterThanDistinctWords_ReturnsAll()
    {
        var top = WordFrequency.Top("one two two", 10);

        Assert.Equal(2, top.Count);
        Assert.Equal(("two", 2), top[0]);
        Assert.Equal(("one", 1), top[1]);
    }

    [Fact]
    public void Top_Zero_ReturnsEmpty()
    {
        Assert.Empty(WordFrequency.Top("a b c", 0));
    }

    [Fact]
    public void Top_NegativeN_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WordFrequency.Top("a", -1));
    }
}
