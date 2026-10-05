using Module15.Api.Models;

namespace Module15.Api.Repositories;

/// <summary>
/// Хранилище задач. Эндпоинты зависят от интерфейса, а не от конкретного класса:
/// завтра InMemory заменится на EF Core (модуль 16) — эндпоинты не изменятся,
/// а в тестах можно подставить свою реализацию.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Все задачи, отсортированные по Id.</summary>
    IReadOnlyList<TaskItem> GetAll();

    /// <summary>Задача по Id или null, если её нет.</summary>
    TaskItem? GetById(int id);

    /// <summary>Сохраняет новую задачу, присваивает ей новый Id (1, 2, 3, …) и возвращает сохранённую копию.</summary>
    TaskItem Add(TaskItem item);

    /// <summary>Заменяет задачу с item.Id. Возвращает false, если такой задачи нет.</summary>
    bool Update(TaskItem item);

    /// <summary>Удаляет задачу. Возвращает false, если такой задачи нет.</summary>
    bool Delete(int id);
}
