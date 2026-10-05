using System.Globalization;
using Module04.Tasks;

namespace Module04.Tests;

// Важно: счётчик static — общий для всех тестов. Поэтому тесты проверяют только ПРИРОСТ
// (было → стало), а не абсолютные значения. Тесты внутри одного класса xUnit выполняет последовательно,
// а SupportTicket создаётся только здесь, так что параллельные тесты других классов не мешают.
public class SupportTicketTests
{
    [Fact]
    public void Constructor_ValidTitle_StoresTrimmedTitle()
    {
        var ticket = new SupportTicket("  Printer is on fire  ");

        Assert.Equal("Printer is on fire", ticket.Title);
    }

    [Fact]
    public void Constructor_TwoTickets_GetSequentialIds()
    {
        var first = new SupportTicket("First");
        var second = new SupportTicket("Second");

        Assert.Equal(first.Id + 1, second.Id);
        Assert.True(first.Id >= 1);
    }

    [Fact]
    public void TotalCreated_IncreasesWithEachTicket()
    {
        int before = SupportTicket.TotalCreated;

        _ = new SupportTicket("One");
        _ = new SupportTicket("Two");
        _ = new SupportTicket("Three");

        Assert.Equal(before + 3, SupportTicket.TotalCreated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankTitle_ThrowsAndDoesNotIncrementCounter(string? title)
    {
        int before = SupportTicket.TotalCreated;

        Assert.ThrowsAny<ArgumentException>(() => new SupportTicket(title!));
        Assert.Equal(before, SupportTicket.TotalCreated);
    }

    [Fact]
    public void Constructor_TooLongTitle_ThrowsAndDoesNotIncrementCounter()
    {
        int before = SupportTicket.TotalCreated;

        Assert.ThrowsAny<ArgumentException>(
            () => new SupportTicket(new string('x', SupportTicket.MaxTitleLength + 1)));
        Assert.Equal(before, SupportTicket.TotalCreated);
    }

    [Fact]
    public void Constructor_TitleOfMaxLength_IsAllowed()
    {
        var ticket = new SupportTicket(new string('x', SupportTicket.MaxTitleLength));

        Assert.Equal(SupportTicket.MaxTitleLength, ticket.Title.Length);
    }

    [Fact]
    public void Number_IsPrefixPlusIdPaddedToFourDigits()
    {
        var ticket = new SupportTicket("Need a new keyboard");

        Assert.StartsWith("TCK-", ticket.Number);
        Assert.Equal(
            SupportTicket.Prefix + ticket.Id.ToString("D4", CultureInfo.InvariantCulture),
            ticket.Number);
    }

    [Fact]
    public void Ids_ForManyTickets_AreUnique()
    {
        var ids = Enumerable.Range(0, 100)
            .Select(i => new SupportTicket($"Ticket {i}").Id)
            .ToList();

        Assert.Equal(100, ids.Distinct().Count());
    }
}
