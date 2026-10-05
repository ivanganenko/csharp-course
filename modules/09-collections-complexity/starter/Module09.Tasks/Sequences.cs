namespace Module09.Tasks;

/// <summary>
/// Задача 09.4 — свои итераторы через <c>yield return</c>.
/// LINQ внутри не используйте. Тесты: SequencesTests.
/// </summary>
public static class Sequences
{
    /// <summary>
    /// Лениво возвращает числа from, from + step, from + 2*step, ... строго меньше <paramref name="toExclusive"/>.
    /// Если from &gt;= toExclusive — пустая последовательность.
    /// step &lt;= 0 → <see cref="ArgumentOutOfRangeException"/>, причём СРАЗУ при вызове метода,
    /// а не при первом MoveNext (подсказка: что происходит с проверками внутри метода с yield?).
    /// Осторожно с переполнением int рядом с <see cref="int.MaxValue"/>.
    /// </summary>
    public static IEnumerable<int> Range(int from, int toExclusive, int step = 1)
    {
        throw new NotImplementedException();
    }

    /// <summary>Бесконечная ленивая последовательность 1, 2, 3, ... (до int.MaxValue).</summary>
    public static IEnumerable<int> Naturals()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Лениво возвращает каждый n-й элемент: n-й, 2n-й, 3n-й, ... (нумерация с 1).
    /// Пример: [a, b, c, d, e, f, g], n = 3 → [c, f].
    /// Должен работать с бесконечными последовательностями (не материализуйте source в список).
    /// null → <see cref="ArgumentNullException"/>, n &lt;= 0 → <see cref="ArgumentOutOfRangeException"/> — СРАЗУ при вызове.
    /// </summary>
    public static IEnumerable<T> EveryNth<T>(IEnumerable<T> source, int n)
    {
        throw new NotImplementedException();
    }
}
