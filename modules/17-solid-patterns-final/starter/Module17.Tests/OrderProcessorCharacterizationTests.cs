using Module17.Tasks;

namespace Module17.Tests;

/// <summary>
/// ХАРАКТЕРИЗАЦИОННЫЕ ТЕСТЫ (characterization tests): фиксируют ТЕКУЩЕЕ поведение legacy-кода.
/// Они зелёные с самого начала и должны оставаться зелёными после КАЖДОГО шага рефакторинга.
/// Используют только публичный фасад: new OrderProcessor(), Process, GetOrder. Менять эти тесты нельзя.
/// </summary>
public sealed class OrderProcessorCharacterizationTests
{
    private static readonly DateOnly Tuesday = new(2026, 3, 10);
    private static readonly DateOnly Sunday = new(2026, 3, 15);

    private static OrderRequest Request(
        string delivery,
        CustomerTier tier = CustomerTier.Regular,
        DateOnly? date = null,
        OrderLine[]? lines = null) =>
        new()
        {
            CustomerEmail = "anna@example.test",
            Tier = tier,
            DeliveryType = delivery,
            OrderDate = date ?? Tuesday,
            Lines = lines ?? Array.Empty<OrderLine>(),
        };

    [Fact]
    public void Regular_Pickup_NoDiscountNoDelivery()
    {
        var result = new OrderProcessor().Process(Request("pickup", lines: [new OrderLine("BOOK-001", 2)]));

        Assert.Equal(3000.00m, result.Subtotal);
        Assert.Equal(0m, result.Discount);
        Assert.Equal(0m, result.DeliveryCost);
        Assert.Equal(3000.00m, result.Total);
        Assert.Equal("pickup", result.DeliveryType);
    }

    [Fact]
    public void Silver_Courier_BelowFreeThreshold_Pays350()
    {
        var result = new OrderProcessor().Process(Request("courier", CustomerTier.Silver, lines: [new OrderLine("MUG-001", 3)]));

        Assert.Equal(1500.00m, result.Subtotal);
        Assert.Equal(75.00m, result.Discount);
        Assert.Equal(350m, result.DeliveryCost);
        Assert.Equal(1775.00m, result.Total);
    }

    [Fact]
    public void Gold_Courier_AfterDiscountAboveThreshold_IsFree()
    {
        var result = new OrderProcessor().Process(Request("courier", CustomerTier.Gold, lines: [new OrderLine("KB-001", 1)]));

        Assert.Equal(650.00m, result.Discount);
        Assert.Equal(0m, result.DeliveryCost);
        Assert.Equal(5850.00m, result.Total);
    }

    [Fact]
    public void Courier_ExactlyFiveThousand_IsFree_BelowIsNot()
    {
        var processor = new OrderProcessor();

        var exactly = processor.Process(Request("courier", lines: [new OrderLine("BOOK-001", 2), new OrderLine("MUG-001", 4)]));
        var below = processor.Process(Request("courier", lines: [new OrderLine("BOOK-001", 3)]));

        Assert.Equal(0m, exactly.DeliveryCost);
        Assert.Equal(5000.00m, exactly.Total);
        Assert.Equal(350m, below.DeliveryCost);
        Assert.Equal(4850.00m, below.Total);
    }

    [Fact]
    public void Post_ChargesPerStartedKgOverOne()
    {
        var processor = new OrderProcessor();

        var light = processor.Process(Request("post", lines: [new OrderLine("CABLE-01", 2)]));                               // 0.2 кг
        var medium = processor.Process(Request("post", lines: [new OrderLine("KB-001", 1), new OrderLine("CABLE-01", 1)])); // 1.3 кг
        var heavy = processor.Process(Request("post", lines: [new OrderLine("MON-001", 1)]));                                // 6.5 кг

        Assert.Equal(250m, light.DeliveryCost);
        Assert.Equal(1030.00m, light.Total);
        Assert.Equal(310m, medium.DeliveryCost);
        Assert.Equal(7200.00m, medium.Total);
        Assert.Equal(610m, heavy.DeliveryCost);
        Assert.Equal(19600.00m, heavy.Total);
    }

    [Fact]
    public void Express_Weekday_LightAndHeavy()
    {
        var processor = new OrderProcessor();

        var light = processor.Process(Request("express", lines: [new OrderLine("MUG-001", 1)]));
        var heavy = processor.Process(Request("express", lines: [new OrderLine("MON-001", 1)]));

        Assert.Equal(900m, light.DeliveryCost);
        Assert.Equal(1400.00m, light.Total);
        Assert.Equal(1400m, heavy.DeliveryCost);
        Assert.Equal(20390.00m, heavy.Total);
    }

