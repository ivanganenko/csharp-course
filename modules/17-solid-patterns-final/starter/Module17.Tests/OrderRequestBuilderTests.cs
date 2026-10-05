using Module17.Tasks;

namespace Module17.Tests;

/// <summary>Задача 4. Builder.</summary>
public sealed class OrderRequestBuilderTests
{
    private static readonly DateOnly Tuesday = new(2026, 3, 10);

    [Fact]
    public void Build_FullChain_ReturnsConfiguredRequest()
    {
        var request = new OrderRequestBuilder()
            .ForCustomer("anna@example.test", CustomerTier.Gold)
            .AddItem("BOOK-001", 2)
            .AddItem("MUG-001")
            .WithDelivery("courier")
            .On(Tuesday)
            .Build();

        Assert.Equal("anna@example.test", request.CustomerEmail);
        Assert.Equal(CustomerTier.Gold, request.Tier);
        Assert.Equal(new[] { new OrderLine("BOOK-001", 2), new OrderLine("MUG-001", 1) }, request.Lines.ToArray());
        Assert.Equal("courier", request.DeliveryType);
        Assert.Equal(Tuesday, request.OrderDate);
    }

    [Fact]
    public void Build_Defaults_RegularTierAndPickup()
    {
        var request = new OrderRequestBuilder().ForCustomer("a@b.test").AddItem("MUG-001").On(Tuesday).Build();

        Assert.Equal(CustomerTier.Regular, request.Tier);
        Assert.Equal("pickup", request.DeliveryType);
    }

    [Fact]
    public void AddItem_SameSku_MergesQuantities_KeepsOrder()
    {
        var request = new OrderRequestBuilder()
            .ForCustomer("a@b.test")
            .AddItem("MUG-001", 1)
            .AddItem("BOOK-001", 1)
            .AddItem("MUG-001", 3)
            .On(Tuesday)
            .Build();

        Assert.Equal(new[] { new OrderLine("MUG-001", 4), new OrderLine("BOOK-001", 1) }, request.Lines.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_NonPositiveQuantity_Throws(int quantity)
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new OrderRequestBuilder().AddItem("MUG-001", quantity));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void AddItem_BlankSku_Throws(string sku)
    {
        Assert.ThrowsAny<ArgumentException>(() => new OrderRequestBuilder().AddItem(sku));
    }

    [Fact]
    public void ForCustomer_BlankEmail_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new OrderRequestBuilder().ForCustomer(" "));
    }

    [Fact]
    public void Build_WithoutCustomer_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderRequestBuilder().AddItem("MUG-001").On(Tuesday).Build());
    }

    [Fact]
    public void Build_WithoutItems_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderRequestBuilder().ForCustomer("a@b.test").On(Tuesday).Build());
    }

    [Fact]
    public void Build_WithoutDate_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new OrderRequestBuilder().ForCustomer("a@b.test").AddItem("MUG-001").Build());
    }

    [Fact]
    public void Build_Twice_RequestsAreIndependent()
    {
        var builder = new OrderRequestBuilder().ForCustomer("a@b.test").AddItem("MUG-001").On(Tuesday);

        var first = builder.Build();
        builder.AddItem("BOOK-001").AddItem("MUG-001");
        var second = builder.Build();

        Assert.Equal(new[] { new OrderLine("MUG-001", 1) }, first.Lines.ToArray());
        Assert.Equal(new[] { new OrderLine("MUG-001", 2), new OrderLine("BOOK-001", 1) }, second.Lines.ToArray());
    }

    [Fact]
    public void BuiltRequest_IsAcceptedByOrderProcessor()
    {
        var request = new OrderRequestBuilder()
            .ForCustomer("anna@example.test", CustomerTier.Silver)
            .AddItem("MUG-001", 3)
            .WithDelivery("courier")
            .On(Tuesday)
            .Build();

        var result = new OrderProcessor().Process(request);

        Assert.Equal(1775.00m, result.Total);
    }
}
