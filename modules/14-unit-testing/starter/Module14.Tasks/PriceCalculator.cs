namespace Module14.Tasks;

/// <summary>
/// Задача 14.1 — ГОТОВЫЙ код, к которому нужно написать тесты.
/// ⚠ В этом коде есть баг, который не ловят тесты-образцы. Найди его тестами и исправь.
/// </summary>
public static class PriceCalculator
{
    public const decimal DiscountThreshold = 1000m;
    public const decimal DiscountRate = 0.10m;

    /// <summary>
    /// Итог заказа:
    /// - subtotal = Σ Quantity × UnitPrice;
    /// - если subtotal &gt;= 1000 — скидка 10%;
    /// - результат округляется до 2 знаков (MidpointRounding.AwayFromZero).
    /// Ошибки: lines == null → ArgumentNullException; пустой список → ArgumentException;
    /// Quantity &lt;= 0 или UnitPrice &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal CalculateTotal(IReadOnlyList<OrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one line.", nameof(lines));
        }

        decimal subtotal = 0;
        foreach (var line in lines)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(line.Quantity);
            ArgumentOutOfRangeException.ThrowIfNegative(line.UnitPrice);
            subtotal += line.Quantity * line.UnitPrice;
        }

        var total = subtotal > DiscountThreshold
            ? subtotal * (1 - DiscountRate)
            : subtotal;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
