namespace Module03.Tasks;

public static class TicketPricing
{
    private const int MaxAge = 120;

    public static int GetPrice(int age, DayOfWeek day, bool isStudent)
    {
        // Валидация отдельно от бизнес-правил: switch описывает ТАРИФЫ, а не проверку ввода.
        if (age is < 0 or > MaxAge)
        {
            throw new ArgumentOutOfRangeException(nameof(age), age, $"Возраст должен быть от 0 до {MaxAge}.");
        }

        // switch expression по кортежу: все правила видны как таблица решений,
        // порядок строк = приоритет правил (первое совпадение выигрывает).
        // Цепочка if/else делала бы то же, но правила тонули бы в фигурных скобках.
        // Компилятор проверяет полноту (exhaustiveness): без `_` он предупредил бы о необработанных случаях.
        return (age, day, isStudent) switch
        {
            (< 3, _, _) => 0,
            (< 14, _, _) => 250,
            (>= 65, _, _) => 300,
            (_, not (DayOfWeek.Saturday or DayOfWeek.Sunday), true) => 350,
            (_, DayOfWeek.Saturday or DayOfWeek.Sunday, _) => 600,
            _ => 500,
        };
    }

    // `or`-паттерны позволяют перечислить месяцы без 12 отдельных case;
    // `_ => throw` — выражение throw внутри switch expression (C# 7+): невалидный месяц — ошибка вызывающего кода.
    public static string GetSeason(int month) => month switch
    {
        12 or 1 or 2 => "Winter",
        >= 3 and <= 5 => "Spring",
        >= 6 and <= 8 => "Summer",
        >= 9 and <= 11 => "Autumn",
        _ => throw new ArgumentOutOfRangeException(nameof(month), month, "Месяц должен быть от 1 до 12."),
    };
}
