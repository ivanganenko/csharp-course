using Module02.Tasks;

namespace Module02.Tests;

public class SafeMathTests
{
    [Fact]
    public void Demo_UncheckedIntAddition_WrapsAround()
    {
        // Этот тест не проверяет ваш код — он документирует поведение C#:
        // по умолчанию переполнение int НЕ бросает исключение, а «заворачивает» значение.
        var max = int.MaxValue;

        var wrapped = unchecked(max + 1);

        Assert.Equal(int.MinValue, wrapped);
    }

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(-10, 4, -6)]
    [InlineData(int.MaxValue, 0, int.MaxValue)]
    [InlineData(int.MinValue, int.MaxValue, -1)]
    public void AddChecked_NoOverflow_ReturnsSum(int a, int b, int expected)
    {
        Assert.Equal(expected, SafeMath.AddChecked(a, b));
    }

    [Theory]
    [InlineData(int.MaxValue, 1)]
    [InlineData(int.MinValue, -1)]
    public void AddChecked_Overflow_ThrowsOverflowException(int a, int b)
    {
        Assert.Throws<OverflowException>(() => SafeMath.AddChecked(a, b));
    }

    [Fact]
    public void TryAdd_NoOverflow_ReturnsTrueAndSum()
    {
        var ok = SafeMath.TryAdd(40, 2, out var result);

        Assert.True(ok);
        Assert.Equal(42, result);
    }

    [Fact]
    public void TryAdd_Overflow_ReturnsFalseAndZero()
    {
        var ok = SafeMath.TryAdd(int.MaxValue, 10, out var result);

        Assert.False(ok);
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(2, 7, 4)]
    [InlineData(0, 0, 0)]
    [InlineData(5, 5, 5)]
    [InlineData(0, int.MaxValue, 1073741823)]
    [InlineData(int.MaxValue - 1, int.MaxValue, int.MaxValue - 1)]
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue)]
    public void Midpoint_ValidRange_ReturnsMiddleWithoutOverflow(int low, int high, int expected)
    {
        Assert.Equal(expected, SafeMath.Midpoint(low, high));
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(5, 2)]
    public void Midpoint_InvalidRange_Throws(int low, int high)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SafeMath.Midpoint(low, high));
    }

    [Theory]
    [InlineData(42L, 42)]
    [InlineData(-2147483648L, int.MinValue)]
    public void ToIntChecked_FitsInInt_ReturnsValue(long value, int expected)
    {
        Assert.Equal(expected, SafeMath.ToIntChecked(value));
    }

    [Theory]
    [InlineData(3_000_000_000L)]
    [InlineData(-2147483649L)]
    public void ToIntChecked_DoesNotFit_ThrowsOverflowException(long value)
    {
        Assert.Throws<OverflowException>(() => SafeMath.ToIntChecked(value));
    }

    [Fact]
    public void SumAsLong_LargeValues_DoesNotOverflow()
    {
        Assert.Equal(4_294_967_294L, SafeMath.SumAsLong([int.MaxValue, int.MaxValue]));
    }

    [Fact]
    public void SumAsLong_Empty_ReturnsZero()
    {
        Assert.Equal(0L, SafeMath.SumAsLong([]));
    }

    [Fact]
    public void SumAsLong_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SafeMath.SumAsLong(null!));
    }
}
