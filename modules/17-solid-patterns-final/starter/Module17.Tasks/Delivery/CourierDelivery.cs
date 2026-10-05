namespace Module17.Tasks;

/// <summary>
/// Задача 2. Курьер: 350, но бесплатно, если сумма после скидки ≥ <see cref="FreeFrom"/> (5000).
/// </summary>
public sealed class CourierDelivery : IDeliveryMethod
{
    public const decimal Price = 350m;
    public const decimal FreeFrom = 5000m;

    public string Code => "courier";

    public string DisplayName => "курьер";

    public decimal CalculateCost(DeliveryContext context)
    {
        throw new NotImplementedException();
    }
}
