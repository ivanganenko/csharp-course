namespace Module17.Tasks;

/// <summary>
/// Задача 2. Реестр способов доставки: находит стратегию по коду.
/// Новый способ доставки = новый класс + регистрация; OrderProcessor не меняется (OCP).
/// </summary>
public sealed class DeliveryMethodRegistry
{
    /// <summary>
    /// methods == null → ArgumentNullException.
    /// Два способа с одинаковым Code (без учёта регистра) → ArgumentException.
    /// </summary>
    public DeliveryMethodRegistry(IEnumerable<IDeliveryMethod> methods)
    {
        throw new NotImplementedException();
    }

    /// <summary>Коды всех зарегистрированных способов.</summary>
    public IReadOnlyCollection<string> Codes => throw new NotImplementedException();

    /// <summary>
    /// Способ доставки по коду: без учёта регистра и пробелов по краям (" Courier " → courier).
    /// null/пустой/неизвестный код → ArgumentException.
    /// </summary>
    public IDeliveryMethod Get(string? code)
    {
        throw new NotImplementedException();
    }

    /// <summary>Реестр со всеми четырьмя стандартными способами (уже готово).</summary>
    public static DeliveryMethodRegistry CreateDefault() =>
        new(new IDeliveryMethod[] { new PickupDelivery(), new CourierDelivery(), new PostDelivery(), new ExpressDelivery() });
}
