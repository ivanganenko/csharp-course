using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    public Task<List<Book>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking: данные только читаем — не нужен снимок исходных значений и регистрация в change tracker.
        // Быстрее и меньше памяти. Цена: изменения таких объектов SaveChanges не увидит.
        return _db.Books
            .AsNoTracking()
            .OrderBy(b => b.Title)
            .ThenBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RenameBookAsync(int bookId, string newTitle, CancellationToken cancellationToken = default)
    {
        // Валидация до обращения к БД — не тратим запрос на заведомо плохие данные.
        ArgumentException.ThrowIfNullOrWhiteSpace(newTitle);
        var title = newTitle.Trim();
        if (title.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Название не длиннее {MaxTitleLength} символов", nameof(newTitle));
        }

        // Обычный (tracking) запрос: изменение свойства отследится, и SaveChanges выполнит UPDATE.
        // Если бы взяли сущность из GetCatalogAsync (AsNoTracking), SaveChanges ничего бы не сделал.
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        if (book is null)
        {
            return false;
        }

        book.Title = title;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
