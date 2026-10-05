using Module01.Tasks;

namespace Module01.Tests;

// decimal нельзя передать в [InlineData] (атрибуты принимают только константы примитивных типов),
// поэтому здесь отдельные [Fact] с литералами вида 45.50m.
public class TipCalculatorTests
{
    [Fact]
    public void CalculateTip_TenPercentOfHundred_ReturnsTen()
    {
        Assert.Equal(10m, TipCalculator.CalculateTip(100m, 10m));
    }

    [Fact]
    public void CalculateTip_MidpointValue_RoundsAwayFromZero()
    {
        // 45.50 * 15 / 100 = 6.825 → 6.83
        Assert.Equal(6.83m, TipCalculator.CalculateTip(45.50m, 15m));
    }

    [Fact]
    public void CalculateTip_SmallValue_RoundsToTwoDecimals()
    {
        // 10.05 * 5 / 100 = 0.5025 → 0.50
        Assert.Equal(0.50m, TipCalculator.CalculateTip(10.05m, 5m));
    }

    [Fact]
    public void CalculateTip_ZeroBill_ReturnsZero()
    {
        Assert.Equal(0m, TipCalculator.CalculateTip(0m, 20m));
    }

    [Fact]
    public void Total_AddsTipToBill()
    {
        Assert.Equal(52.33m, TipCalculator.Total(45.50m, 15m));
    }

    [Fact]
    public void PerPerson_NotDivisibleEvenly_RoundsUpToCent()
    {
        // 100 + 10% = 110; 110 / 3 = 36.666… → 36.67
        Assert.Equal(36.67m, TipCalculator.PerPerson(100m, 10m, 3));
    }

    [Fact]
    public void PerPerson_DivisibleEvenly_ReturnsExactShare()
    {
        Assert.Equal(30m, TipCalculator.PerPerson(90m, 0m, 3));
    }

    [Fact]
    public void CalculateTip_NegativeBill_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TipCalculator.CalculateTip(-1m, 10m));
    }

    [Fact]
    public void CalculateTip_PercentAboveHundred_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TipCalculator.CalculateTip(100m, 101m));
    }

    [Fact]
    public void CalculateTip_NegativePercent_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TipCalculator.CalculateTip(100m, -5m));
    }

    [Fact]
    public void PerPerson_ZeroPeople_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TipCalculator.PerPerson(100m, 10m, 0));
    }
}
