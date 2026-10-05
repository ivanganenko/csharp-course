using Module15.Api.Models;

namespace Module15.Api.Repositories;

/// <summary>
/// Готовая реализация (её писать не нужно). Регистрируется как Singleton:
/// данные должны жить между запросами. Раз объект один на всё приложение,
/// а запросы обрабатываются параллельно, доступ к словарю защищён lock (модуль 13).
/// </summary>
public sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly Dictionary<int, TaskItem> _items = new();
    private readonly object _sync = new();
    private int _nextId = 1;

    public IReadOnlyList<TaskItem> GetAll()
    {
        lock (_sync)
        {
            return _items.Values.OrderBy(item => item.Id).Select(item => item.Clone()).ToList();
        }
    }

    public TaskItem? GetById(int id)
    {
        lock (_sync)
        {
            return _items.TryGetValue(id, out var item) ? item.Clone() : null;
        }
    }

    public TaskItem Add(TaskItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            var stored = item.Clone();
            stored.Id = _nextId++;
            _items[stored.Id] = stored;
            return stored.Clone();
        }
    }

    public bool Update(TaskItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            if (!_items.ContainsKey(item.Id))
            {
                return false;
            }

            _items[item.Id] = item.Clone();
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_sync)
        {
            return _items.Remove(id);
        }
    }
}
