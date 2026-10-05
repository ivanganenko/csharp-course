namespace Module02.Tasks;

/// <summary>
/// Задача 02.1 — безопасный парсинг пользовательского ввода.
/// Ни один метод не должен бросать исключение на «плохом» вводе: вместо этого — null
/// (nullable value types: int?, decimal?, bool?).
/// Тесты: dotnet test --filter "FullyQualifiedName~InputParserTests"
/// </summary>
public static class InputParser
{
    /// <summary>
    /// Целое число в формате InvariantCulture. Допускаются пробелы по краям и знак: " -42 " → -42.
    /// null, пустая строка, не число, дробное число ("1.5") или переполнение int ("2147483648") → null.
    /// </summary>
    public static int? ParseInt(string? input)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возраст: целое число от 0 до 150 включительно (правила как в ParseInt). Иначе → null.
    /// </summary>
    public static int? ParseAge(string? input)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Денежная сумма как decimal. Пользователь может ввести разделитель и точкой, и запятой:
    /// "12.5" → 12.5, "12,5" → 12.5, " -3 " → -3.
    /// Разделители тысяч НЕ поддерживаются. Допустимы: пробелы по краям, знак, одна десятичная точка/запятая.
    /// null, пусто, не число ("abc", "1.2.3") → null.
    /// </summary>
    public static decimal? ParseAmount(string? input)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ответ «да/нет» без учёта регистра и пробелов по краям:
    /// "yes", "y", "true", "1", "да" → true; "no", "n", "false", "0", "нет" → false.
    /// Всё остальное (включая null) → null.
    /// </summary>
    public static bool? ParseYesNo(string? input)
    {
        throw new NotImplementedException();
    }
}
