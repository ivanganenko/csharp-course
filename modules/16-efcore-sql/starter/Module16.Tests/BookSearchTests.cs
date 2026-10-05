using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>Задача 7 ⭐. Фильтрация и пагинация на IQueryable.</summary>
public sealed class BookSearchTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly LibraryData _data;

    public BookSearchTests()
    {
        _data = LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    private async Task<PagedResult<BookListItem>> SearchAsync(BookSearchCriteria criteria)
    {
        await using var db = _database.CreateContext();
        return await new LibraryService(db).SearchBooksAsync(criteria);
    }

    private static string[] Titles(PagedResult<BookListItem> result) => result.Items.Select(b => b.Title).ToArray();

    [Fact]
    public async Task NoFilters_ReturnsAllSortedByTitle()
    {
        var result = await SearchAsync(new BookSearchCriteria());

        Assert.Equal(new[] { "Atlas of Fog", "Quiet Orbit", "Ring of Salt", "Stone Garden", "The Ring Keeper" }, Titles(result));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task Projection_FillsAuthorNameAndAvailableCopies()
    {
        var result = await SearchAsync(new BookSearchCriteria());

        var ringKeeper = result.Items.Single(b => b.Title == "The Ring Keeper");
        Assert.Equal(new BookListItem(_data.RingKeeper.Id, "The Ring Keeper", "Mira Oakwood", 2001, AvailableCopies: 1), ringKeeper);
        Assert.Equal(0, result.Items.Single(b => b.Title == "Stone Garden").AvailableCopies);
        Assert.Equal(3, result.Items.Single(b => b.Title == "Ring of Salt").AvailableCopies);
    }

    [Fact]
    public async Task TitleContains_IgnoresCaseAndSpaces()
    {
        var result = await SearchAsync(new BookSearchCriteria { TitleContains = "  RING " });

        Assert.Equal(new[] { "Ring of Salt", "The Ring Keeper" }, Titles(result));
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task AuthorFilter()
    {
        var result = await SearchAsync(new BookSearchCriteria { AuthorId = _data.Clara.Id });

        Assert.Equal(new[] { "Atlas of Fog", "Quiet Orbit" }, Titles(result));
    }

    [Fact]
    public async Task YearRange_IsInclusive()
    {
        var result = await SearchAsync(new BookSearchCriteria { YearFrom = 2010, YearTo = 2015 });

        Assert.Equal(new[] { "Quiet Orbit", "Ring of Salt" }, Titles(result));
    }

    [Fact]
    public async Task GenreFilter()
    {
        var result = await SearchAsync(new BookSearchCriteria { Genre = "Mystery" });

        Assert.Equal(new[] { "Atlas of Fog", "Stone Garden", "The Ring Keeper" }, Titles(result));
    }

    [Fact]
    public async Task OnlyAvailable_ExcludesBooksWithoutFreeCopies()
    {
        var result = await SearchAsync(new BookSearchCriteria { OnlyAvailable = true });

        Assert.Equal(new[] { "Atlas of Fog", "Ring of Salt", "The Ring Keeper" }, Titles(result));
    }

    [Fact]
    public async Task CombinedFilters()
    {
        var result = await SearchAsync(new BookSearchCriteria { TitleContains = "ring", Genre = "Fantasy", OnlyAvailable = true, YearFrom = 2005 });

        Assert.Equal(new[] { "Ring of Salt" }, Titles(result));
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task SecondPage_TotalCountIsBeforePaging()
    {
        var result = await SearchAsync(new BookSearchCriteria { Page = 2, PageSize = 2 });

        Assert.Equal(new[] { "Ring of Salt", "Stone Garden" }, Titles(result));
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task PageBeyondEnd_ReturnsEmptyItems()
    {
        var result = await SearchAsync(new BookSearchCriteria { Page = 4, PageSize = 2 });

        Assert.Empty(result.Items);
        Assert.Equal(5, result.TotalCount);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task InvalidPaging_Throws(int page, int pageSize)
    {
        await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(
            () => SearchAsync(new BookSearchCriteria { Page = page, PageSize = pageSize }));
    }

    [Fact]
    public async Task NullCriteria_ThrowsArgumentNullException()
    {
        await using var db = _database.CreateContext();

        await Assert.ThrowsAsync<ArgumentNullException>(() => new LibraryService(db).SearchBooksAsync(null!));
    }
}
