namespace Module04.Tasks;

/// <summary>
/// Задача 04.5 — extension methods (методы расширения) для string.
/// Extension method — статический метод статического класса, у первого параметра которого есть модификатор this.
/// Вызывается как метод экземпляра: "text".Truncate(3).
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// true, если строка null, пустая или состоит только из пробельных символов.
    /// Обратите внимание: метод можно вызвать на null-ссылке — ((string?)null).IsNullOrBlank() вернёт true,
    /// потому что это обычный статический вызов, а не вызов метода объекта.
    /// </summary>
    public static bool IsNullOrBlank(this string? value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Обрезает строку до maxLength символов и добавляет "..." , если строка была длиннее.
    /// "Hello world".Truncate(5) → "Hello..."; "Hi".Truncate(5) → "Hi"; "Hello".Truncate(5) → "Hello".
    /// value == null → ArgumentNullException; maxLength &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Количество слов: слова разделяются любыми пробельными символами (пробел, \t, \n), подряд идущие
    /// разделители считаются одним. null/пусто/пробелы → 0. "  two   words " → 2.
    /// </summary>
    public static int WordCount(this string? value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Маскирует строку, оставляя видимыми последние visibleSuffix символов:
    /// "1234567812345678".Mask() → "************5678"; "secret".Mask(2) → "****et"; "12345".Mask(2, '#') → "###45".
    /// Если длина строки &lt;= visibleSuffix — строка возвращается без изменений.
    /// value == null → ArgumentNullException; visibleSuffix &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static string Mask(this string value, int visibleSuffix = 4, char maskChar = '*')
    {
        throw new NotImplementedException();
    }
}
