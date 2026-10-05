namespace Module12.Tasks;

/// <summary>
/// Задача 12.3 — разбор строк без лишних аллокаций.
/// Правило задачи: НЕ используй Substring, string.Split, ToString(), new string(...) внутри методов —
/// только операции над <see cref="ReadOnlySpan{T}"/> (Slice / диапазоны [a..b], IndexOf, Trim, int.Parse(span, ...)).
/// Числа разбирай с <c>CultureInfo.InvariantCulture</c>.
/// </summary>
public static class SpanParser
{
    /// <summary>
    /// Сумма целых чисел, разделённых запятыми: "10, 20,30" → 60.
    /// Пробелы вокруг чисел допустимы. Пустая строка или только пробелы → 0.
    /// Пустой или нечисловой элемент ("1,,2", "1,x") → <see cref="FormatException"/>.
    /// </summary>
    public static int SumCsv(ReadOnlySpan<char> line)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Количество слов: слово — непрерывная последовательность символов, для которых
    /// char.IsWhiteSpace == false. "  hello   big world " → 3, "" → 0.
    /// </summary>
    public static int CountWords(ReadOnlySpan<char> text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Разбирает строку вида "key=value" по ПЕРВОМУ символу '='; key и value обрезаются (Trim).
    /// "  host = example.local " → true, key "host", value "example.local".
    /// "a=b=c" → key "a", value "b=c". "key=" → true, value пустой.
    /// Нет '=' или key пустой после Trim → false (key и value — пустые span).
    /// </summary>
    public static bool TryParseKeyValue(ReadOnlySpan<char> input, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Уровень лога из строки вида "[ERROR] Disk is full" → "ERROR".
    /// Строка должна начинаться с '[' и содержать ']'; иначе — пустой span.
    /// "[] text" → пустой span.
    /// </summary>
    public static ReadOnlySpan<char> GetLogLevel(ReadOnlySpan<char> logLine)
    {
        throw new NotImplementedException();
    }
}
