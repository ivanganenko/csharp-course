namespace Module13.Tasks;

public interface ICounter
{
    int Value { get; }

    void Increment();

    void Add(int amount);
}

public sealed class UnsafeCounter : ICounter
{
    private int _value;

    public int Value => _value;

    public void Increment() => _value++;

    public void Add(int amount) => _value += amount;
}

public sealed class LockCounter : ICounter
{
    // Приватный объект-замок: никто снаружи не сможет заблокироваться на нём
    // и устроить нам deadlock (в отличие от lock(this)).
    // System.Threading.Lock (.NET 9+) — специализированный тип, lock(...) с ним работает быстрее,
    // чем с обычным object.
    private readonly Lock _sync = new();
    private int _value;

    public int Value
    {
        get
        {
            // Чтение тоже под lock: гарантирует, что мы видим самое свежее значение
            // (lock даёт memory barrier).
            lock (_sync)
            {
                return _value;
            }
        }
    }

    public void Increment()
    {
        lock (_sync)
        {
            _value++;
        }
    }

    public void Add(int amount)
    {
        lock (_sync)
        {
            _value += amount;
        }
    }

    public bool TryIncrementUpTo(int limit)
    {
        // Check-then-act: проверка и изменение ОБЯЗАНЫ быть под одним lock.
        // Если проверить без lock, два потока одновременно увидят 499 < 500 и оба увеличат → 501.
        lock (_sync)
        {
            if (_value >= limit)
            {
                return false;
            }

            _value++;
            return true;
        }
    }
}

public sealed class InterlockedCounter : ICounter
{
    private int _value;

    // Volatile.Read — чтобы не прочитать устаревшее значение из кэша/регистра.
    public int Value => Volatile.Read(ref _value);

    // Interlocked выполняет «прочитать-изменить-записать» одной атомарной инструкцией процессора.
    // Быстрее lock, но подходит только для простых операций над одной переменной.
    public void Increment() => Interlocked.Increment(ref _value);

    public void Add(int amount) => Interlocked.Add(ref _value, amount);
}
