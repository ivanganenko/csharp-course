namespace Module10.Tasks;

/// <summary>
/// Задача 10.4 — closures (замыкания): лямбды, которые «захватывают» переменные.
/// Тесты: ClosureLabTests.
/// </summary>
public static class ClosureLab
{
    /// <summary>
    /// Возвращает функцию-счётчик: первый вызов → 1, второй → 2 и т.д.
    /// Каждый вызов CreateCounter создаёт НЕЗАВИСИМЫЙ счётчик. Без static-полей.
    /// </summary>
    public static Func<int> CreateCounter()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает список из <paramref name="count"/> функций; функция с индексом i возвращает i * 10.
    /// Пример: count = 3 → функции, возвращающие 0, 10, 20.
    /// Осторожно: ловушка с захватом переменной цикла for!
    /// count &lt; 0 → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public static IReadOnlyList<Func<int>> CreateMultipliers(int count)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает логгер: при вызове logger("text") в <paramref name="sink"/> добавляется "[prefix] text".
    /// null sink → <see cref="ArgumentNullException"/>; null prefix → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static Action<string> CreateLogger(ICollection<string> sink, string prefix)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Композиция функций: возвращает x => second(first(x)).
    /// null-аргументы → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static Func<int, int> Compose(Func<int, int> first, Func<int, int> second)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Мемоизация: возвращает функцию, которая для каждого аргумента вызывает <paramref name="function"/>
    /// только ОДИН раз, а повторные вызовы с тем же аргументом берёт из кэша (Dictionary в замыкании).
    /// null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static Func<T, TResult> Memoize<T, TResult>(Func<T, TResult> function) where T : notnull
    {
        throw new NotImplementedException();
    }
}
