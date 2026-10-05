namespace Module13.Tasks;

/// <summary>Счётчик, который могут одновременно увеличивать много потоков.</summary>
public interface ICounter
{
    int Value { get; }

    void Increment();

    void Add(int amount);
}

/// <summary>
/// ГОТОВО, НЕ МЕНЯТЬ. Небезопасный счётчик для эксперимента (см. задачу 13.3, часть «исследование»).
/// _value++ — это три операции: прочитать, прибавить, записать. Два потока могут прочитать
/// одно и то же значение — и один инкремент «потеряется» (race condition).
/// </summary>
public sealed class UnsafeCounter : ICounter
{
    private int _value;

    public int Value => _value;

    public void Increment() => _value++;

    public void Add(int amount) => _value += amount;
}

/// <summary>
/// Задача 13.3 (часть 1) — потокобезопасный счётчик на lock.
/// </summary>
public sealed class LockCounter : ICounter
{
    /// <summary>Текущее значение (чтение тоже должно быть синхронизировано).</summary>
    public int Value => throw new NotImplementedException();

    /// <summary>Увеличивает на 1. Потокобезопасно.</summary>
    public void Increment()
    {
        throw new NotImplementedException();
    }

    /// <summary>Прибавляет amount (может быть отрицательным). Потокобезопасно.</summary>
    public void Add(int amount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Увеличивает на 1, только если текущее значение меньше limit. Возвращает true, если увеличил.
    /// Проверка и увеличение должны быть ОДНОЙ атомарной операцией: при 10 000 параллельных вызовах
    /// с limit = 500 ровно 500 вызовов вернут true.
    /// </summary>
    public bool TryIncrementUpTo(int limit)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Задача 13.3 (часть 2) — потокобезопасный счётчик без lock, через класс Interlocked.
/// </summary>
public sealed class InterlockedCounter : ICounter
{
    /// <summary>Текущее значение (подсказка: Volatile.Read или Interlocked.CompareExchange).</summary>
    public int Value => throw new NotImplementedException();

    /// <summary>Увеличивает на 1 атомарно.</summary>
    public void Increment()
    {
        throw new NotImplementedException();
    }

    /// <summary>Прибавляет amount атомарно.</summary>
    public void Add(int amount)
    {
        throw new NotImplementedException();
    }
}
