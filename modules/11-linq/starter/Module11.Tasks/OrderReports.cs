namespace Module11.Tasks;

/// <summary>
/// Задача 11.1 — отчёты по заказам на базовых операторах LINQ
/// (Where, Select, SelectMany, Sum, OrderBy/ThenBy, First/FirstOrDefault, Any/All, Take).
/// Пишите в method syntax; один-два метода можно попробовать в query syntax.
/// Сравнение строк при сортировке — ordinal (<see cref="StringComparer.Ordinal"/>).
/// Тесты: OrderReportsTests.
/// </summary>
public static class OrderReports
{
    /// <summary>Выручка: сумма <see cref="Order.Total"/> только по заказам со статусом Completed. Нет таких → 0.</summary>
    public static decimal TotalRevenue(IEnumerable<Order> orders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Id «крупных» заказов: НЕ отменённые (Status != Cancelled) с Total &gt;= threshold.
    /// Сортировка: по Total по убыванию, при равенстве — по Id по возрастанию.
    /// </summary>
    public static IReadOnlyList<int> LargeOrderIds(IEnumerable<Order> orders, decimal threshold)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Названия самых продаваемых товаров по суммарному количеству (Quantity) в Completed-заказах.
    /// Сортировка: по количеству по убыванию, при равенстве — по названию (ordinal).
    /// Возвращает не более <paramref name="count"/> названий. count &lt; 0 → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public static IReadOnlyList<string> TopProducts(IEnumerable<Order> orders, int count)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Самый ранний заказ покупателя (любой статус): минимальная Date, при равенстве — минимальный Id.
    /// Нет заказов → null.
    /// </summary>
    public static Order? FirstOrderOf(IEnumerable<Order> orders, string customer)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// «Лояльный» покупатель: не меньше <paramref name="minCompletedOrders"/> заказов в статусе Completed
    /// и НИ ОДНОГО Cancelled. minCompletedOrders &lt; 1 → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public static bool IsLoyalCustomer(IEnumerable<Order> orders, string customer, int minCompletedOrders)
    {
        throw new NotImplementedException();
    }
}
