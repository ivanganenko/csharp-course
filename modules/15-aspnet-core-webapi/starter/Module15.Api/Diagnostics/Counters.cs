namespace Module15.Api.Diagnostics;

/// <summary>
/// Счётчик для демонстрации DI lifetimes (задача 4). Готов, менять не нужно.
/// Interlocked — потому что Singleton-экземпляр могут одновременно дёргать несколько запросов.
/// </summary>
public abstract class Counter
{
    private int _value;

    public int Value => Volatile.Read(ref _value);

    public int Increment() => Interlocked.Increment(ref _value);
}

/// <summary>Должен быть зарегистрирован как Singleton.</summary>
public sealed class SingletonCounter : Counter
{
}

/// <summary>Должен быть зарегистрирован как Scoped.</summary>
public sealed class ScopedCounter : Counter
{
}

/// <summary>Должен быть зарегистрирован как Transient.</summary>
public sealed class TransientCounter : Counter
{
}
