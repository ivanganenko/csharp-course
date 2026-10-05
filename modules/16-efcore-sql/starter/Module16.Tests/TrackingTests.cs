using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>Задача 5. Tracking vs AsNoTracking.</summary>
public sealed class TrackingTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly LibraryData _data;

    public TrackingTests()
    {
        _data = LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    private string LoadTitle(int bookId)
    {
        using var db = _database.CreateContext();
        return db.Books.Single(b => b.Id == bookId).Title;
    }

    [Fact]
    public async Task GetCatalog_ReturnsAllBooksSortedByTitle()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var catalog = await service.GetCatalogAsync();

        Assert.Equal(
            new[] { "Atlas of Fog", "Quiet Orbit", "Ring of Salt", "Stone Garden", "The Ring Keeper" },
            catalog.Select(b => b.Title).ToArray());
    }

    [Fact]
    public async Task GetCatalog_DoesNotTrackEntities()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await service.GetCatalogAsync();

        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task RenameBook_Existing_SavesTrimmedTitle()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var renamed = await service.RenameBookAsync(_data.QuietOrbit.Id, "  Quiet Orbit (2nd edition)  ");

        Assert.True(renamed);
        Assert.Equal("Quiet Orbit (2nd edition)", LoadTitle(_data.QuietOrbit.Id));
    }

    [Fact]
    public async Task RenameBook_AfterGetCatalog_StillSaves()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // Классическая ошибка: взять сущность из AsNoTracking-запроса, изменить и вызвать SaveChanges —
        // EF о ней не знает, и ничего не сохранится.
        await service.GetCatalogAsync();
        var renamed = await service.RenameBookAsync(_data.AtlasOfFog.Id, "Atlas of Mist");

        Assert.True(renamed);
        Assert.Equal("Atlas of Mist", LoadTitle(_data.AtlasOfFog.Id));
    }

    [Fact]
    public async Task RenameBook_Missing_ReturnsFalse()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        Assert.False(await service.RenameBookAsync(999, "Whatever"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RenameBook_BlankTitle_ThrowsArgumentException(string newTitle)
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.RenameBookAsync(_data.QuietOrbit.Id, newTitle));
        Assert.Equal("Quiet Orbit", LoadTitle(_data.QuietOrbit.Id));
    }

    [Fact]
    public async Task RenameBook_TooLongTitle_ThrowsArgumentException()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);
        var tooLong = new string('x', LibraryService.MaxTitleLength + 1);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.RenameBookAsync(_data.QuietOrbit.Id, tooLong));
    }

    [Fact]
    public async Task TrackedQuery_TracksEntities_ForComparison()
    {
        // Не задача, а демонстрация: обычный запрос (с tracking) кладёт сущности в change tracker.
        await using var db = _database.CreateContext();

        await db.Books.ToListAsync();

        Assert.Equal(5, db.ChangeTracker.Entries().Count());
    }
}
