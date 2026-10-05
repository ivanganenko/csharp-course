using Module15.Api.Models;

namespace Module15.Api.Querying;

public sealed record TaskQueryParameters(string? Status, string? Search, int Page, int PageSize);

public static class TaskQuery
{
    private const string StatusAll = "all";
    private const string StatusActive = "active";
    private const string StatusCompleted = "completed";

    public static Dictionary<string, string[]> Validate(TaskQueryParameters query, int maxPageSize)
    {
        ArgumentNullException.ThrowIfNull(query);

        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrEmpty(query.Status) && !IsKnownStatus(query.Status))
        {
            errors["Status"] = [$"Допустимые значения: {StatusAll}, {StatusActive}, {StatusCompleted}."];
        }

        if (query.Page < 1)
        {
            errors["Page"] = ["Номер страницы начинается с 1."];
        }

        // Верхняя граница pageSize — защита сервера: без неё один запрос «?pageSize=1000000»
        // заставит сериализовать всю базу.
        if (query.PageSize < 1 || query.PageSize > maxPageSize)
        {
            errors["PageSize"] = [$"Размер страницы — от 1 до {maxPageSize}."];
        }

        return errors;
    }

    public static PagedResponse<TaskResponse> Apply(IEnumerable<TaskItem> source, TaskQueryParameters query)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentOutOfRangeException.ThrowIfLessThan(query.Page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(query.PageSize, 1);

        // Фильтры добавляем «по цепочке» только если параметр задан — тот же приём
        // потом будет на IQueryable в EF Core (модуль 16), где он превратится в WHERE.
        var filtered = source;

        if (string.Equals(query.Status, StatusActive, StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(t => !t.IsCompleted);
        }
        else if (string.Equals(query.Status, StatusCompleted, StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(t => t.IsCompleted);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();

            // OrdinalIgnoreCase: результат не зависит от культуры сервера (CurrentCulture на Windows и Linux может отличаться).
            filtered = filtered.Where(t =>
                t.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (t.Description is not null && t.Description.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        // ToList один раз: дальше используем результат дважды (Count и страница) —
        // без материализации фильтры выполнились бы два раза (multiple enumeration, модуль 11).
        var ordered = filtered
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.Id) // стабильный «тай-брейкер»: без него порядок равных элементов и страницы «плавают»
            .ToList();

        var items = ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => t.ToResponse())
            .ToList();

        return new PagedResponse<TaskResponse>(items, query.Page, query.PageSize, ordered.Count);
    }

    private static bool IsKnownStatus(string status) =>
        string.Equals(status, StatusAll, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, StatusActive, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, StatusCompleted, StringComparison.OrdinalIgnoreCase);
}
