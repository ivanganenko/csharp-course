using Module14.Tasks;

namespace Module14.Tests;

/// <summary>
/// Stub/fake часов: возвращает заданное время, которое тест может «перемотать».
/// Самописный fake проще и читабельнее мока, когда зависимость — простое значение.
/// </summary>
public sealed class FakeClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = now;

    public void Advance(TimeSpan by) => UtcNow += by;
}

public sealed record SentEmail(string To, string Subject, string Body);

/// <summary>
/// Fake (точнее — spy): ничего не отправляет, а запоминает письма, чтобы тест мог их проверить.
/// Проверка состояния (что лежит в Sent) обычно устойчивее к рефакторингу, чем проверка вызовов мока.
/// </summary>
public sealed class FakeEmailSender : IEmailSender
{
    private readonly List<SentEmail> _sent = [];

    public IReadOnlyList<SentEmail> Sent => _sent;

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken)
    {
        _sent.Add(new SentEmail(to, subject, body));
        return Task.CompletedTask;
    }
}
