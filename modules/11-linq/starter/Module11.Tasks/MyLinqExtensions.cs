namespace Module11.Tasks;

/// <summary>
/// Задача 11.6 ⭐ — свои операторы LINQ через yield.
/// Требования ко всем: ленивые (deferred), однопроходные (source перечисляется один раз и лениво,
/// работают с бесконечными последовательностями), проверка аргументов — СРАЗУ при вызове.
/// Встроенные Chunk/DistinctBy не используйте — смысл задачи в том, чтобы написать их самому.
/// Тесты: MyLinqExtensionsTests.
/// </summary>
public static class MyLinqExtensions
{
    /// <summary>
    /// Разбивает последовательность на пакеты по <paramref name="size"/> элементов (последний может быть меньше).
    /// [1..7], size 3 → [1,2,3], [4,5,6], [7]. Каждый пакет — новый список.
    /// Пакет отдаётся, как только набран (не нужно читать source дальше).
    /// null → <see cref="ArgumentNullException"/>, size &lt;= 0 → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Оставляет первый элемент для каждого ключа, сохраняя порядок: аналог DistinctBy.
    /// <paramref name="comparer"/> == null → <see cref="EqualityComparer{T}.Default"/>.
    /// null source/keySelector → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static IEnumerable<T> DistinctByKey<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Пары соседних элементов: [a, b, c, d] → (a, b), (b, c), (c, d). Меньше двух элементов → пусто.
    /// null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        throw new NotImplementedException();
    }
}
