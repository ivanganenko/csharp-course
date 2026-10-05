namespace Module08.Tasks;

public abstract record Message(string Text);

public sealed record EmailMessage(string Text, string To) : Message(Text);

public sealed record SmsMessage(string Text, string Phone) : Message(Text);

// out T — covariance: T встречается ТОЛЬКО в выходных позициях (возвращаемое значение).
// Это безопасно: кто ждёт Message, получит EmailMessage — а EmailMessage и есть Message.
// Компилятор запретит добавить сюда метод void Add(T item) — иначе через IMessageSource<Message>
// можно было бы положить SmsMessage в источник email-ов.
public interface IMessageSource<out T> where T : Message
{
    T? Next();
}

// in T — contravariance: T встречается ТОЛЬКО во входных позициях (параметры).
// Обработчик, умеющий работать с любым Message, точно справится с EmailMessage.
public interface IMessageHandler<in T> where T : Message
{
    void Handle(T message);
}

// Классы не могут быть вариантными — только интерфейсы и делегаты.
// Поэтому вариантность «включается» через интерфейс, который класс реализует.
public class QueueMessageSource<T> : IMessageSource<T> where T : Message
{
    private readonly Queue<T> _messages;

    public QueueMessageSource(IEnumerable<T> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        // Копируем вход: внешний код не должен иметь возможности менять наш источник после создания.
        _messages = new Queue<T>(messages);
    }

    // Queue<T> — FIFO, ровно то, что нужно; TryDequeue без исключения на пустой очереди.
    public T? Next() => _messages.TryDequeue(out T? message) ? message : null;
}

public class LoggingHandler : IMessageHandler<Message>
{
    private readonly List<string> _log = new();

    // Наружу — IReadOnlyList: читать можно, менять журнал в обход Handle — нельзя.
    public IReadOnlyList<string> Log => _log;

    public void Handle(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        // GetType() — фактический (runtime) тип, а не статический Message.
        _log.Add($"{message.GetType().Name}: {message.Text}");
    }
}

public static class MessagePump
{
    public static int Drain<T>(IMessageSource<T> source, IMessageHandler<T> handler) where T : Message
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(handler);

        int count = 0;
        // Pattern matching `is { } message` — «не null» + объявление переменной в одном выражении.
        while (source.Next() is { } message)
        {
            handler.Handle(message);
            count++;
        }

        return count;
    }
}
