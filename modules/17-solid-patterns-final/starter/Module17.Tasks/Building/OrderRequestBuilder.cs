namespace Module17.Tasks;

/// <summary>
/// Задача 4. Builder: пошаговое создание <see cref="OrderRequest"/> с понятным fluent API.
/// <code>
/// var request = new OrderRequestBuilder()
///     .ForCustomer("anna@example.test", CustomerTier.Gold)
///     .AddItem("BOOK-001", 2)
///     .WithDelivery("courier")
///     .On(new DateOnly(2026, 3, 10))
///     .Build();
/// </code>
/// </summary>
public sealed class OrderRequestBuilder
{
    /// <summary>email null/пустой/пробелы → ArgumentException. Возвращает this.</summary>
    public OrderRequestBuilder ForCustomer(string email, CustomerTier tier = CustomerTier.Regular)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Добавляет товар. sku null/пустой → ArgumentException; quantity ≤ 0 → ArgumentOutOfRangeException.
    /// Повторный AddItem того же sku (точное сравнение) увеличивает количество существующей строки,
    /// порядок строк — порядок первого добавления. Возвращает this.
    /// </summary>
    public OrderRequestBuilder AddItem(string sku, int quantity = 1)
    {
        throw new NotImplementedException();
    }

    /// <summary>deliveryType null/пустой → ArgumentException. По умолчанию (если не вызывали) — "pickup". Возвращает this.</summary>
    public OrderRequestBuilder WithDelivery(string deliveryType)
    {
        throw new NotImplementedException();
    }

    /// <summary>Дата заказа. Возвращает this.</summary>
    public OrderRequestBuilder On(DateOnly orderDate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Создаёт НОВЫЙ OrderRequest. Не задан клиент, нет товаров или не задана дата → InvalidOperationException.
    /// Builder можно продолжать использовать после Build: изменения builder'а не должны влиять
    /// на уже созданные запросы (строки копируются).
    /// </summary>
    public OrderRequest Build()
    {
        throw new NotImplementedException();
    }
}
