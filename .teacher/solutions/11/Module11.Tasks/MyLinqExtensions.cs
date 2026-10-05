namespace Module11.Tasks;

public static class MyLinqExtensions
{
    public static IEnumerable<IReadOnlyList<T>> Batch<T>(this IEnumerable<T> source, int size)
    {
        // Схема всех операторов LINQ: жадная валидация во внешнем методе, ленивая работа в итераторе.
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);
        return BatchIterator(source, size);

        static IEnumerable<IReadOnlyList<T>> BatchIterator(IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>(batchSize);
            foreach (T item in items)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    // Отдаём пакет сразу, как набрали, — не читаем источник дальше, чем нужно.
                    yield return batch;
                    // НОВЫЙ список, а не batch.Clear(): вызывающий мог сохранить ссылку на прошлый пакет
                    // (например, через ToList()) — очистка испортила бы его данные.
                    batch = new List<T>(batchSize);
                }
            }

            if (batch.Count > 0)
            {
                yield return batch; // неполный последний пакет
            }
        }
    }

    public static IEnumerable<T> DistinctByKey<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);
        return DistinctByKeyIterator(source, keySelector, comparer);

        static IEnumerable<T> DistinctByKeyIterator(
            IEnumerable<T> items,
            Func<T, TKey> selectKey,
            IEqualityComparer<TKey>? keyComparer)
        {
            // HashSet создаётся внутри итератора — у каждого перечисления свой набор «уже виденных»
            // ключей, поэтому повторный перебор результата снова даёт полный ответ.
            var seenKeys = new HashSet<TKey>(keyComparer);
            foreach (T item in items)
            {
                // Streaming: элемент отдаётся сразу, как только его ключ встречен впервые.
                if (seenKeys.Add(selectKey(item)))
                {
                    yield return item;
                }
            }
        }
    }

    public static IEnumerable<(T Previous, T Current)> Pairwise<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return PairwiseIterator(source);

        static IEnumerable<(T Previous, T Current)> PairwiseIterator(IEnumerable<T> items)
        {
            // Ручной enumerator: нужен «предыдущий» элемент, а индексов у IEnumerable нет.
            // Один проход, работает с бесконечными последовательностями.
            using IEnumerator<T> enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                yield break;
            }

            T previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                T current = enumerator.Current;
                yield return (previous, current);
                previous = current;
            }
        }
    }
}
