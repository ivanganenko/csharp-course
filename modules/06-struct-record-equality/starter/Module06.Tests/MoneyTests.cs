using Module06.Tasks;

namespace Module06.Tests;

public class MoneyTests
{
    [Fact]
    public void Constructor_NormalizesCurrency()
    {
        var money = new Money(10m, " usd ");

        Assert.Equal("USD", money.Currency);
        Assert.Equal(10m, money.Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("12$")]
    public void Constructor_InvalidCurrency_Throws(string? currency)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Money(1m, currency!));
    }

    [Fact]
    public void Equality_IsByValue()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "usd");

        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.False(a == new Money(10m, "EUR"));
        Assert.True(a != new Money(11m, "USD"));
    }

    [Fact]
    public void With_ValidatesAndNormalizesCurrency()
    {
        var usd = new Money(5m, "USD");

        var eur = usd with { Currency = "eur" };

        Assert.Equal("EUR", eur.Currency);
        Assert.Equal("USD", usd.Currency);
        Assert.ThrowsAny<ArgumentException>(() => usd with { Currency = "euro" });
    }

    [Fact]
    public void AddAndSubtract_SameCurrency()
    {
        var a = new Money(10.50m, "EUR");
        var b = new Money(2.25m, "EUR");

        Assert.Equal(new Money(12.75m, "EUR"), a + b);
        Assert.Equal(new Money(8.25m, "EUR"), a - b);
    }

    [Fact]
    public void Operators_DifferentCurrencies_ThrowInvalidOperation()
    {
        var usd = new Money(1m, "USD");
        var eur = new Money(1m, "EUR");

        Assert.Throws<InvalidOperationException>(() => usd + eur);
        Assert.Throws<InvalidOperationException>(() => usd - eur);
        Assert.Throws<InvalidOperationException>(() => usd < eur);
        Assert.Throws<InvalidOperationException>(() => usd >= eur);
    }

    [Fact]
    public void Multiply_RoundsAwayFromZero()
    {
        Assert.Equal(new Money(3.33m, "USD"), new Money(10m, "USD") * 0.333m);
        Assert.Equal(new Money(0.63m, "USD"), new Money(1.25m, "USD") * 0.5m);
    }

    [Fact]
    public void Comparison_SameCurrency()
    {
        var five = new Money(5m, "USD");
        var ten = new Money(10m, "USD");

        Assert.True(five < ten);
        Assert.True(ten > five);
        Assert.True(five <= new Money(5m, "USD"));
        Assert.True(ten >= five);
        Assert.False(five > ten);
    }

    [Fact]
    public void Zero_CreatesZeroInNormalizedCurrency()
    {
        Assert.Equal(new Money(0m, "GBP"), Money.Zero("gbp"));
    }

    [Fact]
    public void ToString_UsesTwoDecimalsInvariant()
    {
        Assert.Equal("12.50 EUR", new Money(12.5m, "EUR").ToString());
        Assert.Equal("-3.00 USD", new Money(-3m, "USD").ToString());
    }

    [Fact]
    public void IsValueType_CopyIsIndependent()
    {
        var a = new Money(1m, "USD");
        var b = a;          // копируется значение
        b = b * 2m;         // b теперь новый экземпляр

        Assert.Equal(1m, a.Amount);
        Assert.True(typeof(Money).IsValueType);
    }
}