    [Fact]
    public void Express_OnSunday_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () => new OrderProcessor().Process(Request("express", date: Sunday, lines: [new OrderLine("MUG-001", 1)])));
    }

    [Fact]
    public void BulkDiscount_AddsThreePercent_FromTwentyThousandInclusive()
    {
        var processor = new OrderProcessor();

        var gold = processor.Process(Request("pickup", CustomerTier.Gold, lines: [new OrderLine("MON-001", 2)]));
        var boundary = processor.Process(Request("pickup", lines: [new OrderLine("MUG-001", 40)]));

        Assert.Equal(37980.00m, gold.Subtotal);
        Assert.Equal(4937.40m, gold.Discount);
        Assert.Equal(33042.60m, gold.Total);
        Assert.Equal(600.00m, boundary.Discount);
        Assert.Equal(19400.00m, boundary.Total);
    }

    [Fact]
    public void Discount_RoundsMidpointAwayFromZero()
    {
        // 599.70 × 5% = 29.985 → 29.99 (банковское округление дало бы 29.98)
        var result = new OrderProcessor().Process(Request("pickup", CustomerTier.Silver, lines: [new OrderLine("STICKER-01", 3)]));

        Assert.Equal(599.70m, result.Subtotal);
        Assert.Equal(29.99m, result.Discount);
        Assert.Equal(569.71m, result.Total);
    }

    [Fact]
    public void DeliveryType_IsCaseInsensitive_AndTrimmed()
    {
        var result = new OrderProcessor().Process(Request("  Courier ", lines: [new OrderLine("MUG-001", 1)]));

        Assert.Equal("courier", result.DeliveryType);
        Assert.Equal(350m, result.DeliveryCost);
    }

    [Fact]
    public void UnknownDelivery_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new OrderProcessor().Process(Request("drone", lines: [new OrderLine("MUG-001", 1)])));
    }

    [Fact]
    public void NullRequest_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor().Process(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    public void InvalidEmail_ThrowsArgumentException(string email)
    {
        var request = new OrderRequest
        {
            CustomerEmail = email,
            DeliveryType = "pickup",
            OrderDate = Tuesday,
            Lines = [new OrderLine("MUG-001", 1)],
        };

        Assert.Throws<ArgumentException>(() => new OrderProcessor().Process(request));
    }

    [Fact]
    public void EmptyOrder_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new OrderProcessor().Process(Request("pickup")));
    }

    [Theory]
    [InlineData("MUG-001", 0)]
    [InlineData("MUG-001", -2)]
    [InlineData("NO-SUCH-SKU", 1)]
    public void InvalidLine_ThrowsArgumentException(string sku, int quantity)
    {
        Assert.Throws<ArgumentException>(
            () => new OrderProcessor().Process(Request("pickup", lines: [new OrderLine(sku, quantity)])));
    }

    [Fact]
    public void OrderNumbers_AreSequential_AndOrdersCanBeFound()
    {
        var processor = new OrderProcessor();

        var first = processor.Process(Request("pickup", lines: [new OrderLine("BOOK-001", 1)]));
        var second = processor.Process(Request("pickup", date: new DateOnly(2026, 3, 11), lines: [new OrderLine("BOOK-001", 1)]));

        Assert.Equal("ORD-20260310-0001", first.OrderNumber);
        Assert.Equal("ORD-20260311-0002", second.OrderNumber);
        Assert.Equal(first, processor.GetOrder("ORD-20260310-0001"));
        Assert.Equal(second, processor.GetOrder("ORD-20260311-0002"));
        Assert.Null(processor.GetOrder("ORD-00000000-9999"));
    }

    [Fact]
    public void FailedOrder_DoesNotConsumeNumber()
    {
        var processor = new OrderProcessor();

        Assert.Throws<InvalidOperationException>(
            () => processor.Process(Request("express", date: Sunday, lines: [new OrderLine("MUG-001", 1)])));
        var ok = processor.Process(Request("pickup", lines: [new OrderLine("MUG-001", 1)]));

        Assert.Equal("ORD-20260310-0001", ok.OrderNumber);
    }

    [Fact]
    public void NotificationMessage_HasExactFormat()
    {
        var result = new OrderProcessor().Process(Request("courier", CustomerTier.Silver, lines: [new OrderLine("MUG-001", 3)]));

        Assert.Equal("Заказ ORD-20260310-0001 оформлен. Сумма: 1775.00 руб. Доставка: курьер", result.NotificationMessage);
    }
}
