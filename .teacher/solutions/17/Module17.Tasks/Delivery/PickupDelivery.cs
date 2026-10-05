namespace Module17.Tasks;

public sealed class PickupDelivery : IDeliveryMethod
{
    public string Code => "pickup";

    public string DisplayName => "самовывоз";

    public decimal CalculateCost(DeliveryContext context) => 0m;
}
