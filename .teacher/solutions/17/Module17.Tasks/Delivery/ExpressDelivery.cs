namespace Module17.Tasks;

public sealed class ExpressDelivery : IDeliveryMethod
{
    public const decimal BasePrice = 900m;
    public const decimal HeavySurcharge = 500m;
    public const decimal HeavyFromKg = 5m;

    public string Code => "express";

    public string DisplayName => "экспресс";

    public decimal CalculateCost(DeliveryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Дата приходит в контексте, а не берётся из DateTime.Now — правило тестируемо и детерминировано.
        if (context.OrderDate.DayOfWeek == DayOfWeek.Sunday)
        {
            throw new InvalidOperationException("Экспресс-доставка не работает по воскресеньям");
        }

        return context.TotalWeightKg > HeavyFromKg ? BasePrice + HeavySurcharge : BasePrice;
    }
}
