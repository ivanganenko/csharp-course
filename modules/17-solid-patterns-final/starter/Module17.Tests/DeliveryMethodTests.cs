using System.Globalization;
using Module17.Tasks;

namespace Module17.Tests;

/// <summary>Задача 2. Strategy для доставки + реестр.</summary>
public sealed class DeliveryMethodTests
{
    private static readonly DateOnly Tuesday = new(2026, 3, 10);
    private static readonly DateOnly Sunday = new(2026, 3, 15);

    private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    private static DeliveryContext Context(string amount = "1000", string weight = "1", DateOnly? date = null) =>
        new(D(amount), D(weight), date ?? Tuesday);

    [Fact]
    public void Pickup_IsAlwaysFree()
    {
        Assert.Equal(0m, new PickupDelivery().CalculateCost(Context(amount: "100", weight: "50")));
    }

    [Theory]
    [InlineData("4999.99", "350")]
    [InlineData("5000", "0")]
    [InlineData("12000", "0")]
    [InlineData("0", "350")]
    public void Courier_FreeFromFiveThousand(string amount, string expected)
    {
        Assert.Equal(D(expected), new CourierDelivery().CalculateCost(Context(amount: amount)));
    }

    [Theory]
    [InlineData("0", "250")]
    [InlineData("0.3", "250")]
    [InlineData("1.0", "250")]
    [InlineData("1.01", "310")]
    [InlineData("3.0", "370")]
    [InlineData("6.5", "610")]
    public void Post_ChargesPerStartedExtraKg(string weight, string expected)
    {
        Assert.Equal(D(expected), new PostDelivery().CalculateCost(Context(weight: weight)));
    }

    [Theory]
    [InlineData("0.4", "900")]
    [InlineData("5", "900")]
    [InlineData("5.01", "1400")]
    public void Express_Weekday_HeavySurchargeAboveFiveKg(string weight, string expected)
    {
        Assert.Equal(D(expected), new ExpressDelivery().CalculateCost(Context(weight: weight)));
    }

    [Fact]
    public void Express_OnSunday_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new ExpressDelivery().CalculateCost(Context(date: Sunday)));
    }

    [Theory]
    [InlineData("pickup", typeof(PickupDelivery))]
    [InlineData("COURIER", typeof(CourierDelivery))]
    [InlineData("  post ", typeof(PostDelivery))]
    [InlineData("Express", typeof(ExpressDelivery))]
    public void Registry_Get_IgnoresCaseAndSpaces(string code, Type expectedType)
    {
        var registry = DeliveryMethodRegistry.CreateDefault();

        Assert.IsType(expectedType, registry.Get(code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("drone")]
    public void Registry_Get_UnknownOrBlank_ThrowsArgumentException(string? code)
    {
        var registry = DeliveryMethodRegistry.CreateDefault();

        Assert.ThrowsAny<ArgumentException>(() => registry.Get(code));
    }

    [Fact]
    public void Registry_CreateDefault_HasFourMethods()
    {
        var codes = DeliveryMethodRegistry.CreateDefault().Codes.OrderBy(c => c, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "courier", "express", "pickup", "post" }, codes);
    }

    [Fact]
    public void Registry_DuplicateCodes_ThrowsArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(
            () => new DeliveryMethodRegistry(new IDeliveryMethod[] { new PickupDelivery(), new PickupDelivery() }));
    }

    [Fact]
    public void Registry_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new DeliveryMethodRegistry(null!));
    }

    [Fact]
    public void Registry_AcceptsNewMethod_WithoutChangingExistingCode()
    {
        // Open/Closed: новый способ — новый класс, ничего в старом коде не правим.
        var registry = new DeliveryMethodRegistry(new IDeliveryMethod[] { new PickupDelivery(), new FixedPriceDelivery("drone", 42m) });

        Assert.Equal(42m, registry.Get("DRONE").CalculateCost(Context()));
    }

    /// <summary>Тестовая стратегия: фиксированная цена.</summary>
    internal sealed class FixedPriceDelivery(string code, decimal price) : IDeliveryMethod
    {
        public string Code => code;

        public string DisplayName => code;

        public decimal CalculateCost(DeliveryContext context) => price;
    }
}
