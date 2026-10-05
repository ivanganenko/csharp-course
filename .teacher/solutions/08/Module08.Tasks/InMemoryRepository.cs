namespace Module08.Tasks;

public interface IEntity
{
    int Id { get; }
}

public sealed record Book(int Id, string Title, string Author) : IEntity;

public sealed record Reader(int Id, string Name) : IEntity;

public class InMemoryRepository<T> where T : class, IEntity
{
    // Dictionary по Id: поиск, добавление и удаление за O(1) в среднем.
    // List<T> потребовал бы линейного поиска O(n) на каждый GetById.
    private readonly Dictionary<int, T> _items = new();

    public int Count => _items.Count;

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Constraint IEntity даёт доступ к entity.Id — без него T был бы «просто object».
        // TryAdd вместо ContainsKey + Add: один поиск в хэш-таблице вместо двух.
        if (!_items.TryAdd(entity.Id, entity))
        {
            throw new InvalidOperationException($"Entity with Id {entity.Id} already exists.");
        }
    }

    // Constraint class позволяет вернуть null как «не найдено» и писать T? без сюрпризов:
    // для value type T? означал бы Nullable<T> — совсем другой тип.
    public T? GetById(int id) => _items.GetValueOrDefault(id);

    public IReadOnlyList<T> GetAll()
    {
        // Порядок перечисления Dictionary не гарантирован — поэтому явно сортируем.
        // ToList создаёт копию: вызывающий код не сломает внутреннее состояние репозитория.
        return _items.Values.OrderBy(entity => entity.Id).ToList();
    }

    public bool Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!_items.ContainsKey(entity.Id))
        {
            return false;
        }

        _items[entity.Id] = entity;
        return true;
    }

    public bool Remove(int id) => _items.Remove(id);
}
