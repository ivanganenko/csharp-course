namespace Module05.Tasks;

public sealed class Recipient
{
    public Recipient(string name, string? email = null, string? phone = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Email = email;
        Phone = phone;
    }

    public string Name { get; }

    public string? Email { get; }

    public string? Phone { get; }
}

public interface INotificationChannel
{
    string Name { get; }

    bool CanSend(Recipient recipient);

    void Send(Recipient recipient, string message);

    string Format(string message) => $"[{Name}] {message}";
}

public sealed class EmailChannel : INotificationChannel
{
    private readonly List<string> _sent = new();

    public string Name => "Email";

    public IReadOnlyList<string> Sent => _sent.AsReadOnly();

    public bool CanSend(Recipient recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);
        return !string.IsNullOrWhiteSpace(recipient.Email);
    }

    public void Send(Recipient recipient, string message)
    {
        if (!CanSend(recipient))
        {
            throw new InvalidOperationException($"Recipient '{recipient.Name}' has no email.");
        }

        // Default interface method не становится членом класса: this.Format(...) не скомпилируется.
        // Его можно вызвать только через ссылку типа интерфейса.
        var formatted = ((INotificationChannel)this).Format(message);
        _sent.Add($"{recipient.Email}: {formatted}");
    }
}

public sealed class SmsChannel : INotificationChannel
{
    public const int MaxLength = 70;

    private readonly List<string> _sent = new();

    public string Name => "SMS";

    public IReadOnlyList<string> Sent => _sent.AsReadOnly();

    public bool CanSend(Recipient recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);
        return !string.IsNullOrWhiteSpace(recipient.Phone);
    }

    public void Send(Recipient recipient, string message)
    {
        if (!CanSend(recipient))
        {
            throw new InvalidOperationException($"Recipient '{recipient.Name}' has no phone.");
        }

        _sent.Add($"{recipient.Phone}: {((INotificationChannel)this).Format(message)}");
    }

    // Explicit interface implementation: метод виден только через ссылку INotificationChannel
    // и заменяет реализацию по умолчанию. Публичный API класса не «засоряется» деталью форматирования.
    string INotificationChannel.Format(string message) =>
        message.Length <= MaxLength ? message : message[..MaxLength];
}

public sealed class NotificationService
{
    // Зависим от абстракции (интерфейса), а не от EmailChannel/SmsChannel:
    // новый канал не требует правок здесь (Open/Closed), а в тестах можно подставить fake.
    private readonly IReadOnlyList<INotificationChannel> _channels;

    public NotificationService(IEnumerable<INotificationChannel> channels)
    {
        ArgumentNullException.ThrowIfNull(channels);

        // Копируем: если вызывающий код потом изменит свой список, сервис этого не заметит.
        _channels = channels.ToList();
    }

    public int Notify(Recipient recipient, string message)
    {
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        int sent = 0;
        foreach (var channel in _channels)
        {
            // Полиморфизм: один и тот же вызов — разное поведение в зависимости от реального типа канала.
            if (channel.CanSend(recipient))
            {
                channel.Send(recipient, message);
                sent++;
            }
        }

        return sent;
    }
}
