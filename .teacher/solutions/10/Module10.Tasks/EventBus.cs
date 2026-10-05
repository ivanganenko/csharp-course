namespace Module10.Tasks;

public class EventBus
{
    // Ключ — тип сообщения, значение — список обработчиков. Delegate — общий базовый тип,
    // потому что в одном словаре лежат Action<OrderPlaced>, Action<UserRegistered> и т.д.
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (!_handlers.TryGetValue(typeof(TEvent), out List<Delegate>? existing))
        {
            existing = new List<Delegate>();
            _handlers.Add(typeof(TEvent), existing);
        }

        List<Delegate> handlers = existing;

        // Оборачиваем в отдельный объект-«билет»: у каждой подписки свой target делегата,
        // поэтому Dispose удалит ровно эту подписку, даже если один и тот же handler подписан дважды
        // (List.Remove сравнивает делегаты через Delegate.Equals: target + метод).
        var entry = new HandlerEntry<TEvent>(handler);
        Action<TEvent> registered = entry.Invoke;
        handlers.Add(registered);

        // Замыкание захватывает список и конкретный делегат — объекту Subscription не нужно знать про шину.
        return new Subscription(() => handlers.Remove(registered));
    }

    public int Publish<TEvent>(TEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_handlers.TryGetValue(typeof(TEvent), out List<Delegate>? list) || list.Count == 0)
        {
            return 0;
        }

        // Снимок (snapshot): обработчик может отписаться/подписать нового во время рассылки.
        // Перебор самого списка дал бы InvalidOperationException или пропуск элементов.
        // Так же ведут себя multicast-делегаты: они immutable, вызов идёт по «старому» списку.
        Delegate[] snapshot = list.ToArray();
        foreach (Delegate handler in snapshot)
        {
            ((Action<TEvent>)handler)(message);
        }

        return snapshot.Length;
    }

    public int SubscriberCount<TEvent>() =>
        _handlers.TryGetValue(typeof(TEvent), out List<Delegate>? list) ? list.Count : 0;

    // Обёртка нужна, чтобы у каждой подписки был уникальный target делегата.
    private sealed class HandlerEntry<TEvent>
    {
        private readonly Action<TEvent> _handler;

        public HandlerEntry(Action<TEvent> handler) => _handler = handler;

        public void Invoke(TEvent message) => _handler(message);
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _unsubscribe;

        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;

        public void Dispose()
        {
            // Обнуляем действие — повторный Dispose ничего не делает (идемпотентность).
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }
    }
}
