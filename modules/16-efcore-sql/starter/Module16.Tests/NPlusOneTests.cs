using Module16.Tasks.Entities;
using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>Задача 6 🧪. Проблема N+1: считаем реальные SQL-команды через LogTo.</summary>
public sealed class NPlusOneTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public NPlusOneTests()
    {
        LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    private static void AssertSameReport(IReadOnlyList<AuthorReportLine> expected, IReadOnlyList<AuthorReportLine> actual)
    {
        // Records сравниваются по значению, НО свойство-список сравнивается по ссылке,
        // поэтому сравниваем поля вручную.
        Assert.Equal(expected.Select(l => l.AuthorName).ToArray(), actual.Select(l => l.AuthorName).ToArray());
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i].BookTitles.ToArray(), actual[i].BookTitles.ToArray());
        }
    }

    [Fact]
    public async Task NaiveReport_ExecutesOneQueryPerAuthor()
    {
        var commands = new List<string>();
        await using var db = _database.CreateContext(commands.Add);
        var service = new LibraryService(db);

        await service.BuildAuthorReportNaiveAsync();

        // 1 запрос за авторами + 4 запроса за книгами = 5. Это и есть N+1.
        Assert.Equal(5, commands.Count);
    }

    [Fact]
    public async Task OptimizedReport_ReturnsSameDataAsNaive()
    {
        await using var naiveDb = _database.CreateContext();
        var expected = await new LibraryService(naiveDb).BuildAuthorReportNaiveAsync();

        await using var db = _database.CreateContext();
        var actual = await new LibraryService(db).BuildAuthorReportAsync();

        AssertSameReport(expected, actual);
    }

    [Fact]
    public async Task OptimizedReport_HasExpectedContent()
    {
        await using var db = _database.CreateContext();

        var report = await new LibraryService(db).BuildAuthorReportAsync();

        Assert.Equal(new[] { "Boris Stone", "Clara Vale", "Dan Empty", "Mira Oakwood" }, report.Select(l => l.AuthorName).ToArray());
        Assert.Equal(new[] { "Ring of Salt", "Stone Garden" }, report[0].BookTitles.ToArray());
        Assert.Empty(report[2].BookTitles);
    }

    [Fact]
    public async Task OptimizedReport_ExecutesAtMostTwoCommands()
    {
        var commands = new List<string>();
        await using var db = _database.CreateContext(commands.Add);

        await new LibraryService(db).BuildAuthorReportAsync();

        Assert.InRange(commands.Count, 1, 2);
    }

    [Fact]
    public async Task OptimizedReport_CommandCountDoesNotGrowWithAuthors()
    {
        await using (var seed = _database.CreateContext())
        {
            for (var i = 0; i < 20; i++)
            {
                seed.Authors.Add(new Author { Name = $"Extra Author {i:D2}" });
            }

            await seed.SaveChangesAsync();
        }

        var commands = new List<string>();
        await using var db = _database.CreateContext(commands.Add);

        var report = await new LibraryService(db).BuildAuthorReportAsync();

        Assert.Equal(24, report.Count);
        Assert.InRange(commands.Count, 1, 2);
    }
}
