using System.Globalization;
using Module05.Tasks;

namespace Module05.Tests;

public class PaymentTests
{
    // decimal нельзя передать в атрибут [InlineData], поэтому передаём строку и парсим без зависимости от культуры.
    private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("100", "2.00")]
    [InlineData("50", "1.00")]
    [InlineData("10", "0.30")]
    [InlineData("15", "0.30")]
    [InlineData("33.33", "0.67")]
    public void CardPayment_Fee_IsTwoPercentButNotLessThanMinimum(string amount, string expectedFee)
    {
        Assert.Equal(D(expectedFee), new CardPayment().CalculateFee(D(amount)));
    }

    [Fact]
    public void CardPayment_Pay_ChargesAmountPlusFee()
    {
        var card = new CardPayment();

        var result = card.Pay(100m);

        Assert.True(result.Success);
        Assert.Equal("Card", result.Method);
        Assert.Equal(102.00m, result.Charged);
        Assert.Null(result.Error);
        Assert.Equal(102.00m, card.TotalCharged);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Pay_NonPositiveAmount_ThrowsArgumentOutOfRange(string amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CardPayment().Pay(D(amount)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WalletPayment(100m).Pay(D(amount)));
    }

    [Fact]
    public void WalletPayment_EnoughBalance_DeductsWithoutFee()
    {
        var wallet = new WalletPayment(50m);

        var result = wallet.Pay(30m);

        Assert.True(result.Success);
        Assert.Equal("Wallet", result.Method);
        Assert.Equal(30m, result.Charged);
        Assert.Equal(20m, wallet.Balance);
    }

    [Fact]
    public void WalletPayment_NotEnoughBalance_FailsAndKeepsBalance()
    {
        var wallet = new WalletPayment(50m);

        var result = wallet.Pay(60m);

        Assert.False(result.Success);
        Assert.Equal(0m, result.Charged);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
        Assert.Equal(50m, wallet.Balance);
    }

    [Fact]
    public void WalletPayment_NegativeBalance_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WalletPayment(-1m));
    }

    [Fact]
    public void BankTransfer_BelowMinimum_Fails_AboveMinimum_ChargesFixedFee()
    {
        var transfer = new BankTransferPayment();

        var small = transfer.Pay(5m);
        var normal = transfer.Pay(100m);

        Assert.False(small.Success);
        Assert.True(normal.Success);
        Assert.Equal("BankTransfer", normal.Method);
        Assert.Equal(101.00m, normal.Charged);
        Assert.Equal(101.00m, transfer.TotalCharged);
    }

    [Fact]
    public void CanPay_IsPolymorphic()
    {
        var methods = new List<PaymentMethod> { new CardPayment(), new WalletPayment(20m), new BankTransferPayment() };

        var canPay15 = methods.Select(m => m.CanPay(15m)).ToList();
        var canPay5 = methods.Select(m => m.CanPay(5m)).ToList();

        Assert.Equal("True True True", string.Join(" ", canPay15));
        Assert.Equal("True True False", string.Join(" ", canPay5));
        Assert.All(methods, m => Assert.False(m.CanPay(0m)));
    }

    [Fact]
    public void Checkout_UsesFirstMethodThatCanPay()
    {
        var wallet = new WalletPayment(10m);
        var card = new CardPayment();

        var result = Checkout.PayWithFirstAvailable(new PaymentMethod[] { wallet, card }, 25m);

        Assert.True(result.Success);
        Assert.Equal("Card", result.Method);
        Assert.Equal(25.50m, result.Charged);
        Assert.Equal(10m, wallet.Balance);
    }

    [Fact]
    public void Checkout_NoMethodCanPay_ReturnsFailedNone()
    {
        var result = Checkout.PayWithFirstAvailable(new PaymentMethod[] { new WalletPayment(10m) }, 25m);

        Assert.False(result.Success);
        Assert.Equal("none", result.Method);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void Checkout_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Checkout.PayWithFirstAvailable(null!, 10m));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Checkout.PayWithFirstAvailable(new PaymentMethod[] { new CardPayment() }, 0m));
    }
}
