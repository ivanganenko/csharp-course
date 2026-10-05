using System.Globalization;

namespace Module17.Tasks;

// Repository: прячет, ГДЕ и КАК хранятся заказы. Сегодня — словарь, завтра — EF Core (модуль 16);
// OrderProcessor этого не заметит. Нумерация тоже здесь: в реальной БД это была бы sequence.
public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<string, OrderResult> _orders = new(StringComparer.Ordinal);
    private int _counter;

    public string NextOrderNumber(DateOnly orderDate)
    {
        _counter++;
        return "ORD-" + orderDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            + "-" + _counter.ToString("D4", CultureInfo.InvariantCulture);
    }

    public void Save(OrderResult order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _orders[order.OrderNumber] = order;
    }

    public OrderResult? FindByNumber(string orderNumber) => _orders.GetValueOrDefault(orderNumber);
}
