namespace Module17.Tasks;

// Модели заказа. Это публичный контракт — его используют характеризационные тесты,
// поэтому при рефакторинге эти типы НЕ меняются.
// (Все типы модуля для простоты живут в одном namespace Module17.Tasks, хотя разложены по папкам.)

public enum CustomerTier
{
    Regular,
    Silver,
    Gold,
}

/// <summary>Строка заказа: артикул товара и количество.</summary>
public sealed record OrderLine(string Sku, int Quantity);

/// <summary>Запрос на оформление заказа.</summary>
public sealed class OrderRequest
{
    public string CustomerEmail { get; init; } = string.Empty;

    public CustomerTier Tier { get; init; } = CustomerTier.Regular;

    public IReadOnlyList<OrderLine> Lines { get; init; } = Array.Empty<OrderLine>();

    /// <summary>"pickup", "courier", "post", "express" (без учёта регистра и пробелов по краям).</summary>
    public string DeliveryType { get; init; } = string.Empty;

    public DateOnly OrderDate { get; init; }
}

/// <summary>Результат оформления заказа.</summary>
public sealed record OrderResult(
    string OrderNumber,
    decimal Subtotal,
    decimal Discount,
    decimal DeliveryCost,
    decimal Total,
    string DeliveryType,
    string NotificationMessage);
