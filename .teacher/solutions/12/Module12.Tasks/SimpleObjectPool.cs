namespace Module12.Tasks;

public sealed class SimpleObjectPool<T> where T : class
{
    private readonly Func<T> _factory;
    private readonly Action<T>? _reset;
    private readonly int _maxRetained;

    // Stack даёт LIFO: последний возвращённый объект «тёплый» (скорее всего в кэше процессора).
    private readonly Stack<T> _items = new();

    // Один объект-замок защищает И стек, И счётчик: операции «проверить размер и положить»
    // должны быть атомарными вместе (check-then-act), Interlocked тут не хватит.
    private readonly Lock _sync = new();

    private int _createdCount;

    public SimpleObjectPool(Func<T> factory, int maxRetained, Action<T>? reset = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRetained);

        _factory = factory;
        _maxRetained = maxRetained;
        _reset = reset;
    }

    public int AvailableCount
    {
        get
        {
            lock (_sync)
            {
                return _items.Count;
            }
        }
    }

    public int CreatedCount => Volatile.Read(ref _createdCount);

    public T Rent()
    {
        lock (_sync)
        {
            if (_items.TryPop(out var item))
            {
                return item;
            }
        }

        // factory вызываем ВНЕ lock: создание может быть долгим (или вызывать чужой код),
        // держать замок в это время — значит блокировать всех остальных.
        Interlocked.Increment(ref _createdCount);
        return _factory();
    }

    public void Return(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // reset тоже вне lock — объект сейчас принадлежит только вызывающему потоку.
        _reset?.Invoke(item);

        lock (_sync)
        {
            if (_items.Count < _maxRetained)
            {
                _items.Push(item);
            }

            // Иначе просто «забываем» объект: на него больше нет ссылок, его соберёт GC.
            // Ограничение размера — защита от неограниченного роста памяти (это тоже утечка!).
        }
    }
}
