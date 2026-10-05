namespace Module11.Tasks;

public static class SalesAnalytics
{
    public static Dictionary<string, decimal> RevenueByCategory(IEnumerable<Order> orders)
    {
        return orders
            .Where(order => order.Status == OrderStatus.Completed)
            .SelectMany(order => order.Lines)
            .GroupBy(line => line.Category)
            // ToDictionary — immediate execution: результат вычисляется прямо здесь.
            .ToDictionary(group => group.Key, group => group.Sum(line => line.Total));
    }

    public static IReadOnlyList<MonthlySales> SalesByMonth(IEnumerable<Order> orders)
    {
        return orders
            .Where(order => order.Status == OrderStatus.Completed)
            // Составной ключ — анонимный тип: у него сгенерированы Equals/GetHashCode по значениям,
            // поэтому GroupBy корректно объединяет заказы одного месяца.
            .GroupBy(order => new { order.Date.Year, order.Date.Month })
            .OrderBy(group => group.Key.Year)
            .ThenBy(group => group.Key.Month)
            .Select(group => new MonthlySales(
                group.Key.Year,
                group.Key.Month,
                group.Count(),
                group.Sum(order => order.Total)))
            .ToList();
    }

    public static ILookup<string, int> OrderIdsByCustomer(IEnumerable<Order> orders)
    {
        // ToLookup, а не GroupBy: lookup вычисляется сразу (immediate), даёт доступ по ключу
        // и для отсутствующего ключа возвращает пустую последовательность вместо исключения.
        return orders.ToLookup(order => order.Customer, order => order.Id);
    }

    public static Dictionary<string, decimal> AverageOrderValueByCustomer(IEnumerable<Order> orders)
    {
        // Average безопасен: каждая группа GroupBy содержит хотя бы один элемент.
        return orders
            .Where(order => order.Status == OrderStatus.Completed)
            .GroupBy(order => order.Customer)
            .ToDictionary(group => group.Key, group => group.Average(order => order.Total));
    }

    public static IReadOnlyList<decimal> RevenueChanges(IReadOnlyList<decimal> dailyRevenue)
    {
        // Zip «сшивает» последовательность со своей же копией, сдвинутой на один элемент:
        // (d0, d1), (d1, d2), ... Zip останавливается на более короткой — поэтому для 0 или 1 элемента
        // результат пустой без отдельных проверок.
        return dailyRevenue
            .Zip(dailyRevenue.Skip(1), (previous, current) => current - previous)
            .ToList();
    }
}
