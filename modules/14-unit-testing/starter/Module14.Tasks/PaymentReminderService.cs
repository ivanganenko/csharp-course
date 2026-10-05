namespace Module14.Tasks;

/// <summary>
/// Задача 14.3 — ГОТОВЫЙ код. Напиши к нему тесты с самописными fakes (FakeClock, FakeEmailSender).
/// Напоминает клиентам, у которых не прошла оплата, повторить её.
/// </summary>
public sealed class PaymentReminderService
{
    public static readonly TimeSpan ReminderDelay = TimeSpan.FromDays(1);

    private readonly IClock _clock;
    private readonly IEmailSender _emailSender;

    // Зависимости приходят через конструктор (constructor injection) — поэтому сервис тестируемый.
    public PaymentReminderService(IClock clock, IEmailSender emailSender)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(emailSender);
        _clock = clock;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Для каждого заказа со статусом PaymentFailed, созданного ReminderDelay (1 сутки) назад или раньше,
    /// отправляет письмо на CustomerEmail с темой "Payment reminder".
    /// Остальные заказы пропускает. Возвращает количество отправленных писем.
    /// orders == null → ArgumentNullException.
    /// </summary>
    public async Task<int> SendRemindersAsync(IEnumerable<Order> orders, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var now = _clock.UtcNow;
        var sent = 0;
        foreach (var order in orders)
        {
            if (order.Status != OrderStatus.PaymentFailed || now - order.CreatedAt < ReminderDelay)
            {
                continue;
            }

            await _emailSender.SendAsync(
                order.CustomerEmail,
                "Payment reminder",
                $"Payment for order {order.Id} failed. Please try again.",
                cancellationToken);
            sent++;
        }

        return sent;
    }
}
