namespace Module09.Tasks;

/// <summary>
/// Задача 09.1 — подсчёт частоты слов (например, для облака тегов или анализа отзывов).
/// Тесты: WordFrequencyTests.
/// </summary>
public static class WordFrequency
{
    /// <summary>
    /// Считает, сколько раз встречается каждое слово.
    /// Слово — непрерывная последовательность букв или цифр (<see cref="char.IsLetterOrDigit(char)"/>);
    /// всё остальное (пробелы, знаки препинания, переносы строк) — разделители.
    /// Регистр не важен: ключи словаря — в нижнем регистре (<see cref="string.ToLowerInvariant"/>).
    /// Пустая строка → пустой словарь. null → <see cref="ArgumentNullException"/>.
    /// Сложность — O(n) по длине текста.
    /// </summary>
    public static Dictionary<string, int> Count(string text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает <paramref name="n"/> самых частых слов: по убыванию количества,
    /// при равенстве — по алфавиту (ordinal-сравнение, <see cref="string.CompareOrdinal(string, string)"/>).
    /// Если различных слов меньше n — возвращает все.
    /// n &lt; 0 → <see cref="ArgumentOutOfRangeException"/>; null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public static IReadOnlyList<(string Word, int Count)> Top(string text, int n)
    {
        throw new NotImplementedException();
    }
}
