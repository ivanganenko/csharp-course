namespace Module17.Tasks;

public sealed class PostDelivery : IDeliveryMethod
{
    public const decimal BasePrice = 250m;
    public const decimal PricePerExtraKg = 60m;

    public string Code => "post";

    public string DisplayName => "почта";

    public decimal CalculateCost(DeliveryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // «Каждый начатый килограмм сверх первого» = Ceiling(вес − 1), но не меньше нуля.
        var extraKg = Math.Ceiling(Math.Max(0m, context.TotalWeightKg - 1m));
        return BasePrice + PricePerExtraKg * extraKg;
    }
}
