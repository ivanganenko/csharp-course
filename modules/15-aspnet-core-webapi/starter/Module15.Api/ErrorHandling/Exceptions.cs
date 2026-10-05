namespace Module15.Api.ErrorHandling;

/// <summary>Задача не найдена. Глобальный обработчик превращает её в 404.</summary>
public sealed class TaskNotFoundException : Exception
{
    public TaskNotFoundException(int taskId)
        : base($"Задача с id={taskId} не найдена")
    {
        TaskId = taskId;
    }

    public int TaskId { get; }
}

/// <summary>Нарушено бизнес-правило (например, «задача уже завершена»). Обработчик превращает её в 409 Conflict.</summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
