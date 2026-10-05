namespace Module06.Tasks;

/// <summary>
/// Задача 06.3 — tuples (кортежи): вернуть несколько значений без отдельного класса.
/// (int Min, int Max) — это System.ValueTuple&lt;int, int&gt; с именами элементов: value type с равенством по значению.
/// </summary>
public static class Statistics
{
    /// <summary>
    /// Минимум и максимум за ОДИН проход по коллекции.
    /// values == null → ArgumentNullException; пустая коллекция → InvalidOperationException.
    /// </summary>
    public static (int Min, int Max) MinMax(IReadOnlyList<int> values)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Количество и среднее арифметическое. Пустая коллекция → (0, 0.0).
    /// values == null → ArgumentNullException.
    /// </summary>
    public static (int Count, double Average) CountAndAverage(IReadOnlyList<int> values)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Меняет значения местами и возвращает их: Swap((1, "a")) → ("a", 1).
    /// Подсказка: деконструкция var (a, b) = pair;
    /// </summary>
    public static (string, int) Swap((int Number, string Text) pair)
    {
        throw new NotImplementedException();
    }
}
