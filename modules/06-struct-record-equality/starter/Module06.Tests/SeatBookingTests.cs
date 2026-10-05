using Module06.Tasks;

namespace Module06.Tests;

public class SeatBookingTests
{
    [Fact]
    public void SeatKey_Constructor_TrimsHallAndKeepsCase()
    {
        var seat = new SeatKey("  Red ", 5, 7);

        Assert.Equal("Red", seat.Hall);
        Assert.Equal(5, seat.Row);
        Assert.Equal(7, seat.Seat);
        Assert.Equal("Red R5 S7", seat.ToString());
    }

    [Fact]
    public void SeatKey_InvalidArguments_Throw()
    {
        Assert.ThrowsAny<ArgumentException>(() => new SeatKey(" ", 1, 1));
        Assert.ThrowsAny<ArgumentException>(() => new SeatKey(null!, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeatKey("Red", 0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeatKey("Red", 1, -1));
    }

    [Fact]
    public void SeatKey_Equality_IgnoresHallCase()
    {
        var a = new SeatKey("Red", 5, 7);
        var b = new SeatKey("RED", 5, 7);

        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void SeatKey_DifferentRowSeatOrHall_NotEqual()
    {
        var a = new SeatKey("Red", 5, 7);

        Assert.NotEqual(a, new SeatKey("Red", 5, 8));
        Assert.NotEqual(a, new SeatKey("Red", 6, 7));
        Assert.NotEqual(a, new SeatKey("Blue", 5, 7));
        Assert.False(a.Equals("Red R5 S7"));
    }

    [Fact]
    public void SeatKey_Default_DoesNotThrowOnHashOrEquals()
    {
        var empty = default(SeatKey);

        _ = empty.GetHashCode();
        Assert.True(empty.Equals(default(SeatKey)));
        Assert.False(empty.Equals(new SeatKey("Red", 1, 1)));
    }

    [Fact]
    public void SeatKey_HashSet_DeduplicatesCaseInsensitiveHalls()
    {
        var set = new HashSet<SeatKey> { new("Red", 1, 1), new("red", 1, 1), new("RED", 1, 1), new("Red", 1, 2) };

        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void TryBook_SameSeatTwice_SecondFails()
    {
        var service = new BookingService();

        Assert.True(service.TryBook(new SeatKey("Red", 5, 7), " Anna "));
        Assert.False(service.TryBook(new SeatKey("RED", 5, 7), "Boris"));

        Assert.Equal(1, service.Count);
        Assert.Equal("Anna", service.GetCustomer(new SeatKey("red", 5, 7)));
    }

    [Fact]
    public void IsBookedAndCancel_UseEqualKeys()
    {
        var service = new BookingService();
        service.TryBook(new SeatKey("Blue", 1, 1), "Anna");

        Assert.True(service.IsBooked(new SeatKey("BLUE", 1, 1)));
        Assert.False(service.IsBooked(new SeatKey("Blue", 1, 2)));
        Assert.True(service.Cancel(new SeatKey("blue", 1, 1)));
        Assert.False(service.Cancel(new SeatKey("Blue", 1, 1)));
        Assert.Null(service.GetCustomer(new SeatKey("Blue", 1, 1)));
        Assert.Equal(0, service.Count);
    }

    [Fact]
    public void TryBook_BlankCustomer_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new BookingService().TryBook(new SeatKey("Red", 1, 1), " "));
    }

    [Fact]
    public void GetSeatsOf_ReturnsSortedSeatsOfCustomer()
    {
        var service = new BookingService();
        service.TryBook(new SeatKey("Red", 2, 1), "Anna");
        service.TryBook(new SeatKey("blue", 3, 3), "Anna");
        service.TryBook(new SeatKey("Red", 1, 9), "anna");
        service.TryBook(new SeatKey("Red", 1, 2), "Anna");
        service.TryBook(new SeatKey("Red", 1, 1), "Boris");

        var seats = service.GetSeatsOf("ANNA");

        Assert.Equal("blue R3 S3|Red R1 S2|Red R1 S9|Red R2 S1", string.Join("|", seats));
        Assert.Empty(service.GetSeatsOf("Clara"));
    }
}
