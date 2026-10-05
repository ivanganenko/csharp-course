namespace Module17.Tasks;

/// <summary>Задача 2. Самовывоз: всегда 0.</summary>
public sealed class PickupDelivery : IDeliveryMethod
{
    public string Code => "pickup";

    public string DisplayName => "самовывоз";

    public decimal CalculateCost(DeliveryContext context)
    {
        throw new NotImplementedException();
    }
}
