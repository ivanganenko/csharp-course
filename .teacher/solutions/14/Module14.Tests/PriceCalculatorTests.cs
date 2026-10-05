using Module14.Tasks;

namespace Module14.Tests;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculateTotal_SingleLine_ReturnsQuantityTimesPrice()
    {
        // Arrange
        var lines = new[] { new OrderLine("BOOK-1", 2, 150m) };

        // Act
        var total = PriceCalculator.CalculateTotal(lines);

        // Assert
        Assert.Equal(300m, total);
    }

    [Fact]
    public void CalculateTotal_EmptyLines_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PriceCalculator.CalculateTotal([]));
    }

    [Fact]
    public void CalculateTotal_SeveralLines_SumsAllLines()
    {
        var lines = new[]
        {
            new OrderLine("BOOK-1", 2, 150m),
            new OrderLine("PEN-7", 1, 99.99m),
        };

        Assert.Equal(399.99m, PriceCalculator.CalculateTotal(lines));
    }

    // decimal нельзя передать в [InlineData] (атрибуты принимают только константы примитивных типов,
    // а decimal для CLR не примитив). Поэтому — TheoryData + MemberData: типобезопасно и без конвертаций.
    public static TheoryData<int, decimal, decimal> DiscountCases => new()
    {
        { 1, 999.99m, 999.99m },   // чуть ниже порога — без скидки
        { 1, 1000m, 900m },        // ровно порог — скидка (этот случай находит баг!)
        { 2, 500.01m, 900.02m },   // 1000.02 * 0.9 = 900.018 → 900.02
        { 3, 500m, 1350m },        // явно выше порога
        { 1, 1234.56m, 1111.10m }, // 1111.104 → 1111.10
        { 4, 0m, 0m },             // бесплатный товар допустим
    };

    [Theory]
    [MemberData(nameof(DiscountCases))]
    public void CalculateTotal_AppliesDiscountFromThresholdAndRounds(int quantity, decimal unitPrice, decimal expected)
    {
        var lines = new[] { new OrderLine("ITEM", quantity, unitPrice) };

        Assert.Equal(expected, PriceCalculator.CalculateTotal(lines));
    }

    [Fact]
    public void CalculateTotal_DiscountIsBasedOnWholeOrder_NotOnSingleLine()
    {
        var lines = new[]
        {
            new OrderLine("A", 1, 600m),
            new OrderLine("B", 1, 400m),
        };

        Assert.Equal(900m, PriceCalculator.CalculateTotal(lines));
    }

    [Fact]
    public void CalculateTotal_NullLines_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => PriceCalculator.CalculateTotal(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateTotal_NonPositiveQuantity_ThrowsArgumentOutOfRange(int quantity)
    {
        var lines = new[] { new OrderLine("BOOK-1", quantity, 10m) };

        Assert.Throws<ArgumentOutOfRangeException>(() => PriceCalculator.CalculateTotal(lines));
    }

    [Fact]
    public void CalculateTotal_NegativePrice_ThrowsArgumentOutOfRange()
    {
        var lines = new[] { new OrderLine("BOOK-1", 1, -0.01m) };

        Assert.Throws<ArgumentOutOfRangeException>(() => PriceCalculator.CalculateTotal(lines));
    }
}
