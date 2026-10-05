namespace Module11.Tasks;

// Модели интернет-магазина для задач 11.1 и 11.2. Готовы — менять не нужно.
// Данные в тестах вымышленные.

public enum OrderStatus
{
    New,
    Completed,
    Cancelled,
}

/// <summary>Строка заказа: товар, категория, количество, цена за единицу.</summary>
public sealed record OrderLine(string Product, string Category, int Quantity, decimal UnitPrice)
{
    public decimal Total => Quantity * UnitPrice;
}

/// <summary>Заказ. Сумма заказа — сумма его строк.</summary>
public sealed record Order(int Id, string Customer, DateOnly Date, OrderStatus Status, IReadOnlyList<OrderLine> Lines)
{
    public decimal Total => Lines.Sum(line => line.Total);
}

/// <summary>Продажи за месяц (задача 11.2).</summary>
public sealed record MonthlySales(int Year, int Month, int OrderCount, decimal Revenue);
