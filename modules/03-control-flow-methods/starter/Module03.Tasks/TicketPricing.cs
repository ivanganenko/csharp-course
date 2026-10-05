namespace Module03.Tasks;

/// <summary>
/// Задача 03.1 — тарифы кинотеатра: switch expression, relational и logical patterns.
/// Тесты: dotnet test --filter "FullyQualifiedName~TicketPricingTests"
/// </summary>
public static class TicketPricing
{
    /// <summary>
    /// Цена билета в рублях. Правила проверяются СВЕРХУ ВНИЗ, срабатывает первое подходящее:
    /// <list type="number">
    /// <item>age &lt; 0 или age &gt; 120 → ArgumentOutOfRangeException;</item>
    /// <item>age &lt; 3 → 0 (бесплатно);</item>
    /// <item>age &lt; 14 → 250 (детский);</item>
    /// <item>age ≥ 65 → 300 (пенсионный);</item>
    /// <item>студент в будний день (понедельник–пятница) → 350;</item>
    /// <item>суббота или воскресенье → 600;</item>
    /// <item>иначе → 500.</item>
    /// </list>
    /// Попробуйте написать это одним switch expression по кортежу (age, day, isStudent).
    /// </summary>
    public static int GetPrice(int age, DayOfWeek day, bool isStudent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Время года по номеру месяца: 12, 1, 2 → "Winter"; 3–5 → "Spring"; 6–8 → "Summer"; 9–11 → "Autumn".
    /// Месяц вне 1–12 → ArgumentOutOfRangeException.
    /// </summary>
    public static string GetSeason(int month)
    {
        throw new NotImplementedException();
    }
}
