namespace Module03.Tasks;

/// <summary>
/// Задача 03.2 — pattern matching: type, constant, relational, property, positional и list patterns.
/// Тесты: dotnet test --filter "FullyQualifiedName~PatternMatcherTests"
/// </summary>
public static class PatternMatcher
{
    /// <summary>
    /// Описывает значение строкой. Правила проверяются сверху вниз:
    /// <list type="bullet">
    /// <item>null → "null"</item>
    /// <item>int 0 → "zero"; отрицательный int → "negative int"; положительный int → "positive int"</item>
    /// <item>double (любой) → "double"</item>
    /// <item>string длины 0 → "empty string"; иначе → "string of length N" (N — длина)</item>
    /// <item>int[] без элементов → "empty array"; из одного элемента → "single element X";
    /// из двух и более → "array from FIRST to LAST" (первый и последний элементы)</item>
    /// <item>Point с X = 0 и Y = 0 → "origin"; Point, у которой X = 0 или Y = 0 → "on axis"; другая Point → "point"</item>
    /// <item>DateOnly 31 декабря (любого года) → "new year's eve"; другой DateOnly → "date"</item>
    /// <item>всё остальное (long, bool, List и т.д.) → "unknown"</item>
    /// </list>
    /// Обратите внимание: 5L (long) — это НЕ int, поэтому → "unknown".
    /// </summary>
    public static string Describe(object? value)
    {
        throw new NotImplementedException();
    }
}
