namespace Module17.Tasks;

/// <summary>
/// Задача 2. Почта: 250 + 60 за каждый НАЧАТЫЙ килограмм сверх первого.
/// Примеры: 0.3 кг → 250; 1.0 → 250; 1.01 → 310; 3.0 → 370; 6.5 → 610.
/// </summary>
public sealed class PostDelivery : IDeliveryMethod
{
    public const decimal BasePrice = 250m;
    public const decimal PricePerExtraKg = 60m;

    public string Code => "post";

    public string DisplayName => "почта";

    public decimal CalculateCost(DeliveryContext context)
    {
        throw new NotImplementedException();
    }
}
