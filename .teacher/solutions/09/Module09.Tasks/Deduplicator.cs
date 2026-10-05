namespace Module09.Tasks;

public static class Deduplicator
{
    public static List<T> RemoveDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        // HashSet — проверка «уже видели?» за O(1) в среднем. List.Contains дал бы O(n) на каждый
        // элемент и O(n²) в сумме. null-comparer → HashSet сам возьмёт EqualityComparer<T>.Default.
        var seen = new HashSet<T>(comparer);
        var result = new List<T>();

        foreach (T item in items)
        {
            // HashSet.Add возвращает false, если элемент уже был, — проверка и вставка за одну операцию.
            if (seen.Add(item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public static List<T> FindDuplicates<T>(IEnumerable<T> items, IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        var seen = new HashSet<T>(comparer);
        // Второй набор — чтобы элемент, встретившийся 3+ раза, попал в результат только один раз.
        var reported = new HashSet<T>(comparer);
        var result = new List<T>();

        foreach (T item in items)
        {
            if (!seen.Add(item) && reported.Add(item))
            {
                result.Add(item);
            }
        }

        return result;
    }
}
