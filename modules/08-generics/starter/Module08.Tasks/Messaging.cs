namespace Module08.Tasks;

// Задача 08.6 ⭐ — covariance (out) и contravariance (in) на примере системы сообщений.
// Тесты: VarianceTests.
//
// Иерархия сообщений уже готова — менять её не нужно.

/// <summary>Базовое сообщение.</summary>
public abstract record Message(string Text);

/// <summary>Email-сообщение (адреса вымышленные, домен example.com).</summary>
public sealed record EmailMessage(string Text, string To) : Message(Text);

/// <summary>SMS-сообщение (номера вымышленные).</summary>
public sealed record SmsMessage(string Text, string Phone) : Message(Text);

/// <summary>
/// Источник сообщений: только ОТДАЁТ T наружу.
/// TODO: сделайте интерфейс ковариантным, чтобы источник email-сообщений можно было использовать
/// там, где ждут источник любых сообщений: IMessageSource&lt;EmailMessage&gt; → IMessageSource&lt;Message&gt;.
/// </summary>
public interface IMessageSource<T> where T : Message
{
    /// <summary>Следующее сообщение или null, если сообщения закончились.</summary>
    T? Next();
}

/// <summary>
/// Обработчик сообщений: только ПРИНИМАЕТ T.
/// TODO: сделайте интерфейс контравариантным, чтобы обработчик любых сообщений можно было использовать
/// там, где ждут обработчик email: IMessageHandler&lt;Message&gt; → IMessageHandler&lt;EmailMessage&gt;.
/// </summary>
public interface IMessageHandler<T> where T : Message
{
    void Handle(T message);
}

/// <summary>Источник, отдающий сообщения из заранее переданного набора в исходном порядке.</summary>
public class QueueMessageSource<T> : IMessageSource<T> where T : Message
{
    /// <summary>null вместо набора → <see cref="ArgumentNullException"/>. Набор копируется при создании.</summary>
    public QueueMessageSource(IEnumerable<T> messages)
    {
        throw new NotImplementedException();
    }

    /// <summary>Возвращает сообщения по одному в исходном порядке; когда они закончились — null.</summary>
    public T? Next()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Обработчик ЛЮБЫХ сообщений: записывает в <see cref="Log"/> строку
/// "{имя типа сообщения}: {Text}", например "EmailMessage: Hello".
/// </summary>
public class LoggingHandler : IMessageHandler<Message>
{
    /// <summary>Записи журнала в порядке обработки.</summary>
    public IReadOnlyList<string> Log => throw new NotImplementedException();

    /// <summary>null → <see cref="ArgumentNullException"/>.</summary>
    public void Handle(Message message)
    {
        throw new NotImplementedException();
    }
}

/// <summary>Перекачивает сообщения из источника в обработчик.</summary>
public static class MessagePump
{
    /// <summary>
    /// Забирает из <paramref name="source"/> все сообщения (пока Next() не вернёт null),
    /// передаёт каждое в <paramref name="handler"/> и возвращает количество обработанных.
    /// null-аргументы → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static int Drain<T>(IMessageSource<T> source, IMessageHandler<T> handler) where T : Message
    {
        throw new NotImplementedException();
    }
}
