namespace Module16.Tasks.Services;

// Задача 3. Агрегаты: COUNT / GROUP BY / ORDER BY — должны считаться В БАЗЕ, одним запросом,
// а не загрузкой всех строк в память.
public sealed partial class LibraryService
{
    /// <summary>
    /// Топ авторов по числу выдач их книг (все выдачи, включая возвращённые).
    /// <list type="bullet">
    /// <item>Только авторы, у которых LoanCount &gt; 0.</item>
    /// <item>BookCount — количество книг автора (всех, не только выданных).</item>
    /// <item>Сортировка: LoanCount по убыванию, затем AuthorName по возрастанию.</item>
    /// <item>Не больше <paramref name="top"/> строк. top &lt; 1 → ArgumentOutOfRangeException.</item>
    /// </list>
    /// </summary>
    public Task<List<AuthorStats>> GetTopAuthorsAsync(int top, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Отчёт о должниках на дату <paramref name="today"/>: просроченная выдача — ReturnedOn == null и DueOn &lt; today.
    /// Только читатели с хотя бы одной просрочкой; сортировка: OverdueCount по убыванию, затем MemberName.
    /// Дата передаётся параметром (а не DateTime.Today внутри) — так метод детерминирован и тестируем.
    /// </summary>
    public Task<List<OverdueReportLine>> GetOverdueReportAsync(DateOnly today, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
