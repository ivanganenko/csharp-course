namespace Module05.Tasks;

/// <summary>Получатель уведомления. Уже готов — менять не нужно.</summary>
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

/// <summary>
/// Задача 05.4 — канал доставки уведомлений. Интерфейс уже готов — менять не нужно.
/// Интерфейс — это контракт «что умеет делать», без «как» и без состояния.
/// </summary>
public interface INotificationChannel
{
    /// <summary>Имя канала, например "Email".</summary>
    string Name { get; }

    /// <summary>Может ли канал доставить сообщение этому получателю.</summary>
    bool CanSend(Recipient recipient);

    /// <summary>Отправить сообщение. Если CanSend(recipient) == false → InvalidOperationException.</summary>
    void Send(Recipient recipient, string message);

    /// <summary>
    /// Default interface method (C# 8): реализация по умолчанию прямо в интерфейсе.
    /// Классы могут не реализовывать этот метод — тогда используется эта версия.
    /// Важно: вызвать его можно только через ссылку типа интерфейса: ((INotificationChannel)this).Format(...).
    /// </summary>
    string Format(string message) => $"[{Name}] {message}";
}

/// <summary>
/// Email-канал. Name → "Email". CanSend → у получателя есть непустой Email.
/// Send записывает в Sent строку "{recipient.Email}: {Format(message)}", где Format — метод интерфейса
/// по умолчанию: для "Hello" получится "anna@example.com: [Email] Hello".
/// Реальной отправки нет — только запись в список (так канал легко проверить тестом).
/// </summary>
public sealed class EmailChannel : INotificationChannel
{
    public string Name => throw new NotImplementedException();

    /// <summary>Отправленные сообщения в порядке отправки.</summary>
    public IReadOnlyList<string> Sent => throw new NotImplementedException();

    public bool CanSend(Recipient recipient)
    {
        throw new NotImplementedException();
    }

    public void Send(Recipient recipient, string message)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// SMS-канал. Name → "SMS". CanSend → у получателя есть непустой Phone.
/// SMS короткие, поэтому канал ЗАМЕНЯЕТ реализацию Format по умолчанию:
/// сообщение без префикса, обрезанное до MaxLength символов ("Hi" → "Hi"; 100 символов → первые 70).
/// Реализуй Format через explicit interface implementation:
///     string INotificationChannel.Format(string message) { ... }
/// Send записывает в Sent строку "{recipient.Phone}: {Format(message)}".
/// </summary>
public sealed class SmsChannel : INotificationChannel
{
    public const int MaxLength = 70;

    public string Name => throw new NotImplementedException();

    /// <summary>Отправленные сообщения в порядке отправки.</summary>
    public IReadOnlyList<string> Sent => throw new NotImplementedException();

    public bool CanSend(Recipient recipient)
    {
        throw new NotImplementedException();
    }

    public void Send(Recipient recipient, string message)
    {
        throw new NotImplementedException();
    }

    // TODO: explicit implementation INotificationChannel.Format (см. комментарий к классу).
}

/// <summary>
/// Сервис рассылки. Ничего не знает о конкретных каналах — работает только с INotificationChannel.
/// Новый канал (Telegram, Push) добавляется без изменения этого класса.
/// </summary>
public sealed class NotificationService
{
    /// <summary>channels == null → ArgumentNullException. Список каналов копируется.</summary>
    public NotificationService(IEnumerable<INotificationChannel> channels)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Отправляет message через КАЖДЫЙ канал, который может доставить его получателю (CanSend),
    /// в порядке каналов. Возвращает количество каналов, через которые сообщение ушло (0 — если ни один не подошёл).
    /// recipient == null → ArgumentNullException; message null/пусто/пробелы → ArgumentException (или наследник).
    /// </summary>
    public int Notify(Recipient recipient, string message)
    {
        throw new NotImplementedException();
    }
}
