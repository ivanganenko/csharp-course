using Module14.Tasks;

namespace Module14.Tests;

// Порядок, в котором эти тесты появлялись в TDD:
// 1) Standard = 300 → 2) бесплатно от 3000 (+ граница 2999.99) → 3) Express база 600 →
// 4) доплата за вес (+ «начатый» килограмм) → 5) ошибки ввода.
public class ShippingCalculatorTests
{
    public static TheoryData<decimal, decimal, DeliveryType, decimal> Cases => new()
    {
        { 0m, 1m, DeliveryType.Standard, 300m },
        { 2999.99m, 1m, DeliveryType.Standard, 300m },  // граница: чуть ниже
        { 3000m, 1m, DeliveryType.Standard, 0m },       // граница: ровно порог
        { 5000m, 40m, DeliveryType.Standard, 0m },      // вес не влияет на Standard
        { 100m, 5m, DeliveryType.Express, 600m },       // ровно 5 кг — без доплаты
        { 100m, 5.1m, DeliveryType.Express, 650m },     // начатый килограмм
        { 100m, 6.5m, DeliveryType.Express, 700m },
        { 100m, 7m, DeliveryType.Express, 700m },
        { 10_000m, 0.5m, DeliveryType.Express, 600m },  // Express никогда не бесплатна
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Calculate_ReturnsExpectedPrice(decimal total, decimal weight, DeliveryType type, decimal expected)
    {
        Assert.Equal(expected, ShippingCalculator.Calculate(total, weight, type));
    }

    [Fact]
    public void Calculate_NegativeTotal_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ShippingCalculator.Calculate(-1m, 1m, DeliveryType.Standard));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Calculate_NonPositiveWeight_ThrowsArgumentOutOfRange(int weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ShippingCalculator.Calculate(100m, weight, DeliveryType.Express));
    }

    [Fact]
    public void Calculate_UnknownDeliveryType_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ShippingCalculator.Calculate(100m, 1m, (DeliveryType)42));
    }
}
