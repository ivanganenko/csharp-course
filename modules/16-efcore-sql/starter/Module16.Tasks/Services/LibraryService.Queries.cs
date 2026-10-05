using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

// Задача 2. Загрузка связанных данных: Include / ThenInclude.
// Важно: тесты вызывают методы на НОВОМ контексте (не на том, которым заполняли базу),
// поэтому навигационные свойства будут пустыми/null, если их явно не загрузить.
public sealed partial class LibraryService
{
    /// <summary>
    /// Книга по Id вместе с автором (Author) и жанрами (Genres, отсортированы по Name).
    /// Нет такой книги → null.
    /// </summary>
    public Task<Book?> GetBookWithDetailsAsync(int bookId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Автор по Id вместе с книгами (Books), отсортированными по PublishedYear по возрастанию.
    /// Нет такого автора → null.
    /// </summary>
    public Task<Author?> GetAuthorWithBooksAsync(int authorId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Активные (ReturnedOn == null) выдачи читателя, отсортированные по DueOn,
    /// с загруженными Loan.Book и Loan.Book.Author (ThenInclude).
    /// Нет читателя или нет выдач → пустой список.
    /// </summary>
    public Task<List<Loan>> GetActiveLoansForMemberAsync(int memberId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
