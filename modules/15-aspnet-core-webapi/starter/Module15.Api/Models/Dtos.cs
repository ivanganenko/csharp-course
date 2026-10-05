namespace Module15.Api.Models;

// DTO (Data Transfer Object) — «контракт» API: что клиент присылает и что получает.

/// <summary>Тело запроса POST /api/tasks.</summary>
public sealed class CreateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    /// <summary>Если клиент не прислал priority — используется 3.</summary>
    public int Priority { get; set; } = 3;
}

/// <summary>Тело запроса PUT /api/tasks/{id} — полная замена задачи.</summary>
public sealed class UpdateTaskRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public int Priority { get; set; } = 3;

    public bool IsCompleted { get; set; }
}

/// <summary>То, что API возвращает клиенту.</summary>
public sealed record TaskResponse(int Id, string Title, string? Description, int Priority, bool IsCompleted);

/// <summary>Страница результатов (задача 6).</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>Ответ GET /api/diagnostics/counters (задача 4).</summary>
public sealed record CountersResponse(int Singleton, int Scoped, int Transient);

public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem item) =>
        new(item.Id, item.Title, item.Description, item.Priority, item.IsCompleted);
}
