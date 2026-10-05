namespace Module14.Tasks;

public sealed class HappyHourDiscount
{
    public const decimal DiscountRate = 0.20m;

    private readonly IClock _clock;

    // Рефакторинг «внедрение зависимости»: источник времени приходит снаружи.
    // В production DI-контейнер передаст SystemClock, в тесте — FakeClock с нужной датой.
    // Поведение Apply не изменилось — изменилось только то, ОТКУДА берётся «сейчас».
    public HappyHourDiscount(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
    }

    public decimal Apply(decimal price)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        // UtcDateTime — на случай, если clock вернёт время с ненулевым смещением:
        // правило сформулировано в UTC.
        var now = _clock.UtcNow.UtcDateTime;
        var isHappyHour = now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 17 && now.Hour < 20;

        return isHappyHour
            ? Math.Round(price * (1 - DiscountRate), 2, MidpointRounding.AwayFromZero)
            : price;
    }
}
