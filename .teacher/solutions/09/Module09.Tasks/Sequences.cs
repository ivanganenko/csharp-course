namespace Module09.Tasks;

public static class Sequences
{
    public static IEnumerable<int> Range(int from, int toExclusive, int step = 1)
    {
        // Почему валидация вынесена из итератора: метод с yield целиком превращается компилятором
        // в state machine, и его тело (включая проверки!) начинает выполняться только при первом MoveNext.
        // Ошибку программиста нужно показать сразу, в месте вызова — поэтому внешний метод без yield
        // проверяет аргументы, а ленивую часть делает локальная функция.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);
        return RangeIterator(from, toExclusive, step);

        static IEnumerable<int> RangeIterator(int start, int end, int increment)
        {
            // long, а не int: при current = int.MaxValue - 2 и step = 5 int «перескочил» бы
            // в отрицательные числа (unchecked overflow), условие current < end снова стало бы true —
            // бесконечный цикл.
            for (long current = start; current < end; current += increment)
            {
                yield return (int)current;
            }
        }
    }

    public static IEnumerable<int> Naturals()
    {
        // Бесконечная последовательность возможна только благодаря ленивости:
        // следующий элемент вычисляется, лишь когда его запросили.
        for (int i = 1; ; i++)
        {
            yield return i;
            if (i == int.MaxValue)
            {
                yield break; // не допускаем переполнения
            }
        }
    }

    public static IEnumerable<T> EveryNth<T>(IEnumerable<T> source, int n)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
        return EveryNthIterator(source, n);

        static IEnumerable<T> EveryNthIterator(IEnumerable<T> items, int step)
        {
            // Счётчик позиции вместо индексации: источник может быть не списком,
            // а ленивой/бесконечной последовательностью без Count и индексатора.
            int position = 0;
            foreach (T item in items)
            {
                position++;
                if (position == step)
                {
                    position = 0;
                    yield return item;
                }
            }
        }
    }
}
