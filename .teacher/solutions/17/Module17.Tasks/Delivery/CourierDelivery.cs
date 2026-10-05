namespace Module17.Tasks;

public sealed class CourierDelivery : IDeliveryMethod
{
    public const decimal Price = 350m;
    public const decimal FreeFrom = 5000m;

    public string Code => "courier";

    public string DisplayName => "курьер";

    public decimal CalculateCost(DeliveryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Граница «≥ 5000» включительно — так зафиксировано характеризационными тестами.
        return context.AmountAfterDiscount >= FreeFrom ? 0m : Price;
    }
}
