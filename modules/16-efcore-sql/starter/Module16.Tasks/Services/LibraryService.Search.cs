namespace Module16.Tasks.Services;

// Задача 7 ⭐. Фильтрация + пагинация на IQueryable.
public sealed partial class LibraryService
{
    /// <summary>
    /// Поиск книг с фильтрами и пагинацией — всё должно выполняться В БАЗЕ (строим IQueryable, а не фильтруем список в памяти).
    /// <list type="number">
    /// <item>criteria.Page &lt; 1 или PageSize вне [1; 100] → ArgumentOutOfRangeException.</item>
    /// <item>Фильтры применяются только если заданы: TitleContains (без учёта регистра, после Trim), AuthorId,
    /// YearFrom/YearTo (включительно), Genre (у книги есть жанр с таким Name), OnlyAvailable (есть свободный экземпляр).</item>
    /// <item>TotalCount — после фильтров, до пагинации.</item>
    /// <item>Сортировка: Title, затем Id. Затем Skip/Take.</item>
    /// <item>Проекция в <see cref="BookListItem"/>: AuthorName — имя автора, AvailableCopies = CopiesTotal − активные выдачи.</item>
    /// </list>
    /// </summary>
    public Task<PagedResult<BookListItem>> SearchBooksAsync(BookSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
