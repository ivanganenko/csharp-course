namespace Module14.Tasks;

// ГОТОВЫЙ production-код модуля 14: доменные типы и зависимости сервиса заказов.
// Зависимости — интерфейсы, поэтому в тестах их можно подменить (test doubles).

/// <summary>Строка заказа: артикул, количество, цена за единицу.</summary>
public sealed record OrderLine(string Sku, int Quantity, decimal UnitPrice);

public enum OrderStatus
{
    New,
    Paid,
    PaymentFailed,
    Cancelled,
}

/// <summary>
/// Заказ. Упрощённая модель с публичными сеттерами — чтобы в тестах было легко
/// подготовить заказ в нужном состоянии (в реальном домене мы бы защитили инварианты, см. модуль 04).
/// </summary>
public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string CustomerEmail { get; init; }

    public IReadOnlyList<OrderLine> Lines { get; init; } = [];

    public decimal Total { get; init; }

    public OrderStatus Status { get; set; } = OrderStatus.New;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PaidAt { get; set; }

    public string? TransactionId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}

/// <summary>Результат списания денег.</summary>
public sealed record PaymentResult(bool IsSuccess, string? TransactionId, string? ErrorMessage)
{
    public static PaymentResult Success(string transactionId) => new(true, transactionId, null);

    public static PaymentResult Failure(string errorMessage) => new(false, null, errorMessage);
}

/// <summary>
/// Источник текущего времени. Почему не DateTime.UtcNow напрямую: «сейчас» в тесте
/// каждый раз разное — такой код нельзя проверить детерминированно.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

/// <summary>Реальная реализация для production.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

/// <summary>Платёжный шлюз (в реальности — HTTP API банка).</summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(string customerEmail, decimal amount, CancellationToken cancellationToken);

    /// <summary>Возврат денег. true — возврат проведён, false — банк отказал.</summary>
    Task<bool> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken);
}

/// <summary>Отправка писем (в реальности — SMTP или внешний сервис).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken);
}
