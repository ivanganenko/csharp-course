using Module16.Tasks.Data;

namespace Module16.Tasks.Services;

/// <summary>
/// Сервис библиотеки. Разбит на partial-файлы по задачам:
/// LibraryService.Queries.cs (2), .Statistics.cs (3), .Lending.cs (4), .Catalog.cs (5), .Reports.cs (6), .Search.cs (7).
/// Контекст приходит через конструктор (в ASP.NET Core его внедрил бы DI как Scoped).
/// </summary>
public sealed partial class LibraryService
{
    /// <summary>Срок выдачи в днях.</summary>
    public const int LoanPeriodDays = 14;

    /// <summary>Сколько книг одновременно может быть на руках у читателя.</summary>
    public const int MaxActiveLoansPerMember = 3;

    /// <summary>Максимальная длина названия книги (совпадает с конфигурацией модели).</summary>
    public const int MaxTitleLength = 200;

    private readonly LibraryDbContext _db;

    public LibraryService(LibraryDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }
}
