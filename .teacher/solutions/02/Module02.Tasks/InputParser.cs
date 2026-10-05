using System.Globalization;

namespace Module02.Tasks;

public static class InputParser
{
    private const int MaxAge = 150;

    public static int? ParseInt(string? input)
    {
        // TryParse, а не Parse в try/catch: некорректный ввод — ОЖИДАЕМАЯ ситуация,
        // а исключения дорогие (сбор stack trace) и предназначены для исключительных случаев.
        // NumberStyles.Integer = пробелы по краям + знак; InvariantCulture — чтобы поведение
        // не зависело от настроек Windows. TryParse сам обрабатывает null и переполнение.
        return int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public static int? ParseAge(string? input)
    {
        // Переиспользуем ParseInt, а не дублируем парсинг. Pattern matching
        // `is >= 0 and <= MaxAge` проверяет и HasValue, и диапазон одним выражением.
        var value = ParseInt(input);
        return value is >= 0 and <= MaxAge ? value : null;
    }

    public static decimal? ParseAmount(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Пользователь на русской раскладке вводит запятую, на английской — точку.
        // Нормализуем к точке и парсим ВСЕГДА в InvariantCulture — так результат
        // одинаков на любой машине. Стили перечислены явно: NumberStyles.Number
        // разрешил бы разделители тысяч, и "1,000" неожиданно стало бы 1.
        const NumberStyles styles = NumberStyles.AllowLeadingWhite
                                    | NumberStyles.AllowTrailingWhite
                                    | NumberStyles.AllowLeadingSign
                                    | NumberStyles.AllowDecimalPoint;

        var normalized = input.Replace(',', '.');
        return decimal.TryParse(normalized, styles, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    public static bool? ParseYesNo(string? input)
    {
        if (input is null)
        {
            return null;
        }

        // ToLowerInvariant, а не ToLower(): ToLower зависит от текущей культуры
        // (знаменитая «турецкая i»: в tr-TR "I".ToLower() — это "ı", а не "i").
        // switch expression с несколькими константами через `or` читается как таблица.
        return input.Trim().ToLowerInvariant() switch
        {
            "yes" or "y" or "true" or "1" or "да" => true,
            "no" or "n" or "false" or "0" or "нет" => false,
            _ => null,
        };
    }
}
