using Module02.Tasks;

namespace Module02.Tests;

// decimal нельзя использовать в атрибутах [InlineData], поэтому тесты — [Fact] с литералами вида 1.5m.
public class MoneyCalculatorTests
{
    [Fact]
    public void Demo_DoubleIsNotExact_DecimalIs()
    {
        // Документирующий тест: почему деньги не хранят в double.
        Assert.NotEqual(0.3, 0.1 + 0.2);
        Assert.Equal(0.3m, 0.1m + 0.2m);
    }

    [Fact]
    public void ApplyDiscount_FifteenPercent_RoundsToCents()
    {
        Assert.Equal(169.99m, MoneyCalculator.ApplyDiscount(199.99m, 15m));
    }

    [Fact]
    public void ApplyDiscount_ZeroAndFullDiscount_ReturnsPriceAndZero()
    {
        Assert.Equal(100m, MoneyCalculator.ApplyDiscount(100m, 0m));
        Assert.Equal(0m, MoneyCalculator.ApplyDiscount(100m, 100m));
    }

    [Fact]
    public void ApplyDiscount_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.ApplyDiscount(-1m, 10m));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.ApplyDiscount(100m, 101m));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.ApplyDiscount(100m, -0.5m));
    }

    [Fact]
    public void AddVat_TwentyPercent_RoundsToCents()
    {
        Assert.Equal(120m, MoneyCalculator.AddVat(100m, 20m));
        Assert.Equal(23.99m, MoneyCalculator.AddVat(19.99m, 20m)); // 23.988 → 23.99
    }

    [Fact]
    public void AddVat_MidpointValue_RoundsAwayFromZero()
    {
        // 1.25 * 150 / 100 = 1.875 → 1.88 (при банковском округлении было бы тоже 1.88, поэтому проверяем и 0.125)
        Assert.Equal(1.88m, MoneyCalculator.AddVat(1.25m, 50m));
        Assert.Equal(0.13m, MoneyCalculator.AddVat(0.125m, 0m)); // 0.125 → 0.13, а не 0.12 (ToEven)
    }

    [Fact]
    public void AddVat_NegativeArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.AddVat(-10m, 20m));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.AddVat(10m, -20m));
    }

    [Fact]
    public void Split_HundredIntoThree_ExtraCentGoesToFirst()
    {
        var parts = MoneyCalculator.Split(100m, 3);

        Assert.Equal(new[] { 33.34m, 33.33m, 33.33m }, parts);
        Assert.Equal(100m, parts.Sum());
    }

    [Fact]
    public void Split_FewCents_DistributesExactly()
    {
        Assert.Equal(new[] { 0.02m, 0.02m, 0.01m }, MoneyCalculator.Split(0.05m, 3));
    }

    [Fact]
    public void Split_EvenAmount_EqualParts()
    {
        Assert.Equal(new[] { 2.5m, 2.5m, 2.5m, 2.5m }, MoneyCalculator.Split(10m, 4));
    }

    [Fact]
    public void Split_OnePart_ReturnsTotal()
    {
        Assert.Equal(new[] { 10m }, MoneyCalculator.Split(10m, 1));
    }

    [Fact]
    public void Split_ZeroTotal_ReturnsZeros()
    {
        Assert.Equal(new[] { 0m, 0m }, MoneyCalculator.Split(0m, 2));
    }

    [Fact]
    public void Split_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.Split(10m, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.Split(-10m, 2));
        Assert.Throws<ArgumentException>(() => MoneyCalculator.Split(10.005m, 2));
    }

    [Fact]
    public void CompoundInterest_TwoYearsAtTenPercent_Returns1210()
    {
        Assert.Equal(1210m, MoneyCalculator.CompoundInterest(1000m, 10m, 2));
    }

    [Fact]
    public void CompoundInterest_RoundsOnlyAtTheEnd()
    {
        Assert.Equal(1157.63m, MoneyCalculator.CompoundInterest(1000m, 5m, 3));
    }

    [Fact]
    public void CompoundInterest_ZeroYearsOrZeroRate_ReturnsPrincipal()
    {
        Assert.Equal(1000m, MoneyCalculator.CompoundInterest(1000m, 7m, 0));
        Assert.Equal(1000m, MoneyCalculator.CompoundInterest(1000m, 0m, 5));
    }

    [Fact]
    public void CompoundInterest_NegativeArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.CompoundInterest(-1m, 5m, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.CompoundInterest(1000m, -5m, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MoneyCalculator.CompoundInterest(1000m, 5m, -1));
    }
}
