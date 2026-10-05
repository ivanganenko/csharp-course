namespace Module03.Tasks;

/// <summary>
/// Задача 03.7 ⭐ — бинарный поиск: классика live coding. Не используйте Array.BinarySearch и LINQ.
/// Тесты: dotnet test --filter "FullyQualifiedName~BinarySearchTests"
/// </summary>
public static class BinarySearch
{
    /// <summary>
    /// Индекс target в массиве, отсортированном по возрастанию, или -1, если его нет. O(log n), итеративно.
    /// При дубликатах — индекс любого из вхождений. sorted == null → ArgumentNullException.
    /// </summary>
    public static int IndexOf(int[] sorted, int target)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Lower bound: индекс ПЕРВОГО элемента, который ≥ target. Если все элементы меньше — sorted.Length.
    /// [1, 2, 2, 2, 5] и target 2 → 1; target 3 → 4; target 0 → 0; target 9 → 5. O(log n).
    /// sorted == null → ArgumentNullException.
    /// </summary>
    public static int LowerBound(int[] sorted, int target)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сколько раз target встречается в отсортированном массиве — за O(log n), а не линейным проходом.
    /// Должно работать и для target = int.MaxValue (осторожно с target + 1!).
    /// sorted == null → ArgumentNullException.
    /// </summary>
    public static int CountOccurrences(int[] sorted, int target)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Целая часть квадратного корня: наибольшее x, такое что x * x ≤ n. Бинарный поиск по ответу,
    /// без Math.Sqrt. IntegerSqrt(17) → 4, IntegerSqrt(int.MaxValue) → 46340 (без переполнения!).
    /// n &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static int IntegerSqrt(int n)
    {
        throw new NotImplementedException();
    }
}
