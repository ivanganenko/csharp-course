namespace Module15.Api.Models;

/// <summary>
/// Доменная сущность «задача» в трекере. Хранится в репозитории.
/// Наружу (в HTTP-ответах) не отдаётся напрямую — для этого есть <see cref="TaskResponse"/>.
/// Почему так: внутреннюю модель можно менять (добавить поля, переименовать),
/// не ломая контракт API, на который завязаны клиенты.
/// </summary>
public sealed class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Приоритет от 1 (низкий) до 5 (высокий).</summary>
    public int Priority { get; set; } = 3;

    public bool IsCompleted { get; set; }

    /// <summary>
    /// Поверхностная копия. Репозиторий хранит и отдаёт копии, чтобы внешний код
    /// не мог изменить «базу» в обход метода Update (как с настоящей БД).
    /// </summary>
    public TaskItem Clone() => (TaskItem)MemberwiseClone();
}
