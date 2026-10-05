using System.Globalization;

namespace Module14.Tasks;

/// <summary>
/// Сервис заказов. PlaceOrderAsync — ГОТОВ (задача 14.4: покрыть тестами с NSubstitute).
/// CancelOrderAsync — НЕ реализован (задача 14.5: TDD — сначала тесты, потом код).
/// </summary>
public sealed class OrderService
{
    public static readonly TimeSpan CancellationWindow = TimeSpan.FromDays(14);

    private readonly IPaymentGateway _paymentGateway;
    private readonly IEmailSender _emailSender;
    private readonly IClock _clock;

    public OrderService(IPaymentGateway paymentGateway, IEmailSender emailSender, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(paymentGateway);
        ArgumentNullException.ThrowIfNull(emailSender);
        ArgumentNullException.ThrowIfNull(clock);
        _paymentGateway = paymentGateway;
        _emailSender = emailSender;
        _clock = clock;
    }

    /// <summary>
    /// Оформляет заказ:
    /// 1. customerEmail пустой/пробелы → ArgumentException (платёж не вызывается).
    /// 2. Total = PriceCalculator.CalculateTotal(lines); CreatedAt = clock.UtcNow.
    /// 3. Списывает Total через ChargeAsync(customerEmail, Total, token).
    /// 4. Успех → Status = Paid, PaidAt = clock.UtcNow, TransactionId из ответа,
    ///    письмо на customerEmail с темой "Order confirmed".
    /// 5. Отказ → Status = PaymentFailed, письмо НЕ отправляется.
    /// 6. Исключение из шлюза пробрасывается, письмо не отправляется.
    /// </summary>
    public async Task<Order> PlaceOrderAsync(
        string customerEmail,
        IReadOnlyList<OrderLine> lines,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);

        var total = PriceCalculator.CalculateTotal(lines);
        var order = new Order
        {
            CustomerEmail = customerEmail,
            Lines = lines,
            Total = total,
            CreatedAt = _clock.UtcNow,
        };

        var payment = await _paymentGateway.ChargeAsync(customerEmail, total, cancellationToken);
        if (!payment.IsSuccess)
        {
            order.Status = OrderStatus.PaymentFailed;
            return order;
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = _clock.UtcNow;
        order.TransactionId = payment.TransactionId;

        await _emailSender.SendAsync(
            customerEmail,
            "Order confirmed",
            $"Order {order.Id} is paid. Total: {total.ToString("0.00", CultureInfo.InvariantCulture)}.",
            cancellationToken);

        return order;
    }

    /// <summary>
    /// Отменяет заказ с возвратом денег. Правила:
    /// - order == null → ArgumentNullException.
    /// - Отменить можно только заказ со Status == Paid (и заполненными PaidAt и TransactionId);
    ///   иначе InvalidOperationException, шлюз не вызывается.
    /// - Если с момента оплаты прошло БОЛЬШЕ CancellationWindow (14 дней) по clock.UtcNow →
    ///   InvalidOperationException, шлюз не вызывается. Ровно 14 дней — ещё можно.
    /// - Вызывает RefundAsync(order.TransactionId, order.Total, token).
    ///   false → InvalidOperationException, статус не меняется, письмо не отправляется.
    /// - Успех → Status = Cancelled, CancelledAt = clock.UtcNow,
    ///   письмо на order.CustomerEmail с темой "Order cancelled".
    /// </summary>
    public Task CancelOrderAsync(Order order, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
