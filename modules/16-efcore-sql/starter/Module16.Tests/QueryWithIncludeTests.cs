using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>
/// Задача 2. Include / ThenInclude.
/// Данные кладём одним контекстом, а читаем — НОВЫМ: иначе навигации заполнились бы
/// из change tracker (relationship fixup) и тест прошёл бы даже без Include.
/// </summary>
public sealed class QueryWithIncludeTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly LibraryData _data;

    public QueryWithIncludeTests()
    {
        _data = LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task GetBookWithDetails_LoadsAuthorAndSortedGenres()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var book = await service.GetBookWithDetailsAsync(_data.RingKeeper.Id);

        Assert.NotNull(book);
        Assert.Equal("The Ring Keeper", book.Title);
        Assert.NotNull(book.Author);
        Assert.Equal("Mira Oakwood", book.Author.Name);
        Assert.Equal(new[] { "Fantasy", "Mystery" }, book.Genres.Select(g => g.Name).ToArray());
    }

    [Fact]
    public async Task GetBookWithDetails_MissingBook_ReturnsNull()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var book = await service.GetBookWithDetailsAsync(999);

        Assert.Null(book);
    }

    [Fact]
    public async Task GetAuthorWithBooks_LoadsBooksSortedByYear()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var author = await service.GetAuthorWithBooksAsync(_data.Boris.Id);

        Assert.NotNull(author);
        Assert.Equal(new[] { "Stone Garden", "Ring of Salt" }, author.Books.Select(b => b.Title).ToArray());
    }

    [Fact]
    public async Task GetAuthorWithBooks_AuthorWithoutBooks_ReturnsEmptyList()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var author = await service.GetAuthorWithBooksAsync(_data.Dan.Id);

        Assert.NotNull(author);
        Assert.Empty(author.Books);
    }

    [Fact]
    public async Task GetActiveLoansForMember_ReturnsOnlyActive_SortedByDueOn_WithBookAndAuthor()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var loans = await service.GetActiveLoansForMemberAsync(_data.Olga.Id);

        // L3 (due 15.02) раньше L4 (due 06.03).
        Assert.Equal(new[] { _data.L3.Id, _data.L4.Id }, loans.Select(l => l.Id).ToArray());
        Assert.All(loans, loan => Assert.Null(loan.ReturnedOn));
        Assert.Equal("Quiet Orbit", loans[0].Book.Title);
        Assert.Equal("Clara Vale", loans[0].Book.Author.Name);
        Assert.Equal("Mira Oakwood", loans[1].Book.Author.Name);
    }

    [Fact]
    public async Task GetActiveLoansForMember_ExcludesReturnedLoans()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var loans = await service.GetActiveLoansForMemberAsync(_data.Ivan.Id);

        // У Ивана 3 выдачи, но L1 и L5 возвращены.
        var loan = Assert.Single(loans);
        Assert.Equal(_data.L2.Id, loan.Id);
        Assert.Equal("Boris Stone", loan.Book.Author.Name);
    }

    [Fact]
    public async Task GetActiveLoansForMember_UnknownMember_ReturnsEmpty()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var loans = await service.GetActiveLoansForMemberAsync(999);

        Assert.Empty(loans);
    }
}
