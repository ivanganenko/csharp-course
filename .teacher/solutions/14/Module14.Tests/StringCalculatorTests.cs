using Module14.Tasks;

namespace Module14.Tests;

// Тесты в порядке TDD-шагов: каждый новый InlineData сначала был красным.
public class StringCalculatorTests
{
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("1", 1)]
    [InlineData("1,2", 3)]
    [InlineData("1,2,3,4,5", 15)]
    [InlineData("1\n2,3", 6)]
    [InlineData("//;\n1;2", 3)]
    [InlineData("//;\n1;2,3\n4", 10)]
    [InlineData("//;\n", 0)]
    [InlineData("2,1001", 2)]
    [InlineData("1000,1", 1001)]
    public void Add_ValidInput_ReturnsSum(string? numbers, int expected)
    {
        Assert.Equal(expected, StringCalculator.Add(numbers));
    }

    [Fact]
    public void Add_Negatives_ThrowsWithAllNegativesInMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() => StringCalculator.Add("-1,2,-3"));

        Assert.Equal("Negatives not allowed: -1, -3", ex.Message);
    }

    [Theory]
    [InlineData("1,,2")]
    [InlineData("1,x")]
    [InlineData("1,\n")]
    [InlineData("//;1;2")]
    public void Add_MalformedInput_ThrowsFormatException(string numbers)
    {
        Assert.Throws<FormatException>(() => StringCalculator.Add(numbers));
    }
}
