using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>Задача 3. Агрегаты в базе.</summary>
public sealed class StatisticsTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public StatisticsTests()
    {
        LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task GetTopAuthors_SortsByLoanCountThenName_ExcludesAuthorsWithoutLoans()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var top = await service.GetTopAuthorsAsync(10);

        // Boris: L2 + L5 = 2; Mira: L1 + L4 = 2; Clara: L3 = 1; Dan — без выдач, не попадает.
        Assert.Equal(
            new[]
            {
                new AuthorStats("Boris Stone", BookCount: 2, LoanCount: 2),
                new AuthorStats("Mira Oakwood", BookCount: 1, LoanCount: 2),
                new AuthorStats("Clara Vale", BookCount: 2, LoanCount: 1),
            },
            top.ToArray());
    }

    [Fact]
    public async Task GetTopAuthors_RespectsTop()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var top = await service.GetTopAuthorsAsync(2);

        Assert.Equal(new[] { "Boris Stone", "Mira Oakwood" }, top.Select(s => s.AuthorName).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTopAuthors_NonPositiveTop_Throws(int top)
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(() => service.GetTopAuthorsAsync(top));
    }

    [Fact]
    public async Task GetOverdueReport_OnToday_ReturnsOnlyOlga()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var report = await service.GetOverdueReportAsync(LibraryData.Today);

        Assert.Equal(new[] { new OverdueReportLine("Olga Primerova", 2) }, report.ToArray());
    }

    [Fact]
    public async Task GetOverdueReport_LaterDate_IncludesIvan_SortedByCountDesc()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // 20 марта L2 Ивана (срок 15.03) тоже просрочена.
        var report = await service.GetOverdueReportAsync(new DateOnly(2026, 3, 20));

        Assert.Equal(
            new[] { new OverdueReportLine("Olga Primerova", 2), new OverdueReportLine("Ivan Testov", 1) },
            report.ToArray());
    }

    [Fact]
    public async Task GetOverdueReport_DueDateEqualsToday_IsNotOverdue()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // 15 марта — последний день срока L2, это ещё не просрочка.
        var report = await service.GetOverdueReportAsync(new DateOnly(2026, 3, 15));

        Assert.DoesNotContain(report, line => line.MemberName == "Ivan Testov");
    }

    [Fact]
    public async Task GetOverdueReport_EarlyDate_ReturnsEmpty()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var report = await service.GetOverdueReportAsync(new DateOnly(2026, 1, 1));

        Assert.Empty(report);
    }
}
