namespace Module17.Tasks;

/// <summary>Данные, нужные для расчёта доставки.</summary>
public sealed record DeliveryContext(decimal AmountAfterDiscount, decimal TotalWeightKg, DateOnly OrderDate);

/// <summary>Способ доставки (Strategy). Каждый способ — отдельный класс вместо ветки switch.</summary>
public interface IDeliveryMethod
{
    /// <summary>Код способа в нижнем регистре: "pickup", "courier", …</summary>
    string Code { get; }

    /// <summary>Название для сообщения клиенту: "самовывоз", "курьер", …</summary>
    string DisplayName { get; }

    decimal CalculateCost(DeliveryContext context);
}
