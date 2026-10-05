namespace Module17.Tasks;

/// <summary>
/// Задача 2. Экспресс: 900, плюс 500 при весе строго больше 5 кг.
/// В воскресенье (по OrderDate) не работает → InvalidOperationException.
/// </summary>
public sealed class ExpressDelivery : IDeliveryMethod
{
    public const decimal BasePrice = 900m;
    public const decimal HeavySurcharge = 500m;
    public const decimal HeavyFromKg = 5m;

    public string Code => "express";

    public string DisplayName => "экспресс";

    public decimal CalculateCost(DeliveryContext context)
    {
        throw new NotImplementedException();
    }
}
