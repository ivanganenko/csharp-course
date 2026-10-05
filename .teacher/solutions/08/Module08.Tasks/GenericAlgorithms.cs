namespace Module08.Tasks;

public static class GenericAlgorithms
{
    public static T Max<T>(IEnumerable<T> items) where T : IComparable<T>
    {
        ArgumentNullException.ThrowIfNull(items);

        // Работаем через enumerator напрямую: так мы отличаем «пусто» от «есть первый элемент»
        // за один проход и не требуем от последовательности Count (она может быть ленивой).
        using IEnumerator<T> enumerator = items.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        T max = enumerator.Current;
        while (enumerator.MoveNext())
        {
            // Constraint IComparable<T> — единственная причина, по которой здесь можно вызвать CompareTo.
            // Без него компилятор знает о T только то, что это object.
            if (enumerator.Current.CompareTo(max) > 0)
            {
                max = enumerator.Current;
            }
        }

        return max;
    }

    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (min.CompareTo(max) > 0)
        {
            // Именно ArgumentException: проблема в сочетании аргументов, а не в одном из них.
            throw new ArgumentException($"{nameof(min)} must not be greater than {nameof(max)}.");
        }

        if (value.CompareTo(min) < 0)
        {
            return min;
        }

        return value.CompareTo(max) > 0 ? max : value;
    }

    public static void Swap<T>(ref T first, ref T second)
    {
        // Tuple swap — идиоматичный C# 7+; ref нужен, иначе поменялись бы только локальные копии.
        (first, second) = (second, first);
    }

    public static int CountDefaults<T>(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        // Почему не `item == default`: для неограниченного T оператор == не определён
        // (T может быть struct без ==). EqualityComparer<T>.Default умеет всё:
        // для value types не делает boxing (использует IEquatable<T>), для null — корректно сравнивает.
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        int count = 0;
        foreach (T item in items)
        {
            if (comparer.Equals(item, default!))
            {
                count++;
            }
        }

        return count;
    }

    public static List<T> CreateMany<T>(int count) where T : new()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        // Задаём capacity заранее — размер известен, лишних перевыделений не будет.
        var result = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            // new T() разрешён только благодаря constraint new().
            result.Add(new T());
        }

        return result;
    }
}
