using System.Globalization;
using Module05.Tasks;

namespace Module05.Tests;

public class PriceCalculatorTests
{
    private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    // Отображение старого enum на новую иерархию — только для сравнения со старым кодом.
    private static Customer Create(CustomerType type, int years) => type switch
    {
        CustomerType.Regular => new RegularCustomer(),
        CustomerType.Silver => new SilverCustomer(),
        CustomerType.Gold => new GoldCustomer(years),
        CustomerType.Employee => new EmployeeCustomer(),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    // Новый тип клиента, о котором PriceCalculator ничего не знает (Open/Closed principle).
    private sealed class BlackFridayCustomer : Customer
    {
        public override decimal DiscountPercent => 50m;
    }

    private sealed class BrokenCustomer : Customer
    {
        public override decimal DiscountPercent => 150m;
    }

    [Theory]
    [InlineData(CustomerType.Regular, "100", 0)]
    [InlineData(CustomerType.Silver, "100", 0)]
    [InlineData(CustomerType.Silver, "19.99", 0)]
    [InlineData(CustomerType.Gold, "100", 0)]
    [InlineData(CustomerType.Gold, "100", 3)]
    [InlineData(CustomerType.Gold, "100", 10)]
    [InlineData(CustomerType.Gold, "100", 25)]
    [InlineData(CustomerType.Gold, "0.99", 7)]
    [InlineData(CustomerType.Employee, "100", 0)]
    [InlineData(CustomerType.Employee, "33.33", 0)]
    [InlineData(CustomerType.Regular, "0", 0)]
    public void FinalPrice_MatchesLegacyCalculator(CustomerType type, string price, int years)
    {
        var expected = LegacyPriceCalculator.FinalPrice(type, D(price), years);

        var actual = PriceCalculator.FinalPrice(Create(type, years), D(price));

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 15)]
    [InlineData(10, 20)]
    [InlineData(30, 20)]
    public void GoldCustomer_DiscountGrowsWithYearsUpTo20(int years, int expectedPercent)
    {
        var gold = new GoldCustomer(years);

        Assert.Equal(years, gold.YearsWithUs);
        Assert.Equal(expectedPercent, gold.DiscountPercent);
    }

    [Fact]
    public void GoldCustomer_NegativeYears_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoldCustomer(-1));
    }

    [Fact]
    public void FinalPrice_NewCustomerType_WorksWithoutChangingCalculator()
    {
        Assert.Equal(50.00m, PriceCalculator.FinalPrice(new BlackFridayCustomer(), 100m));
    }

    [Fact]
    public void FinalPrice_DiscountOutOfRange_ThrowsInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() => PriceCalculator.FinalPrice(new BrokenCustomer(), 100m));
    }

    [Fact]
    public void FinalPrice_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => PriceCalculator.FinalPrice(null!, 10m));
        Assert.Throws<ArgumentOutOfRangeException>(() => PriceCalculator.FinalPrice(new RegularCustomer(), -0.01m));
    }

    [Fact]
    public void FinalPrice_RoundsAwayFromZero()
    {
        // Silver: 0.99 - 0.0495 = 0.9405 → 0.94.
        // Employee: 2.35 - 0.705 = 1.645 → 1.65 (AwayFromZero); банковское округление (ToEven) дало бы 1.64.
        Assert.Equal(0.94m, PriceCalculator.FinalPrice(new SilverCustomer(), 0.99m));
        Assert.Equal(1.65m, PriceCalculator.FinalPrice(new EmployeeCustomer(), 2.35m));
    }
}
