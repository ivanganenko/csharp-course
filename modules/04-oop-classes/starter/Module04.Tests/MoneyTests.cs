using Module04.Tasks;

namespace Module04.Tests;

public class MoneyTests
{
    [Fact]
    public void Constructor_NormalizesCurrencyAndRoundsAmount()
    {
        var money = new Money(10.005m, " usd ");

        Assert.Equal(10.01m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Constructor_RoundsMidpointAwayFromZero()
    {
        // При банковском округлении (MidpointRounding.ToEven, по умолчанию в Math.Round) было бы 2.34.
        Assert.Equal(2.35m, new Money(2.345m, "EUR").Amount);
        Assert.Equal(-2.35m, new Money(-2.345m, "EUR").Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("U1D")]
    public void Constructor_InvalidCurrency_ThrowsArgumentException(string? currency)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Money(1m, currency!));
    }

    [Fact]
    public void Add_SameCurrency_ReturnsNewMoneyAndKeepsOperandsUnchanged()
    {
        var a = new Money(10.50m, "USD");
        var b = new Money(0.75m, "USD");

        var sum = a.Add(b);

        Assert.Equal(11.25m, sum.Amount);
        Assert.Equal("USD", sum.Currency);
        Assert.Equal(10.50m, a.Amount);
        Assert.Equal(0.75m, b.Amount);
    }

    [Fact]
    public void AddAndSubtract_DifferentCurrencyOrNull_Throw()
    {
        var usd = new Money(1m, "USD");
        var eur = new Money(1m, "EUR");

        Assert.Throws<InvalidOperationException>(() => usd.Add(eur));
        Assert.Throws<InvalidOperationException>(() => usd.Subtract(eur));
        Assert.Throws<ArgumentNullException>(() => usd.Add(null!));
    }

    [Fact]
    public void Subtract_CanProduceNegativeAmount()
    {
        var result = new Money(5m, "USD").Subtract(new Money(7.5m, "USD"));

        Assert.Equal(-2.5m, result.Amount);
    }

    [Fact]
    public void Multiply_RoundsResult()
    {
        Assert.Equal(3.33m, new Money(10m, "USD").Multiply(0.333m).Amount);
        Assert.Equal(0.63m, new Money(1.25m, "USD").Multiply(0.5m).Amount);
    }

    [Fact]
    public void Allocate_100By3_DistributesRemainderToFirstParts()
    {
        var parts = new Money(100m, "USD").Allocate(3);

        Assert.Equal(3, parts.Count);
        Assert.Equal(33.34m, parts[0].Amount);
        Assert.Equal(33.33m, parts[1].Amount);
        Assert.Equal(33.33m, parts[2].Amount);
        Assert.All(parts, p => Assert.Equal("USD", p.Currency));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(7, 3)]
    [InlineData(1, 1)]
    [InlineData(5, 0.05)]
    public void Allocate_SumOfPartsEqualsOriginal(int parts, double amount)
    {
        var money = new Money((decimal)amount, "EUR");

        var result = money.Allocate(parts);

        Assert.Equal(parts, result.Count);
        Assert.Equal(money.Amount, result.Sum(p => p.Amount));
    }

    [Fact]
    public void Allocate_InvalidState_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(10m, "USD").Allocate(0));
        Assert.Throws<InvalidOperationException>(() => new Money(-10m, "USD").Allocate(2));
    }

    [Fact]
    public void Zero_CreatesZeroInGivenCurrency()
    {
        var zero = Money.Zero("gbp");

        Assert.Equal(0m, zero.Amount);
        Assert.Equal("GBP", zero.Currency);
    }

    [Fact]
    public void ToString_UsesTwoDecimalsAndInvariantCulture()
    {
        Assert.Equal("5.00 EUR", new Money(5m, "EUR").ToString());
        Assert.Equal("-2.50 USD", new Money(-2.5m, "USD").ToString());
        Assert.Equal("1234.56 USD", new Money(1234.56m, "USD").ToString());
    }
}
