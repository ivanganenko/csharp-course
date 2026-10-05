using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Module15.Api.Configuration;
using Module15.Api.ErrorHandling;
using Module15.Api.Models;
using Module15.Api.Querying;
using Module15.Api.Repositories;
using Module15.Api.Validation;

namespace Module15.Api.Endpoints;

/// <summary>
/// Minimal API эндпоинты для задач. Обработчики — обычные статические методы:
/// параметры «связываются» (model binding) автоматически:
/// int id — из маршрута, CreateTaskRequest — из JSON-тела, ITaskRepository — из DI-контейнера,
/// простые типы, которых нет в маршруте (string? status) — из query string.
/// Тип результата Results&lt;A, B&gt; перечисляет все возможные ответы — это и документация, и проверка компилятором.
/// </summary>
public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup — общий префикс маршрута для группы эндпоинтов.
        var group = app.MapGroup("/api/tasks");

        group.MapGet("/", GetTasks);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);
        group.MapPost("/{id:int}/complete", Complete);

        return app;
    }

    /// <summary>
    /// ГОТОВО (пример). GET /api/tasks/{id} → 200 + TaskResponse или 404.
    /// </summary>
    public static Results<Ok<TaskResponse>, NotFound> GetById(int id, ITaskRepository repository)
    {
        var task = repository.GetById(id);
        if (task is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(task.ToResponse());
    }

    /// <summary>
    /// ГОТОВО (пример для задачи 5). POST /api/tasks/{id}/complete — отметить задачу выполненной.
    /// Обрати внимание: здесь ошибки сообщаются ИСКЛЮЧЕНИЯМИ, а превращает их в HTTP-ответы
    /// глобальный обработчик (GlobalExceptionHandler). Сравни с GetById, где 404 возвращается явно.
    /// </summary>
    public static Ok<TaskResponse> Complete(int id, ITaskRepository repository)
    {
        var task = repository.GetById(id) ?? throw new TaskNotFoundException(id);
        if (task.IsCompleted)
        {
            throw new BusinessRuleException($"Задача {id} уже завершена");
        }

        task.IsCompleted = true;
        repository.Update(task);
        return TypedResults.Ok(task.ToResponse());
    }

    /// <summary>
    /// Задача 1 (+ валидация из задачи 2). POST /api/tasks.
    /// <list type="bullet">
    /// <item>Невалидные данные (TaskValidator.Validate вернул ошибки) → 400: TypedResults.ValidationProblem(errors).</item>
    /// <item>Иначе создаём TaskItem: Title — после Trim(); Description — после Trim(), а пустую/из пробелов превращаем в null;
    /// Priority — как пришёл. Сохраняем через repository.Add.</item>
    /// <item>Ответ 201 Created: заголовок Location = "/api/tasks/{id}", тело — TaskResponse созданной задачи.</item>
    /// </list>
    /// </summary>
    public static Results<Created<TaskResponse>, ValidationProblem> Create(CreateTaskRequest request, ITaskRepository repository)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Задача 1 (+ валидация из задачи 2). PUT /api/tasks/{id} — полная замена полей задачи.
    /// <list type="number">
    /// <item>Сначала валидация (как в Create) → 400, если есть ошибки.</item>
    /// <item>Задачи нет → 404.</item>
    /// <item>Обновляем Title/Description (с теми же правилами Trim, что в Create), Priority, IsCompleted;
    /// сохраняем через repository.Update → 204 No Content.</item>
    /// </list>
    /// </summary>
    public static Results<NoContent, NotFound, ValidationProblem> Update(int id, UpdateTaskRequest request, ITaskRepository repository)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Задача 1. DELETE /api/tasks/{id} → 204 No Content, если удалили; 404, если задачи не было.
    /// </summary>
    public static Results<NoContent, NotFound> Delete(int id, ITaskRepository repository)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Задача 6 ⭐. GET /api/tasks?status=active&amp;search=молоко&amp;page=1&amp;pageSize=10.
    /// <list type="number">
    /// <item>page по умолчанию 1, pageSize по умолчанию options.Value.DefaultPageSize.</item>
    /// <item>Проверка через TaskQuery.Validate(query, options.Value.MaxPageSize) → 400 ValidationProblem при ошибках.</item>
    /// <item>Иначе 200 + TaskQuery.Apply(repository.GetAll(), query).</item>
    /// </list>
    /// IOptions&lt;TaskTrackerOptions&gt; приходит из DI — это Options pattern.
    /// </summary>
    public static Results<Ok<PagedResponse<TaskResponse>>, ValidationProblem> GetTasks(
        string? status,
        string? search,
        int? page,
        int? pageSize,
        ITaskRepository repository,
        IOptions<TaskTrackerOptions> options)
    {
        throw new NotImplementedException();
    }
}
