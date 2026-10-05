namespace Module11.Tasks;

public static class OrderReports
{
    public static decimal TotalRevenue(IEnumerable<Order> orders)
    {
        // Sum на пустой последовательности decimal возвращает 0 — отдельная проверка не нужна
        // (в отличие от Average/Min/Max, которые бросают InvalidOperationException).
        return orders
            .Where(order => order.Status == OrderStatus.Completed)
            .Sum(order => order.Total);
    }

    public static IReadOnlyList<int> LargeOrderIds(IEnumerable<Order> orders, decimal threshold)
    {
        // Query syntax для разнообразия: компилятор переведёт его ровно в Where/OrderByDescending/ThenBy/Select.
        // let вычисляет Total один раз на заказ (Total — вычисляемое свойство, суммирует строки).
        var query =
            from order in orders
            where order.Status != OrderStatus.Cancelled
            let total = order.Total
            where total >= threshold
            orderby total descending, order.Id
            select order.Id;

        // ToList — материализуем: метод возвращает готовый результат, а не «рецепт» запроса.
        return query.ToList();
    }

    public static IReadOnlyList<string> TopProducts(IEnumerable<Order> orders, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return orders
            .Where(order => order.Status == OrderStatus.Completed)
            .SelectMany(order => order.Lines)                     // «расплющиваем» заказы в строки
            .GroupBy(line => line.Product)
            .Select(group => new { Product = group.Key, Quantity = group.Sum(line => line.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .ThenBy(x => x.Product, StringComparer.Ordinal)        // ThenBy, а не второй OrderBy!
            .Take(count)
            .Select(x => x.Product)
            .ToList();
    }

    public static Order? FirstOrderOf(IEnumerable<Order> orders, string customer)
    {
        // OrderBy + FirstOrDefault — O(n log n). MinBy был бы O(n), но не умеет второй ключ;
        // для отчёта читаемость важнее. FirstOrDefault (а не First) — покупателя может не быть.
        return orders
            .Where(order => order.Customer == customer)
            .OrderBy(order => order.Date)
            .ThenBy(order => order.Id)
            .FirstOrDefault();
    }

    public static bool IsLoyalCustomer(IEnumerable<Order> orders, string customer, int minCompletedOrders)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minCompletedOrders, 1);

        // Материализуем заказы покупателя один раз: дальше по ним два прохода (Count и Any),
        // а исходная последовательность может быть «дорогой» (multiple enumeration).
        List<Order> customerOrders = orders.Where(order => order.Customer == customer).ToList();

        // Any с условием останавливается на первом совпадении — не нужно считать все отменённые.
        bool hasCancelled = customerOrders.Any(order => order.Status == OrderStatus.Cancelled);
        int completed = customerOrders.Count(order => order.Status == OrderStatus.Completed);

        return !hasCancelled && completed >= minCompletedOrders;
    }
}
