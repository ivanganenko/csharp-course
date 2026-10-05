using Microsoft.EntityFrameworkCore;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    public async Task<List<AuthorReportLine>> BuildAuthorReportNaiveAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _db.Authors
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var report = new List<AuthorReportLine>();
        foreach (var author in authors)
        {
            var titles = await _db.Books
                .Where(b => b.AuthorId == author.Id)
                .Select(b => b.Title)
                .ToListAsync(cancellationToken);

            titles.Sort(StringComparer.Ordinal);
            report.Add(new AuthorReportLine(author.Name, titles));
        }

        return report;
    }

    public async Task<List<AuthorReportLine>> BuildAuthorReportAsync(CancellationToken cancellationToken = default)
    {
        // Один запрос вместо 1 + N: проекция с вложенной коллекцией. EF сделает LEFT JOIN авторов с книгами
        // и соберёт списки на клиенте. Берём только нужные колонки (Name, Title), а не целые сущности,
        // и без tracking (проекция в анонимный тип не отслеживается).
        // Альтернатива: _db.Authors.Include(a => a.Books).AsNoTracking() — тоже один запрос, но тянет все колонки.
        var rows = await _db.Authors
            .OrderBy(a => a.Name)
            .Select(a => new
            {
                a.Name,
                Titles = a.Books.Select(b => b.Title).ToList(),
            })
            .ToListAsync(cancellationToken);

        // Сортируем названия в памяти ординально — так же, как наивная версия (результат не зависит от collation БД).
        return rows
            .Select(r => new AuthorReportLine(r.Name, r.Titles.OrderBy(t => t, StringComparer.Ordinal).ToList()))
            .ToList();
    }
}
