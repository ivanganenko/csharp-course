using Module17.Tasks;

namespace Module17.Tests;

// Ручные test doubles (модуль 14): простые классы вместо mock-библиотеки — так нагляднее видно,
// что DIP позволяет подставить ЛЮБУЮ реализацию абстракции.

/// <summary>Fake-каталог с одним товаром TEST-1 (100 руб., 1 кг).</summary>
internal sealed class FakeCatalog : IProductCatalog
{
    public Product? FindBySku(string sku) =>
        sku == "TEST-1" ? new Product("TEST-1", "Тестовый товар", 100m, 1m) : null;
}

/// <summary>Fake-хранилище: номера TEST-0001, TEST-0002…, запоминает сохранённые заказы.</summary>
internal sealed class FakeOrderRepository : IOrderRepository
{
    private int _counter;

    public List<OrderResult> Saved { get; } = new();

    public string NextOrderNumber(DateOnly orderDate) => $"TEST-{++_counter:D4}";

    public void Save(OrderResult order) => Saved.Add(order);

    public OrderResult? FindByNumber(string orderNumber) => Saved.FirstOrDefault(o => o.OrderNumber == orderNumber);
}

/// <summary>Spy-отправитель: запоминает все отправленные уведомления.</summary>
internal sealed class SpyNotificationSender : INotificationSender
{
    public List<(string Recipient, string Message)> Sent { get; } = new();

    public void Send(string recipient, string message) => Sent.Add((recipient, message));
}

/// <summary>Stub-политика: всегда одна и та же скидка.</summary>
internal sealed class FixedDiscount(decimal discount) : IDiscountPolicy
{
    public decimal CalculateDiscount(CustomerTier tier, decimal subtotal) => discount;
}
