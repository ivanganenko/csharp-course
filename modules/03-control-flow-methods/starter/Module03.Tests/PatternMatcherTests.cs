using Module03.Tasks;

namespace Module03.Tests;

public class PatternMatcherTests
{
    [Theory]
    [InlineData(null, "null")]
    [InlineData(0, "zero")]
    [InlineData(-5, "negative int")]
    [InlineData(7, "positive int")]
    [InlineData(3.14, "double")]
    [InlineData("", "empty string")]
    [InlineData("abc", "string of length 3")]
    [InlineData(true, "unknown")]
    [InlineData(5L, "unknown")]
    public void Describe_ScalarValues_ReturnsDescription(object? value, string expected)
    {
        Assert.Equal(expected, PatternMatcher.Describe(value));
    }

    [Fact]
    public void Describe_EmptyArray_ReturnsEmptyArray()
    {
        Assert.Equal("empty array", PatternMatcher.Describe(Array.Empty<int>()));
    }

    [Fact]
    public void Describe_SingleElementArray_ReturnsSingleElement()
    {
        Assert.Equal("single element 42", PatternMatcher.Describe(new[] { 42 }));
    }

    [Fact]
    public void Describe_TwoElementArray_ReturnsFirstAndLast()
    {
        Assert.Equal("array from 1 to 2", PatternMatcher.Describe(new[] { 1, 2 }));
    }

    [Fact]
    public void Describe_LongArray_ReturnsFirstAndLast()
    {
        Assert.Equal("array from 3 to 9", PatternMatcher.Describe(new[] { 3, 5, 7, 9 }));
    }

    [Theory]
    [InlineData(0, 0, "origin")]
    [InlineData(0, 5, "on axis")]
    [InlineData(7, 0, "on axis")]
    [InlineData(2, 3, "point")]
    public void Describe_Point_ReturnsPosition(int x, int y, string expected)
    {
        Assert.Equal(expected, PatternMatcher.Describe(new Point(x, y)));
    }

    [Fact]
    public void Describe_NewYearsEve_ReturnsNewYearsEve()
    {
        Assert.Equal("new year's eve", PatternMatcher.Describe(new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void Describe_OtherDate_ReturnsDate()
    {
        Assert.Equal("date", PatternMatcher.Describe(new DateOnly(2026, 12, 30)));
    }

    [Fact]
    public void Describe_OtherReferenceType_ReturnsUnknown()
    {
        Assert.Equal("unknown", PatternMatcher.Describe(new List<int> { 1, 2 }));
    }
}
