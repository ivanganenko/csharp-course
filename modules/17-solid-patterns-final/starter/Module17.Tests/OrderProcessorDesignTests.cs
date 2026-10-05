using Module17.Tasks;

namespace Module17.Tests;

/// <summary>
/// Задача 6 ⭐. OrderProcessor зависит от абстракций (DIP) и получает их через конструктор (DI).
/// Эти тесты подставляют fake-реализации — с legacy-кодом (static-каталог, new внутри) это невозможно.
/// </summary>
public sealed class OrderProcessorDesignTests
{
    private static readonly DateOnly Tuesday = new(2026, 3, 10);

    private static OrderRequest TestRequest(string delivery = "pickup", CustomerTier tier = CustomerTier.Regular, int quantity = 1) => new()
    {
        CustomerEmail = "anna@example.test",
        Tier = tier,
        DeliveryType = delivery,
        OrderDate = Tuesday,
        Lines = [new OrderLine("TEST-1", quantity)],
    };

    private static OrderProcessor CreateProcessor(
        IDiscountPolicy? discount = null,
        DeliveryMethodRegistry? delivery = null,
        FakeOrderRepository? repository = null,
        INotificationSender? sender = null) =>
        new(
            new FakeCatalog(),
            discount ?? new TierDiscountPolicy(),
            delivery ?? DeliveryMethodRegistry.CreateDefault(),
            repository ?? new FakeOrderRepository(),
            sender ?? new SpyNotificationSender());

    [Fact]
    public void Process_UsesInjectedCatalog()
    {
        var result = CreateProcessor().Process(TestRequest(quantity: 3));

        Assert.Equal(300m, result.Subtotal);
        Assert.Equal(300m, result.Total);
    }

    [Fact]
    public void Process_UnknownSkuInInjectedCatalog_ThrowsArgumentException()
    {
        var request = new OrderRequest
        {
            CustomerEmail = "anna@example.test",
            DeliveryType = "pickup",
            OrderDate = Tuesday,
            Lines = [new OrderLine("BOOK-001", 1)], // есть в стандартном каталоге, но не в fake
        };

        Assert.Throws<ArgumentException>(() => CreateProcessor().Process(request));
    }

    [Fact]
    public void Process_UsesInjectedDiscountPolicy()
    {
        var result = CreateProcessor(discount: new FixedDiscount(25m)).Process(TestRequest(quantity: 2));

        Assert.Equal(25m, result.Discount);
        Assert.Equal(175m, result.Total);
    }

    [Fact]
    public void Process_WorksWithDecoratedPolicy()
    {
        var policy = new CappedDiscountPolicy(new TierDiscountPolicy(), maxDiscount: 15m);

        var result = CreateProcessor(discount: policy).Process(TestRequest(tier: CustomerTier.Gold, quantity: 2));

        Assert.Equal(15m, result.Discount); // 10% от 200 = 20, но кап 15
    }

    [Fact]
    public void Process_UsesNumberAndStorageFromRepository()
    {
        var repository = new FakeOrderRepository();
        var processor = CreateProcessor(repository: repository);

        var result = processor.Process(TestRequest());

        Assert.Equal("TEST-0001", result.OrderNumber);
        Assert.Equal(result, Assert.Single(repository.Saved));
        Assert.Equal(result, processor.GetOrder("TEST-0001"));
    }

    [Fact]
    public void Process_SendsNotificationThroughInjectedSender()
    {
        var sender = new SpyNotificationSender();

        var result = CreateProcessor(sender: sender).Process(TestRequest("courier"));

        var (recipient, message) = Assert.Single(sender.Sent);
        Assert.Equal("anna@example.test", recipient);
        Assert.Equal(result.NotificationMessage, message);
        Assert.Equal("Заказ TEST-0001 оформлен. Сумма: 450.00 руб. Доставка: курьер", message);
    }

    [Fact]
    public void Process_NewDeliveryMethod_WorksWithoutChangingProcessor()
    {
        var registry = new DeliveryMethodRegistry(new IDeliveryMethod[] { new DroneDelivery() });

        var result = CreateProcessor(delivery: registry).Process(TestRequest("drone"));

        Assert.Equal(42m, result.DeliveryCost);
        Assert.Equal("drone", result.DeliveryType);
        Assert.EndsWith("Доставка: дрон", result.NotificationMessage);
    }

    [Fact]
    public void Process_FailedOrder_DoesNotSaveOrNotify()
    {
        var repository = new FakeOrderRepository();
        var sender = new SpyNotificationSender();
        var processor = CreateProcessor(repository: repository, sender: sender);

        Assert.Throws<ArgumentException>(() => processor.Process(TestRequest("teleport")));

        Assert.Empty(repository.Saved);
        Assert.Empty(sender.Sent);
    }

    [Fact]
    public void Process_WithMailGatewayAdapter_SendsEmail()
    {
        var gateway = new LegacyMailGateway();
        var processor = CreateProcessor(sender: new MailGatewayNotificationSender(gateway));

        var result = processor.Process(TestRequest());

        Assert.Contains(result.OrderNumber, Assert.Single(gateway.Sent).HtmlBody);
    }

    [Fact]
    public void Constructor_NullDependency_Throws()
    {
        var catalog = new FakeCatalog();
        var policy = new TierDiscountPolicy();
        var registry = DeliveryMethodRegistry.CreateDefault();
        var repository = new FakeOrderRepository();
        var sender = new SpyNotificationSender();

        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(null!, policy, registry, repository, sender));
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(catalog, null!, registry, repository, sender));
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(catalog, policy, null!, repository, sender));
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(catalog, policy, registry, null!, sender));
        Assert.Throws<ArgumentNullException>(() => new OrderProcessor(catalog, policy, registry, repository, null!));
    }

    private sealed class DroneDelivery : IDeliveryMethod
    {
        public string Code => "drone";

        public string DisplayName => "дрон";

        public decimal CalculateCost(DeliveryContext context) => 42m;
    }
}
