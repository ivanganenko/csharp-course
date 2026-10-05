using Module15.Api.Models;

namespace Module15.Api.Querying;

/// <summary>Параметры запроса GET /api/tasks?status=&amp;search=&amp;page=&amp;pageSize= (задача 6).</summary>
public sealed record TaskQueryParameters(string? Status, string? Search, int Page, int PageSize);

/// <summary>
/// Задача 6 ⭐. Фильтрация и пагинация списка задач.
/// Логика вынесена из эндпоинта в чистые статические методы: их легко покрыть unit-тестами,
/// а эндпоинт остаётся тонким (привязка параметров → вызов → результат).
/// </summary>
public static class TaskQuery
{
    /// <summary>
    /// Проверяет параметры. Возвращает словарь ошибок (пустой, если всё корректно). Ключи:
    /// <list type="bullet">
    /// <item>"Status": допустимы null/пустая строка, "all", "active", "completed" — без учёта регистра;</item>
    /// <item>"Page": должно быть ≥ 1;</item>
    /// <item>"PageSize": должно быть от 1 до <paramref name="maxPageSize"/> включительно.</item>
    /// </list>
    /// </summary>
    public static Dictionary<string, string[]> Validate(TaskQueryParameters query, int maxPageSize)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Применяет фильтры, сортировку и пагинацию (параметры считаются уже проверенными через <see cref="Validate"/>;
    /// если Page &lt; 1 или PageSize &lt; 1 — бросает ArgumentOutOfRangeException).
    /// <list type="number">
    /// <item>Status: "active" → только незавершённые; "completed" → только завершённые; null/""/"all" → все. Без учёта регистра.</item>
    /// <item>Search (если не null и не пробелы): после Trim() ищем подстроку без учёта регистра
    /// (StringComparison.OrdinalIgnoreCase) в Title ИЛИ в Description.</item>
    /// <item>Сортировка: сначала Priority по убыванию, при равенстве — Id по возрастанию.</item>
    /// <item>TotalCount — количество ПОСЛЕ фильтрации, но ДО пагинации.</item>
    /// <item>Items — страница номер Page (с 1) размером PageSize. Страница за пределами — пустой список.</item>
    /// </list>
    /// </summary>
    public static PagedResponse<TaskResponse> Apply(IEnumerable<TaskItem> source, TaskQueryParameters query)
    {
        throw new NotImplementedException();
    }
}
