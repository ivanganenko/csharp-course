using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    public Task<Book?> GetBookWithDetailsAsync(int bookId, CancellationToken cancellationToken = default)
    {
        // Eager loading: Include превращается в JOIN — автор и жанры приходят тем же запросом.
        // Без Include навигации остались бы null/пустыми: lazy loading в EF Core по умолчанию выключен
        // (и это хорошо — он прячет N+1).
        return _db.Books
            .Include(b => b.Author)
            .Include(b => b.Genres.OrderBy(g => g.Name)) // filtered include (EF Core 5+): сортировка прямо в запросе
            .FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
    }

    public Task<Author?> GetAuthorWithBooksAsync(int authorId, CancellationToken cancellationToken = default)
    {
        return _db.Authors
            .Include(a => a.Books.OrderBy(b => b.PublishedYear))
            .FirstOrDefaultAsync(a => a.Id == authorId, cancellationToken);
    }

    public Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId, CancellationToken cancellationToken = default)
    {
        return _db.Loans
            .Where(l => l.MemberId == memberId && l.ReturnedOn == null)
            .Include(l => l.Book)
                .ThenInclude(b => b.Author) // ThenInclude — следующий уровень от только что включённой навигации
            .OrderBy(l => l.DueOn)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }
}
