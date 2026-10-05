using Microsoft.EntityFrameworkCore;

namespace Module16.Tasks.Services;

// Задача 6 🧪. Проблема N+1.
public sealed partial class LibraryService
{
    /// <summary>
    /// ГОТОВО, но с проблемой N+1: один запрос за авторами + по одному запросу на КАЖДОГО автора.
    /// При 4 авторах — 5 SQL-команд, при 10 000 авторах — 10 001. Включи логирование SQL и убедись сам.
    /// Не меняй этот метод — тесты сравнивают с ним результат оптимизированной версии.
    /// </summary>
    public async Task<List<AuthorReportLine>> BuildAuthorReportNaiveAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _db.Authors
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        var report = new List<AuthorReportLine>();
        foreach (var author in authors)
        {
            // ⚠️ Запрос в цикле — это и есть N+1.
            var titles = await _db.Books
                .Where(b => b.AuthorId == author.Id)
                .Select(b => b.Title)
                .ToListAsync(cancellationToken);

            titles.Sort(StringComparer.Ordinal);
            report.Add(new AuthorReportLine(author.Name, titles));
        }

        return report;
    }

    /// <summary>
    /// Тот же отчёт (авторы по Name, у каждого названия книг по Title, StringComparer.Ordinal;
    /// авторы без книг — с пустым списком), но не больше 2 SQL-команд независимо от числа авторов.
    /// </summary>
    public Task<List<AuthorReportLine>> BuildAuthorReportAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
