namespace Module14.Tasks;

public enum DeliveryType
{
    Standard,
    Express,
}

/// <summary>
/// Задача 14.2 — TDD: сначала тесты в ShippingCalculatorTests, потом реализация.
/// </summary>
public static class ShippingCalculator
{
    public const decimal FreeShippingThreshold = 3000m;

    /// <summary>
    /// Стоимость доставки:
    /// - Standard: 300; бесплатно (0), если orderTotal &gt;= 3000.
    /// - Express: 600 + 50 за каждый НАЧАТЫЙ килограмм сверх 5 кг (5.0 → 600, 5.1 → 650, 7 → 700).
    ///   Express никогда не бывает бесплатной.
    /// Ошибки: orderTotal &lt; 0 → ArgumentOutOfRangeException; weightKg &lt;= 0 → ArgumentOutOfRangeException;
    /// неизвестный DeliveryType (например, (DeliveryType)42) → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal Calculate(decimal orderTotal, decimal weightKg, DeliveryType type)
    {
        throw new NotImplementedException();
    }
}
