namespace Module17.Tasks;

/// <summary>
/// «Сторонняя библиотека» рассылки (вымышленная). МЕНЯТЬ НЕЛЬЗЯ — представь, что это NuGet-пакет.
/// Её интерфейс не совпадает с нашим <see cref="INotificationSender"/>: другая сигнатура,
/// нужен subject, тело — HTML, ошибки сообщаются кодом возврата, а не исключением.
/// </summary>
public sealed class LegacyMailGateway
{
    public const int Ok = 0;
    public const int InvalidAddress = 1;
    public const int EmptyBody = 2;

    private readonly List<SentMail> _sent = new();

    /// <summary>Отправленные письма (для тестов).</summary>
    public IReadOnlyList<SentMail> Sent => _sent;

    /// <summary>Возвращает 0 при успехе, 1 — некорректный адрес, 2 — пустое тело.</summary>
    public int SendMail(string address, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(address) || !address.Contains('@'))
        {
            return InvalidAddress;
        }

        if (string.IsNullOrEmpty(htmlBody))
        {
            return EmptyBody;
        }

        _sent.Add(new SentMail(address, subject, htmlBody));
        return Ok;
    }
}

public sealed record SentMail(string Address, string Subject, string HtmlBody);
