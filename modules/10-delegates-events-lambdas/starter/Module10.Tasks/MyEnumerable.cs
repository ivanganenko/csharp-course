namespace Module10.Tasks;

/// <summary>
/// Задача 10.1 — свои аналоги Where/Select/Aggregate/Count на делегатах.
/// LINQ внутри не используйте — в этом смысл задачи.
/// Тесты: MyEnumerableTests.
/// </summary>
public static class MyEnumerable
{
    /// <summary>
    /// Аналог Where: лениво возвращает элементы, для которых <paramref name="predicate"/> вернул true.
    /// predicate не вызывается, пока результат не начали перебирать.
    /// null-аргументы → <see cref="ArgumentNullException"/> СРАЗУ при вызове.
    /// </summary>
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Аналог Select: лениво преобразует каждый элемент через <paramref name="selector"/>.
    /// null-аргументы → <see cref="ArgumentNullException"/> СРАЗУ при вызове.
    /// </summary>
    public static IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Аналог Aggregate: «сворачивает» последовательность в одно значение.
    /// Начинает с <paramref name="seed"/>, для каждого элемента: acc = accumulator(acc, item). Пусто → seed.
    /// Выполняется сразу (не лениво). null source/accumulator → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static TAccumulate Reduce<T, TAccumulate>(
        this IEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> accumulator)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Считает элементы, удовлетворяющие условию. Обратите внимание: здесь <see cref="Predicate{T}"/>,
    /// а не Func&lt;T, bool&gt; — почему это разные типы, хотя сигнатуры одинаковые? (см. README)
    /// null-аргументы → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static int CountWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        throw new NotImplementedException();
    }
}
