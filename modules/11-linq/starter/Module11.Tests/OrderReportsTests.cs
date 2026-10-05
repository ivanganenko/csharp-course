using Module11.Tasks;

namespace Module11.Tests;

public class OrderReportsTests
{
    [Fact]
    public void TotalRevenue_SumsOnlyCompletedOrders()
    {
        // Completed: 1050 + 60 + 45 + 60
        Assert.Equal(1215m, OrderReports.TotalRevenue(TestData.Orders()));
    }

    [Fact]
    public void TotalRevenue_NoOrders_ReturnsZero()
    {
        Assert.Equal(0m, OrderReports.TotalRevenue(new List<Order>()));
    }

    [Fact]
    public void LargeOrderIds_SortedByTotalDescThenId_ExcludesCancelled()
    {
        // >= 60, не Cancelled: #1 (1050), #5 (1000), #2 (60), #6 (60). #3 отменён, #4 = 45.
        var ids = OrderReports.LargeOrderIds(TestData.Orders(), 60m);

        Assert.Equal(new[] { 1, 5, 2, 6 }, ids);
    }

    [Fact]
    public void LargeOrderIds_ThresholdTooHigh_ReturnsEmpty()
    {
        Assert.Empty(OrderReports.LargeOrderIds(TestData.Orders(), 1_000_000m));
    }

    [Fact]
    public void TopProducts_ByQuantityInCompletedOrders()
    {
        // Completed: Pen 10, Book 3+1+2 = 6, Mouse 2+1 = 3, Laptop 1 (Laptop из #5 — New, не считается)
        Assert.Equal(new[] { "Pen", "Book" }, OrderReports.TopProducts(TestData.Orders(), 2));
        Assert.Equal(new[] { "Pen", "Book", "Mouse", "Laptop" }, OrderReports.TopProducts(TestData.Orders(), 10));
    }

    [Fact]
    public void TopProducts_TieBrokenByNameOrdinal()
    {
        var orders = new List<Order>
        {
            new(1, "X", new DateOnly(2025, 1, 1), OrderStatus.Completed,
                [new("Zeta", "C", 5, 1m), new("Alpha", "C", 5, 1m), new("Mid", "C", 7, 1m)]),
        };

        Assert.Equal(new[] { "Mid", "Alpha", "Zeta" }, OrderReports.TopProducts(orders, 3));
    }

    [Fact]
    public void TopProducts_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderReports.TopProducts(TestData.Orders(), -1));
    }

    [Fact]
    public void FirstOrderOf_ReturnsEarliestOrder()
    {
        Order? first = OrderReports.FirstOrderOf(TestData.Orders(), "Alice");

        Assert.NotNull(first);
        Assert.Equal(1, first.Id);
    }

    [Fact]
    public void FirstOrderOf_SameDate_ReturnsSmallestId()
    {
        var date = new DateOnly(2025, 5, 1);
        var orders = new List<Order>
        {
            new(9, "Zoe", date, OrderStatus.New, []),
            new(7, "Zoe", date, OrderStatus.New, []),
            new(8, "Zoe", date.AddDays(-1), OrderStatus.Cancelled, []),
        };

        Assert.Equal(8, OrderReports.FirstOrderOf(orders, "Zoe")!.Id);
        orders.RemoveAt(2);
        Assert.Equal(7, OrderReports.FirstOrderOf(orders, "Zoe")!.Id);
    }

    [Fact]
    public void FirstOrderOf_UnknownCustomer_ReturnsNull()
    {
        Assert.Null(OrderReports.FirstOrderOf(TestData.Orders(), "Nobody"));
    }

    [Theory]
    [InlineData("Bob", 1, true)]     // 1 Completed, отменённых нет
    [InlineData("Bob", 2, false)]    // #5 — New, Completed только один
    [InlineData("Alice", 2, false)]  // 2 Completed, но есть отменённый #3
    [InlineData("Carol", 1, true)]
    [InlineData("Nobody", 1, false)]
    public void IsLoyalCustomer_ChecksCompletedCountAndNoCancellations(string customer, int min, bool expected)
    {
        Assert.Equal(expected, OrderReports.IsLoyalCustomer(TestData.Orders(), customer, min));
    }

    [Fact]
    public void IsLoyalCustomer_MinLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderReports.IsLoyalCustomer(TestData.Orders(), "Bob", 0));
    }
}
