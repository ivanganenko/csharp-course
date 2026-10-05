namespace Module17.Tasks;

// Вместо switch по строке — словарь стратегий. Добавить способ доставки = написать класс и
// зарегистрировать его (в ASP.NET Core: services.AddSingleton<IDeliveryMethod, DroneDelivery>(),
// а реестр получит IEnumerable<IDeliveryMethod> из DI). OrderProcessor при этом не меняется — OCP.
public sealed class DeliveryMethodRegistry
{
    private readonly Dictionary<string, IDeliveryMethod> _methods;

    public DeliveryMethodRegistry(IEnumerable<IDeliveryMethod> methods)
    {
        ArgumentNullException.ThrowIfNull(methods);

        _methods = new Dictionary<string, IDeliveryMethod>(StringComparer.OrdinalIgnoreCase);
        foreach (var method in methods)
        {
            // TryAdd вместо индексатора: дубликат — ошибка конфигурации, лучше упасть сразу при старте,
            // чем молча «победит» последний зарегистрированный.
            if (!_methods.TryAdd(method.Code, method))
            {
                throw new ArgumentException($"Способ доставки '{method.Code}' зарегистрирован дважды", nameof(methods));
            }
        }
    }

    public IReadOnlyCollection<string> Codes => _methods.Keys;

    public IDeliveryMethod Get(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Не указан способ доставки", nameof(code));
        }

        if (!_methods.TryGetValue(code.Trim(), out var method))
        {
            throw new ArgumentException($"Неизвестный способ доставки: {code}", nameof(code));
        }

        return method;
    }

    public static DeliveryMethodRegistry CreateDefault() =>
        new(new IDeliveryMethod[] { new PickupDelivery(), new CourierDelivery(), new PostDelivery(), new ExpressDelivery() });
}
