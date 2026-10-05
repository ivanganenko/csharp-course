using Module17.Tasks;

namespace Module17.Tests;

/// <summary>Задача 3. Strategy (TierDiscountPolicy) + Decorator (CappedDiscountPolicy).</summary>
public sealed class DiscountPolicyTests
{
    [Fact]
    public void Tier_Regular_NoDiscount()
    {
        Assert.Equal(0m, new TierDiscountPolicy().CalculateDiscount(CustomerTier.Regular, 1000m));
    }

    [Fact]
    public void Tier_Silver_FivePercent()
    {
        Assert.Equal(50.00m, new TierDiscountPolicy().CalculateDiscount(CustomerTier.Silver, 1000m));
    }

    [Fact]
    public void Tier_Gold_TenPercent()
    {
        Assert.Equal(100.00m, new TierDiscountPolicy().CalculateDiscount(CustomerTier.Gold, 1000m));
    }

    [Fact]
    public void Tier_BulkFromTwentyThousand_AddsThreePercent()
    {
        var policy = new TierDiscountPolicy();

        Assert.Equal(600.00m, policy.CalculateDiscount(CustomerTier.Regular, 20000m));
        Assert.Equal(0m, policy.CalculateDiscount(CustomerTier.Regular, 19999.99m));
        Assert.Equal(4937.40m, policy.CalculateDiscount(CustomerTier.Gold, 37980m));
    }

    [Fact]
    public void Tier_RoundsMidpointAwayFromZero()
    {
        Assert.Equal(29.99m, new TierDiscountPolicy().CalculateDiscount(CustomerTier.Silver, 599.70m));
    }

    [Fact]
    public void Tier_NegativeSubtotal_Throws()
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new TierDiscountPolicy().CalculateDiscount(CustomerTier.Gold, -1m));
    }

    [Fact]
    public void Capped_BelowCap_ReturnsInnerDiscount()
    {
        var policy = new CappedDiscountPolicy(new FixedDiscountPolicy(300m), maxDiscount: 1000m);

        Assert.Equal(300m, policy.CalculateDiscount(CustomerTier.Gold, 5000m));
    }

    [Fact]
    public void Capped_AboveCap_ReturnsCap()
    {
        var policy = new CappedDiscountPolicy(new FixedDiscountPolicy(5000m), maxDiscount: 1000m);

        Assert.Equal(1000m, policy.CalculateDiscount(CustomerTier.Gold, 50000m));
    }

    [Fact]
    public void Capped_ZeroCap_ReturnsZero()
    {
        var policy = new CappedDiscountPolicy(new FixedDiscountPolicy(10m), maxDiscount: 0m);

        Assert.Equal(0m, policy.CalculateDiscount(CustomerTier.Silver, 100m));
    }

    [Fact]
    public void Capped_PassesArgumentsToInner()
    {
        var inner = new FixedDiscountPolicy(1m);
        var policy = new CappedDiscountPolicy(inner, 100m);

        policy.CalculateDiscount(CustomerTier.Silver, 777m);

        Assert.Equal((CustomerTier.Silver, 777m), inner.LastCall);
    }

    [Fact]
    public void Capped_DecoratesRealPolicy()
    {
        // Декораторы комбинируются: Gold + опт с 50 000 дал бы 6500, но кап — 2000.
        IDiscountPolicy policy = new CappedDiscountPolicy(new TierDiscountPolicy(), 2000m);

        Assert.Equal(2000m, policy.CalculateDiscount(CustomerTier.Gold, 50000m));
    }

    [Fact]
    public void Capped_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new CappedDiscountPolicy(null!, 10m));
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new CappedDiscountPolicy(new FixedDiscountPolicy(1m), -0.01m));
    }

    /// <summary>Тестовый stub: всегда одна и та же скидка, запоминает аргументы.</summary>
    private sealed class FixedDiscountPolicy(decimal discount) : IDiscountPolicy
    {
        public (CustomerTier Tier, decimal Subtotal)? LastCall { get; private set; }

        public decimal CalculateDiscount(CustomerTier tier, decimal subtotal)
        {
            LastCall = (tier, subtotal);
            return discount;
        }
    }
}
