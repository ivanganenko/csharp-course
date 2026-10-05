using Microsoft.EntityFrameworkCore;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    public async Task<List<AuthorStats>> GetTopAuthorsAsync(int top, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);

        // Один SQL-запрос: подзапросы COUNT(*) считаются в базе, в память приходят только `top` строк.
        // Проекция в анонимный тип, а не сразу в record: EF умеет сортировать по членам анонимного типа,
        // но не умеет «заглянуть» в параметры конструктора record'а для последующего OrderBy.
        var rows = await _db.Authors
            .Select(a => new
            {
                a.Name,
                BookCount = a.Books.Count,
                LoanCount = _db.Loans.Count(l => l.Book.AuthorId == a.Id),
            })
            .Where(x => x.LoanCount > 0)
            .OrderByDescending(x => x.LoanCount)
            .ThenBy(x => x.Name)
            .Take(top)
            .ToListAsync(cancellationToken);

        return rows.Select(x => new AuthorStats(x.Name, x.BookCount, x.LoanCount)).ToList();
    }

    public async Task<List<OverdueReportLine>> GetOverdueReportAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        // today — параметр запроса (@today), сравнение происходит в SQL.
        // Эквивалент через GroupBy: _db.Loans.Where(...).GroupBy(l => l.Member.FullName).Select(g => new { g.Key, Count = g.Count() }).
        var rows = await _db.Members
            .Select(m => new
            {
                m.FullName,
                OverdueCount = m.Loans.Count(l => l.ReturnedOn == null && l.DueOn < today),
            })
            .Where(x => x.OverdueCount > 0)
            .OrderByDescending(x => x.OverdueCount)
            .ThenBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return rows.Select(x => new OverdueReportLine(x.FullName, x.OverdueCount)).ToList();
    }
}
