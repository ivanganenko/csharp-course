namespace Module14.Tasks;

public enum DeliveryType
{
    Standard,
    Express,
}

public static class ShippingCalculator
{
    public const decimal FreeShippingThreshold = 3000m;

    // Именованные константы вместо «магических чисел»: тесты и код говорят на одном языке,
    // а при изменении тарифа правка в одном месте.
    private const decimal StandardPrice = 300m;
    private const decimal ExpressBasePrice = 600m;
    private const decimal ExpressIncludedWeightKg = 5m;
    private const decimal ExpressPricePerExtraKg = 50m;

    public static decimal Calculate(decimal orderTotal, decimal weightKg, DeliveryType type)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(orderTotal);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weightKg);

        return type switch
        {
            DeliveryType.Standard => orderTotal >= FreeShippingThreshold ? 0m : StandardPrice,
            DeliveryType.Express => ExpressBasePrice + ExpressPricePerExtraKg * ExtraStartedKilograms(weightKg),
            // enum в C# — просто число: (DeliveryType)42 скомпилируется, поэтому ветка по умолчанию обязательна.
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown delivery type."),
        };
    }

    // «Каждый начатый килограмм» = округление вверх: 0.1 кг сверх лимита — уже целый платный килограмм.
    private static decimal ExtraStartedKilograms(decimal weightKg) =>
        Math.Ceiling(Math.Max(0m, weightKg - ExpressIncludedWeightKg));
}
