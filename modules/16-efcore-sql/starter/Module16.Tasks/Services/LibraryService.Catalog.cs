using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

// Задача 5. Tracking vs AsNoTracking.
public sealed partial class LibraryService
{
    /// <summary>
    /// Каталог только для чтения: все книги, отсортированные по Title.
    /// Должен использовать AsNoTracking — после вызова change tracker контекста пуст.
    /// </summary>
    public Task<List<Book>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Переименовать книгу. newTitle после Trim(): пустое/пробелы → ArgumentException;
    /// длиннее <see cref="MaxTitleLength"/> → ArgumentException.
    /// Нет книги → false. Иначе сохраняет новое название в базе и возвращает true.
    /// Должно работать и после вызова GetCatalogAsync на том же сервисе.
    /// </summary>
    public Task<bool> RenameBookAsync(int bookId, string newTitle, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
