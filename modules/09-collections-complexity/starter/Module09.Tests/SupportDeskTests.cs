using Module09.Tasks;

namespace Module09.Tests;

public class SupportDeskTests
{
    [Fact]
    public void NewDesk_IsEmpty()
    {
        var desk = new SupportDesk();

        Assert.Equal(0, desk.Count);
        Assert.Null(desk.TakeNext());
        Assert.Empty(desk.History);
    }

    [Fact]
    public void TakeNext_ReturnsMostUrgentFirst()
    {
        var desk = new SupportDesk();
        desk.Submit("T-1", 3);
        desk.Submit("T-2", 1);
        desk.Submit("T-3", 2);

        Assert.Equal("T-2", desk.TakeNext());
        Assert.Equal("T-3", desk.TakeNext());
        Assert.Equal("T-1", desk.TakeNext());
        Assert.Null(desk.TakeNext());
    }

    [Fact]
    public void TakeNext_SamePriority_IsFifo()
    {
        var desk = new SupportDesk();
        // Много обращений с одинаковым приоритетом: PriorityQueue сам по себе НЕ гарантирует FIFO.
        for (int i = 1; i <= 20; i++)
        {
            desk.Submit($"T-{i}", 2);
        }
        desk.Submit("URGENT", 1);

        Assert.Equal("URGENT", desk.TakeNext());
        for (int i = 1; i <= 20; i++)
        {
            Assert.Equal($"T-{i}", desk.TakeNext());
        }
    }

    [Fact]
    public void Submit_DuplicatePendingId_ThrowsInvalidOperationException()
    {
        var desk = new SupportDesk();
        desk.Submit("T-1", 1);

        Assert.Throws<InvalidOperationException>(() => desk.Submit("T-1", 5));
        Assert.Equal(1, desk.Count);
    }

    [Fact]
    public void Submit_IdIsCaseSensitive()
    {
        var desk = new SupportDesk();
        desk.Submit("t-1", 1);
        desk.Submit("T-1", 1);

        Assert.Equal(2, desk.Count);
    }

    [Fact]
    public void Submit_SameIdAfterTaken_IsAllowed()
    {
        var desk = new SupportDesk();
        desk.Submit("T-1", 1);
        desk.TakeNext();

        desk.Submit("T-1", 2);

        Assert.True(desk.Contains("T-1"));
        Assert.Equal("T-1", desk.TakeNext());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Submit_InvalidId_ThrowsArgumentException(string? ticketId)
    {
        var desk = new SupportDesk();

        Assert.ThrowsAny<ArgumentException>(() => desk.Submit(ticketId!, 1));
    }

    [Fact]
    public void Contains_ReflectsPendingTicketsOnly()
    {
        var desk = new SupportDesk();
        desk.Submit("T-1", 1);
        desk.Submit("T-2", 2);

        Assert.True(desk.Contains("T-1"));
        desk.TakeNext();

        Assert.False(desk.Contains("T-1"));
        Assert.True(desk.Contains("T-2"));
        Assert.False(desk.Contains("T-404"));
    }

    [Fact]
    public void History_RecordsTakenTicketsInOrder()
    {
        var desk = new SupportDesk();
        desk.Submit("A", 2);
        desk.Submit("B", 1);
        desk.TakeNext();
        desk.TakeNext();
        desk.TakeNext(); // пусто — в историю ничего не добавляется

        Assert.Equal(new[] { "B", "A" }, desk.History);
        Assert.Equal(0, desk.Count);
    }
}
