namespace Module03.Tasks;

/// <summary>
/// Задача 03.3 — собственный TryParse с out-параметром.
/// Тесты: dotnet test --filter "FullyQualifiedName~DurationParserTests"
/// </summary>
public static class DurationParser
{
    /// <summary>
    /// Разбирает длительность вида "1h30m", "2h", "45m" в общее число минут.
    /// <list type="bullet">
    /// <item>Регистр не важен ("1H30M"), пробелы по краям допустимы, пробелы ВНУТРИ — нет ("1h 30m" → false).</item>
    /// <item>Часы и минуты — целые неотрицательные числа из цифр 0–9 (без знака).</item>
    /// <item>Если указаны и часы, и минуты, минуты должны быть меньше 60 ("1h70m" → false).
    /// Если указаны только минуты — любое количество ("90m" → 90).</item>
    /// <item>Порядок — сначала часы, потом минуты ("30m1h" → false).</item>
    /// <item>Результат должен помещаться в int, иначе → false.</item>
    /// <item>null, пусто, "h", "m", "abc", "-1h", "1h30" → false.</item>
    /// </list>
    /// При false значение totalMinutes = 0 (как у int.TryParse).
    /// </summary>
    public static bool TryParse(string? input, out int totalMinutes)
    {
        throw new NotImplementedException();
    }
}
