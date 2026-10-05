using Module14.Tasks;

namespace Module14.Tests;

public class HappyHourDiscountTests
{
    // 13 марта 2026 — пятница, 12 марта — четверг. Даты фиксированы — тест одинаков в любой день недели.
    private static DateTimeOffset Friday(int hour, int minute = 0, int second = 0) =>
        new(2026, 3, 13, hour, minute, second, TimeSpan.Zero);

    private static HappyHourDiscount CreateAt(DateTimeOffset now) => new(new FakeClock(now));

    public static TheoryData<DateTimeOffset> HappyHourMoments => new()
    {
        Friday(17),
        Friday(18, 30),
        Friday(19, 59, 59),
    };

    public static TheoryData<DateTimeOffset> RegularMoments => new()
    {
        Friday(16, 59, 59),
        Friday(20),
        Friday(9),
        new DateTimeOffset(2026, 3, 12, 18, 0, 0, TimeSpan.Zero), // четверг, «счастливый» час
    };

    [Theory]
    [MemberData(nameof(HappyHourMoments))]
    public void Apply_DuringHappyHour_GivesTwentyPercentDiscount(DateTimeOffset now)
    {
        Assert.Equal(80m, CreateAt(now).Apply(100m));
    }

    [Theory]
    [MemberData(nameof(RegularMoments))]
    public void Apply_OutsideHappyHour_ReturnsOriginalPrice(DateTimeOffset now)
    {
        Assert.Equal(100m, CreateAt(now).Apply(100m));
    }

    [Fact]
    public void Apply_DuringHappyHour_RoundsToCents()
    {
        Assert.Equal(7.99m, CreateAt(Friday(18)).Apply(9.99m)); // 7.992 → 7.99
    }

    [Fact]
    public void Apply_NegativePrice_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAt(Friday(18)).Apply(-1m));
    }

    [Fact]
    public void Constructor_NullClock_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HappyHourDiscount(null!));
    }
}
