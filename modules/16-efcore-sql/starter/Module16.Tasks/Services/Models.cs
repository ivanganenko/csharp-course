namespace Module16.Tasks.Services;

// Результаты запросов (read models / DTO). Records: неизменяемые, со сравнением по значению — удобно в тестах.

/// <summary>Строка статистики автора (задача 3).</summary>
public sealed record AuthorStats(string AuthorName, int BookCount, int LoanCount);

/// <summary>Строка отчёта о должниках (задача 3).</summary>
public sealed record OverdueReportLine(string MemberName, int OverdueCount);

/// <summary>Строка отчёта «автор и его книги» (задача 6).</summary>
public sealed record AuthorReportLine(string AuthorName, IReadOnlyList<string> BookTitles);

/// <summary>Элемент результата поиска (задача 7).</summary>
public sealed record BookListItem(int Id, string Title, string AuthorName, int PublishedYear, int AvailableCopies);

/// <summary>Страница результатов (задача 7).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>Критерии поиска книг (задача 7). Незаданный критерий (null) не фильтрует.</summary>
public sealed record BookSearchCriteria
{
    /// <summary>Подстрока названия без учёта регистра (для латиницы — см. README про SQLite). Пустая/пробелы — не фильтр.</summary>
    public string? TitleContains { get; init; }

    public int? AuthorId { get; init; }

    /// <summary>Год издания ≥ YearFrom.</summary>
    public int? YearFrom { get; init; }

    /// <summary>Год издания ≤ YearTo.</summary>
    public int? YearTo { get; init; }

    /// <summary>Точное имя жанра.</summary>
    public string? Genre { get; init; }

    /// <summary>true — только книги, у которых есть свободный экземпляр.</summary>
    public bool OnlyAvailable { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

/// <summary>Сущность не найдена (книга, читатель, выдача).</summary>
public sealed class NotFoundException(string message) : Exception(message)
{
}

/// <summary>Нарушено правило библиотеки (нет свободных экземпляров, есть просрочка и т.п.).</summary>
public sealed class LibraryRuleException(string message) : Exception(message)
{
}
