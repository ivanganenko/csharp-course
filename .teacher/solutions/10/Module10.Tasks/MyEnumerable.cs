namespace Module10.Tasks;

public static class MyEnumerable
{
    public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        // Жадная проверка + ленивый итератор (тот же приём, что в модуле 09 и в самом LINQ):
        // иначе ArgumentNullException вылетел бы только при переборе, далеко от места ошибки.
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        return FilterIterator(source, predicate);

        static IEnumerable<T> FilterIterator(IEnumerable<T> items, Func<T, bool> condition)
        {
            foreach (T item in items)
            {
                // Делегат вызывается как обычный метод. Логика «что фильтровать» пришла снаружи —
                // наш метод отвечает только за «как перебирать». Это и есть сила делегатов.
                if (condition(item))
                {
                    yield return item;
                }
            }
        }
    }

    public static IEnumerable<TResult> Map<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        return MapIterator(source, selector);

        static IEnumerable<TResult> MapIterator(IEnumerable<T> items, Func<T, TResult> transform)
        {
            foreach (T item in items)
            {
                yield return transform(item);
            }
        }
    }

    public static TAccumulate Reduce<T, TAccumulate>(
        this IEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> accumulator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(accumulator);

        // Reduce возвращает одно значение, а не последовательность, — лениться здесь нечему,
        // поэтому обычный метод без yield (как Aggregate/Sum/Count в LINQ — immediate execution).
        TAccumulate result = seed;
        foreach (T item in source)
        {
            result = accumulator(result, item);
        }

        return result;
    }

    public static int CountWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        // Predicate<T> и Func<T, bool> — разные типы делегатов с одинаковой сигнатурой;
        // неявно друг в друга не преобразуются. Predicate появился в .NET 2.0 (List.FindAll),
        // Func — в .NET 3.5 вместе с LINQ.
        int count = 0;
        foreach (T item in source)
        {
            if (predicate(item))
            {
                count++;
            }
        }

        return count;
    }
}
