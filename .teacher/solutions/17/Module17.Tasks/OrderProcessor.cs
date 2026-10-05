using System.Globalization;

namespace Module17.Tasks;

/// <summary>
/// После рефакторинга OrderProcessor — «дирижёр» (orchestrator): он только задаёт ПОРЯДОК шагов,
/// а каждое правило живёт в своём классе за абстракцией.
/// <list type="bullet">
/// <item>SRP — цены (каталог), скидки, доставка, хранение, уведомления — у каждого своя причина изменения.</item>
/// <item>OCP — новый способ доставки или скидки добавляется новым классом, этот файл не меняется.</item>
/// <item>LSP — любая реализация IDeliveryMethod/IDiscountPolicy подставляется без сюрпризов.</item>
/// <item>ISP — интерфейсы маленькие (один-три метода), никто не зависит от лишнего.</item>
/// <item>DIP — зависимость от абстракций, реализации приходят через конструктор.</item>
/// </list>
/// </summary>
public sealed class OrderProcessor
{
    private readonly IProductCatalog _catalog;
    private readonly IDiscountPolicy _discountPolicy;
    private readonly DeliveryMethodRegistry _deliveryMethods;
    private readonly IOrderRepository _repository;
    private readonly INotificationSender _notificationSender;

    // Фасад для старых вызывающих сторон (и характеризационных тестов) сохранён:
    // конструктор без параметров собирает стандартные реализации. Это «composition root в миниатюре»;
    // в ASP.NET Core ту же сборку делал бы DI-контейнер, а этот конструктор можно было бы удалить.
    public OrderProcessor()
        : this(
            InMemoryProductCatalog.CreateDefault(),
            new TierDiscountPolicy(),
            DeliveryMethodRegistry.CreateDefault(),
            new InMemoryOrderRepository(),
            new ConsoleEmailSender())
    {
    }

    public OrderProcessor(
        IProductCatalog catalog,
        IDiscountPolicy discountPolicy,
        DeliveryMethodRegistry deliveryMethods,
        IOrderRepository repository,
        INotificationSender notificationSender)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(discountPolicy);
        ArgumentNullException.ThrowIfNull(deliveryMethods);
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(notificationSender);

        _catalog = catalog;
        _discountPolicy = discountPolicy;
        _deliveryMethods = deliveryMethods;
        _repository = repository;
        _notificationSender = notificationSender;
    }

    public OrderResult Process(OrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateCustomer(request.CustomerEmail);

        // Порядок шагов сохранён как в legacy: сначала всё, что может упасть, и только потом —
        // номер, сохранение и уведомление (неудачный заказ не тратит номер и не шлёт письмо).
        var (subtotal, weight) = CalculateSubtotalAndWeight(request.Lines);
        var discount = _discountPolicy.CalculateDiscount(request.Tier, subtotal);
        var afterDiscount = subtotal - discount;

        var deliveryMethod = _deliveryMethods.Get(request.DeliveryType);
        var deliveryCost = deliveryMethod.CalculateCost(new DeliveryContext(afterDiscount, weight, request.OrderDate));
        var total = afterDiscount + deliveryCost;

        var number = _repository.NextOrderNumber(request.OrderDate);
        var message = BuildMessage(number, total, deliveryMethod.DisplayName);
        var result = new OrderResult(number, subtotal, discount, deliveryCost, total, deliveryMethod.Code, message);

        _repository.Save(result);
        _notificationSender.Send(request.CustomerEmail, message);
        return result;
    }

    public OrderResult? GetOrder(string orderNumber) => _repository.FindByNumber(orderNumber);

    private static void ValidateCustomer(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ArgumentException("Некорректный email");
        }
    }

    private (decimal Subtotal, decimal Weight) CalculateSubtotalAndWeight(IReadOnlyList<OrderLine> lines)
    {
        if (lines is null || lines.Count == 0)
        {
            throw new ArgumentException("Заказ пуст");
        }

        decimal subtotal = 0;
        decimal weight = 0;
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
            {
                throw new ArgumentException($"Количество должно быть больше нуля: {line.Sku}");
            }

            var product = _catalog.FindBySku(line.Sku)
                ?? throw new ArgumentException($"Неизвестный товар {line.Sku}");

            subtotal += product.Price * line.Quantity;
            weight += product.WeightKg * line.Quantity;
        }

        return (subtotal, weight);
    }

    private static string BuildMessage(string orderNumber, decimal total, string deliveryName) =>
        // InvariantCulture: на машине с ru-RU "0.00" дало бы "1775,00" — формат сообщения не должен зависеть от сервера.
        $"Заказ {orderNumber} оформлен. Сумма: {total.ToString("0.00", CultureInfo.InvariantCulture)} руб. Доставка: {deliveryName}";
}
