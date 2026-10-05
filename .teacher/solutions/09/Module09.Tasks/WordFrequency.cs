namespace Module09.Tasks;

public static class WordFrequency
{
    public static Dictionary<string, int> Count(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // Ключи уже приведены к нижнему регистру, поэтому достаточно быстрого Ordinal-сравнения.
        // Альтернатива — не приводить регистр, а создать словарь с StringComparer.OrdinalIgnoreCase,
        // но тогда ключом станет ПЕРВОЕ встреченное написание ("The"), а по условию нужен нижний регистр.
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        // Один проход по символам: O(n). text.Split(...) тоже подошёл бы, но пришлось бы перечислить
        // все возможные разделители, и он создаёт массив всех кусков (лишние аллокации).
        int wordStart = -1;
        for (int i = 0; i <= text.Length; i++)
        {
            // i == text.Length — «виртуальный разделитель» в конце, чтобы не дублировать код
            // для последнего слова после цикла.
            bool isWordChar = i < text.Length && char.IsLetterOrDigit(text[i]);
            if (isWordChar)
            {
                if (wordStart < 0)
                {
                    wordStart = i;
                }
            }
            else if (wordStart >= 0)
            {
                // ToLowerInvariant, а не ToLower: результат не должен зависеть от культуры машины
                // (классика — турецкая «I» без точки).
                string word = text.Substring(wordStart, i - wordStart).ToLowerInvariant();
                counts.TryGetValue(word, out int current); // отсутствие ключа → current = 0
                counts[word] = current + 1;
                wordStart = -1;
            }
        }

        return counts;
    }

    public static IReadOnlyList<(string Word, int Count)> Top(string text, int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        Dictionary<string, int> counts = Count(text);

        var entries = new List<(string Word, int Count)>(counts.Count);
        foreach (var (word, count) in counts)
        {
            entries.Add((word, count));
        }

        // Полная сортировка — O(m log m), где m — число различных слов. Для top-k на огромных данных
        // можно держать PriorityQueue размера k: O(m log k). Здесь простота важнее.
        entries.Sort((a, b) =>
        {
            int byCount = b.Count.CompareTo(a.Count); // по убыванию
            return byCount != 0 ? byCount : string.CompareOrdinal(a.Word, b.Word);
        });

        return entries.GetRange(0, Math.Min(n, entries.Count));
    }
}
