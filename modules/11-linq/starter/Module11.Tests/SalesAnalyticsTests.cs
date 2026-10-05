using Module11.Tasks;

namespace Module11.Tests;

public class SalesAnalyticsTests
{
    [Fact]
    public void RevenueByCategory_SumsCompletedLinesPerCategory()
    {
        var result = SalesAnalytics.RevenueByCategory(TestData.Orders());

        // Electronics: 1000 + 50 (#1) + 25 (#4); Books: 60 (#2) + 20 (#4) + 40 (#6); Stationery: 20 (#6)
        Assert.Equal(3, result.Count);
        Assert.Equal(1075m, result["Electronics"]);
        Assert.Equal(120m, result["Books"]);
        Assert.Equal(20m, result["Stationery"]);
    }

    [Fact]
    public void RevenueByCategory_NoCompletedOrders_ReturnsEmpty()
    {
        var orders = TestData.Orders().Where(o => o.Status != OrderStatus.Completed);

        Assert.Empty(SalesAnalytics.RevenueByCategory(orders));
    }

    [Fact]
    public void SalesByMonth_GroupsCompletedOrdersChronologically()
    {
        var result = SalesAnalytics.SalesByMonth(TestData.Orders());

        Assert.Equal(
            new[]
            {
                new MonthlySales(2025, 1, 2, 1110m),
                new MonthlySales(2025, 2, 1, 45m),
                new MonthlySales(2025, 3, 1, 60m),
            },
            result);
    }

    [Fact]
    public void SalesByMonth_SortsAcrossYears()
    {
        var orders = new List<Order>
        {
            new(1, "A", new DateOnly(2025, 1, 10), OrderStatus.Completed, [new("P", "C", 1, 10m)]),
            new(2, "A", new DateOnly(2024, 12, 31), OrderStatus.Completed, [new("P", "C", 1, 5m)]),
            new(3, "A", new DateOnly(2024, 2, 1), OrderStatus.Completed, [new("P", "C", 1, 1m)]),
        };

        var result = SalesAnalytics.SalesByMonth(orders);

        Assert.Equal(new[] { (2024, 2), (2024, 12), (2025, 1) }, result.Select(m => (m.Year, m.Month)));
    }

    [Fact]
    public void OrderIdsByCustomer_IncludesAllStatuses_InSourceOrder()
    {
        var lookup = SalesAnalytics.OrderIdsByCustomer(TestData.Orders());

        Assert.Equal(new[] { 1, 3, 6 }, lookup["Alice"]);
        Assert.Equal(new[] { 2, 5 }, lookup["Bob"]);
        Assert.Equal(new[] { 4 }, lookup["Carol"]);
        Assert.Equal(3, lookup.Count);
    }

    [Fact]
    public void OrderIdsByCustomer_UnknownCustomer_ReturnsEmptySequence()
    {
        var lookup = SalesAnalytics.OrderIdsByCustomer(TestData.Orders());

        Assert.Empty(lookup["Nobody"]);
    }

    [Fact]
    public void AverageOrderValueByCustomer_CompletedOnly()
    {
        var result = SalesAnalytics.AverageOrderValueByCustomer(TestData.Orders());

        // Alice: (1050 + 60) / 2; Bob: 60 (#5 — New); Carol: 45
        Assert.Equal(3, result.Count);
        Assert.Equal(555m, result["Alice"]);
        Assert.Equal(60m, result["Bob"]);
        Assert.Equal(45m, result["Carol"]);
    }

    [Fact]
    public void RevenueChanges_ReturnsDayToDayDifferences()
    {
        var changes = SalesAnalytics.RevenueChanges(new[] { 100m, 150m, 120m, 120m });

        Assert.Equal(new[] { 50m, -30m, 0m }, changes);
    }

    [Fact]
    public void RevenueChanges_LessThanTwoValues_ReturnsEmpty()
    {
        Assert.Empty(SalesAnalytics.RevenueChanges(new[] { 100m }));
        Assert.Empty(SalesAnalytics.RevenueChanges(Array.Empty<decimal>()));
    }
}
