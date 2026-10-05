using Module03.Tasks;

namespace Module03.Tests;

public class MethodBasicsTests
{
    [Fact]
    public void Swap_TwoVariables_ExchangesValues()
    {
        var a = 1;
        var b = 2;

        MethodBasics.Swap(ref a, ref b);

        Assert.Equal(2, a);
        Assert.Equal(1, b);
    }

    [Fact]
    public void Sum_NoArguments_ReturnsZero()
    {
        Assert.Equal(0, MethodBasics.Sum());
    }

    [Fact]
    public void Sum_SeveralArguments_ReturnsSum()
    {
        Assert.Equal(6, MethodBasics.Sum(1, 2, 3));
    }

    [Fact]
    public void Sum_ArrayArgument_ReturnsSum()
    {
        int[] numbers = [4, 5, -2];

        Assert.Equal(7, MethodBasics.Sum(numbers));
    }

    [Fact]
    public void Sum_NullArray_Throws()
    {
        int[]? numbers = null;

        Assert.Throws<ArgumentNullException>(() => MethodBasics.Sum(numbers!));
    }

    [Fact]
    public void MinMax_SeveralNumbers_ReturnsMinAndMax()
    {
        MethodBasics.MinMax([3, -1, 7, 0], out var min, out var max);

        Assert.Equal(-1, min);
        Assert.Equal(7, max);
    }

    [Fact]
    public void MinMax_SingleNumber_MinEqualsMax()
    {
        MethodBasics.MinMax([5], out var min, out var max);

        Assert.Equal(5, min);
        Assert.Equal(5, max);
    }

    [Fact]
    public void MinMax_Empty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MethodBasics.MinMax([], out _, out _));
    }

    [Fact]
    public void MinMax_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => MethodBasics.MinMax(null!, out _, out _));
    }

    [Fact]
    public void FormatPrice_DefaultArguments_UsesEurAndTwoDecimals()
    {
        Assert.Equal("12.50 EUR", MethodBasics.FormatPrice(12.5m));
    }

    [Fact]
    public void FormatPrice_PositionalCurrency_UsesGivenCurrency()
    {
        Assert.Equal("12.50 USD", MethodBasics.FormatPrice(12.5m, "USD"));
    }

    [Fact]
    public void FormatPrice_NamedDecimalsOnly_KeepsDefaultCurrency()
    {
        Assert.Equal("3 EUR", MethodBasics.FormatPrice(3m, decimals: 0));
        Assert.Equal("1234.568 EUR", MethodBasics.FormatPrice(1234.5678m, decimals: 3));
    }

    [Fact]
    public void FormatPrice_NamedArgumentsInAnyOrder_Works()
    {
        Assert.Equal("3.0 GBP", MethodBasics.FormatPrice(decimals: 1, amount: 3m, currency: "GBP"));
    }

    [Fact]
    public void FormatPrice_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MethodBasics.FormatPrice(1m, decimals: 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => MethodBasics.FormatPrice(1m, decimals: -1));
        Assert.Throws<ArgumentException>(() => MethodBasics.FormatPrice(1m, ""));
    }

    [Fact]
    public void Area_OneArgument_CallsCircleOverload()
    {
        Assert.Equal(Math.PI * 4, MethodBasics.Area(2.0), 10);
    }

    [Fact]
    public void Area_TwoArguments_CallsRectangleOverload()
    {
        Assert.Equal(6.0, MethodBasics.Area(2.0, 3.0), 10);
    }

    [Fact]
    public void Area_IntArgument_ImplicitlyConvertedToDouble()
    {
        Assert.Equal(Math.PI, MethodBasics.Area(1), 10);
    }

    [Fact]
    public void Area_NegativeValues_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MethodBasics.Area(-1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MethodBasics.Area(2.0, -3.0));
    }
}
