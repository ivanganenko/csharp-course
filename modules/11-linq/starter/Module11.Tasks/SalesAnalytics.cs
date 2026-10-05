namespace Module11.Tasks;

/// <summary>
/// Задача 11.2 — группировки и агрегаты: GroupBy, ToDictionary, ToLookup, Average, Zip.
/// Везде, где не сказано иное, учитываются только заказы со статусом Completed.
/// Тесты: SalesAnalyticsTests.
/// </summary>
public static class SalesAnalytics
{
    /// <summary>Выручка по категориям товаров (сумма OrderLine.Total), только Completed-заказы.</summary>
    public static Dictionary<string, decimal> RevenueByCategory(IEnumerable<Order> orders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Продажи по месяцам (только Completed): количество заказов и выручка.
    /// Только месяцы, в которых были продажи. Сортировка — хронологическая (год, затем месяц).
    /// </summary>
    public static IReadOnlyList<MonthlySales> SalesByMonth(IEnumerable<Order> orders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Id заказов каждого покупателя (ВСЕ статусы) в исходном порядке.
    /// Для неизвестного покупателя lookup должен вернуть пустую последовательность (это поведение ILookup).
    /// </summary>
    public static ILookup<string, int> OrderIdsByCustomer(IEnumerable<Order> orders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Средний чек (среднее Order.Total) по покупателям, только Completed.
    /// Покупатели без Completed-заказов в словарь не попадают.
    /// </summary>
    public static Dictionary<string, decimal> AverageOrderValueByCustomer(IEnumerable<Order> orders)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Изменение выручки день к дню: [100, 150, 120] → [50, -30].
    /// Меньше двух значений → пустой список. Подсказка: Zip.
    /// </summary>
    public static IReadOnlyList<decimal> RevenueChanges(IReadOnlyList<decimal> dailyRevenue)
    {
        throw new NotImplementedException();
    }
}
