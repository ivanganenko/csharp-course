using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Module15.Api.Configuration;
using Module15.Api.ErrorHandling;
using Module15.Api.Models;
using Module15.Api.Querying;
using Module15.Api.Repositories;
using Module15.Api.Validation;

namespace Module15.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapGet("/", GetTasks);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);
        group.MapPost("/{id:int}/complete", Complete);

        return app;
    }

    public static Results<Ok<TaskResponse>, NotFound> GetById(int id, ITaskRepository repository)
    {
        var task = repository.GetById(id);
        if (task is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(task.ToResponse());
    }

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

    public static Results<Created<TaskResponse>, ValidationProblem> Create(CreateTaskRequest request, ITaskRepository repository)
    {
        // Валидация — первой: не тратим ресурсы и не трогаем хранилище для заведомо плохих данных.
        var errors = TaskValidator.Validate(request.Title, request.Description, request.Priority);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var created = repository.Add(new TaskItem
        {
            // "!" безопасен: валидатор уже гарантировал, что Title не null и не пустой.
            Title = request.Title!.Trim(),
            Description = NormalizeDescription(request.Description),
            Priority = request.Priority,
        });

        // 201 Created + Location: REST-клиент сразу знает адрес нового ресурса.
        return TypedResults.Created($"/api/tasks/{created.Id}", created.ToResponse());
    }

    public static Results<NoContent, NotFound, ValidationProblem> Update(int id, UpdateTaskRequest request, ITaskRepository repository)
    {
        var errors = TaskValidator.Validate(request.Title, request.Description, request.Priority);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var existing = repository.GetById(id);
        if (existing is null)
        {
            return TypedResults.NotFound();
        }

        // PUT = полная замена изменяемых полей (в отличие от PATCH — частичного обновления).
        existing.Title = request.Title!.Trim();
        existing.Description = NormalizeDescription(request.Description);
        existing.Priority = request.Priority;
        existing.IsCompleted = request.IsCompleted;

        // Между GetById и Update задачу могли удалить параллельным запросом — честно отвечаем 404.
        if (!repository.Update(existing))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public static Results<NoContent, NotFound> Delete(int id, ITaskRepository repository)
    {
        // Один вызов Delete вместо «проверить, потом удалить»: нет гонки между проверкой и действием.
        if (!repository.Delete(id))
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public static Results<Ok<PagedResponse<TaskResponse>>, ValidationProblem> GetTasks(
        string? status,
        string? search,
        int? page,
        int? pageSize,
        ITaskRepository repository,
        IOptions<TaskTrackerOptions> options)
    {
        var settings = options.Value;
        var query = new TaskQueryParameters(status, search, page ?? 1, pageSize ?? settings.DefaultPageSize);

        var errors = TaskQuery.Validate(query, settings.MaxPageSize);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        // Эндпоинт «тонкий»: привязал параметры → вызвал логику → вернул результат.
        return TypedResults.Ok(TaskQuery.Apply(repository.GetAll(), query));
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
