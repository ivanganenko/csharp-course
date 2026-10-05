namespace Module14.Tasks;

/// <summary>
/// Задача 14.6 — «нетестируемый» код. Метод работает, но написать для него надёжный тест нельзя:
/// результат зависит от реального времени (DateTime.UtcNow).
/// Отрефактори: внедри IClock через конструктор, затем напиши тесты в HappyHourDiscountTests.
/// Поведение метода Apply менять нельзя.
/// </summary>
public sealed class HappyHourDiscount
{
    public const decimal DiscountRate = 0.20m;

    /// <summary>
    /// Скидка 20% по пятницам с 17:00 до 19:59:59 (UTC). Округление до 2 знаков (AwayFromZero).
    /// price &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public decimal Apply(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        var now = DateTime.UtcNow; // ← из-за этой строки код нельзя протестировать детерминированно
        var isHappyHour = now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 17 && now.Hour < 20;

        return isHappyHour
            ? Math.Round(price * (1 - DiscountRate), 2, MidpointRounding.AwayFromZero)
            : price;
    }
}
