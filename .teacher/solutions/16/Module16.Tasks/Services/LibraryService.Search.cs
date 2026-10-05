using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    private const int MaxSearchPageSize = 100;

    public async Task<PagedResult<BookListItem>> SearchBooksAsync(BookSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        ArgumentOutOfRangeException.ThrowIfLessThan(criteria.Page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(criteria.PageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(criteria.PageSize, MaxSearchPageSize);

        // IQueryable<Book> — это ещё не данные, а «описание запроса» (expression tree).
        // Каждый Where лишь дополняет его; SQL строится и выполняется только на CountAsync/ToListAsync.
        // Поэтому условные фильтры ничего не стоят и все попадают в один WHERE.
        IQueryable<Book> query = _db.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(criteria.TitleContains))
        {
            // ToLowerInvariant считается в C# один раз (параметр запроса), ToLower() в лямбде → SQL lower().
            // Оговорка: lower() в SQLite работает только для ASCII — для кириллицы нужна другая collation/ICU.
            var term = criteria.TitleContains.Trim().ToLowerInvariant();
            query = query.Where(b => b.Title.ToLower().Contains(term));
        }

        if (criteria.AuthorId is { } authorId)
        {
            query = query.Where(b => b.AuthorId == authorId);
        }

        if (criteria.YearFrom is { } yearFrom)
        {
            query = query.Where(b => b.PublishedYear >= yearFrom);
        }

        if (criteria.YearTo is { } yearTo)
        {
            query = query.Where(b => b.PublishedYear <= yearTo);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Genre))
        {
            var genre = criteria.Genre;
            query = query.Where(b => b.Genres.Any(g => g.Name == genre)); // EXISTS (SELECT ... FROM BookGenre JOIN Genres ...)
        }

        if (criteria.OnlyAvailable)
        {
            query = query.Where(b => b.Loans.Count(l => l.ReturnedOn == null) < b.CopiesTotal);
        }

        // Отдельный COUNT(*) по тем же фильтрам — до Skip/Take.
        var totalCount = await query.CountAsync(cancellationToken);

        // OrderBy обязателен ДО Skip/Take: без него SQL не гарантирует порядок, и страницы «перемешаются».
        // ThenBy(Id) — тай-брейкер для одинаковых названий.
        var items = await query
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Id)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Select(b => new BookListItem(
                b.Id,
                b.Title,
                b.Author.Name,
                b.PublishedYear,
                b.CopiesTotal - b.Loans.Count(l => l.ReturnedOn == null)))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookListItem>(items, totalCount, criteria.Page, criteria.PageSize);
    }
}
