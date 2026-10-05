using Module03.Tasks;

namespace Module03.Tests;

public class TicketPricingTests
{
    [Theory]
    [InlineData(0, DayOfWeek.Saturday, false, 0)]
    [InlineData(2, DayOfWeek.Monday, true, 0)]
    [InlineData(3, DayOfWeek.Monday, false, 250)]
    [InlineData(13, DayOfWeek.Sunday, true, 250)]
    [InlineData(14, DayOfWeek.Monday, false, 500)]
    [InlineData(30, DayOfWeek.Saturday, false, 600)]
    [InlineData(30, DayOfWeek.Friday, false, 500)]
    [InlineData(20, DayOfWeek.Tuesday, true, 350)]
    [InlineData(20, DayOfWeek.Sunday, true, 600)]
    [InlineData(64, DayOfWeek.Wednesday, false, 500)]
    [InlineData(65, DayOfWeek.Saturday, false, 300)]
    [InlineData(120, DayOfWeek.Monday, true, 300)]
    public void GetPrice_VariousCustomers_ReturnsExpectedPrice(int age, DayOfWeek day, bool isStudent, int expected)
    {
        Assert.Equal(expected, TicketPricing.GetPrice(age, day, isStudent));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(121)]
    public void GetPrice_InvalidAge_Throws(int age)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TicketPricing.GetPrice(age, DayOfWeek.Monday, false));
    }

    [Theory]
    [InlineData(12, "Winter")]
    [InlineData(1, "Winter")]
    [InlineData(2, "Winter")]
    [InlineData(3, "Spring")]
    [InlineData(5, "Spring")]
    [InlineData(6, "Summer")]
    [InlineData(8, "Summer")]
    [InlineData(9, "Autumn")]
    [InlineData(11, "Autumn")]
    public void GetSeason_ValidMonth_ReturnsSeason(int month, string expected)
    {
        Assert.Equal(expected, TicketPricing.GetSeason(month));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-5)]
    public void GetSeason_InvalidMonth_Throws(int month)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TicketPricing.GetSeason(month));
    }
}
