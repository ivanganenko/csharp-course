namespace Module08.Tasks;

/// <summary>Сущность с целочисленным идентификатором. Нужна как constraint для репозитория.</summary>
public interface IEntity
{
    int Id { get; }
}

/// <summary>Книга в библиотеке (вымышленные данные).</summary>
public sealed record Book(int Id, string Title, string Author) : IEntity;

/// <summary>Читатель библиотеки (вымышленные данные).</summary>
public sealed record Reader(int Id, string Name) : IEntity;

/// <summary>
/// Задача 08.3 — generic-репозиторий в памяти. Один класс работает и для Book, и для Reader, и для
/// любой другой сущности, реализующей <see cref="IEntity"/>.
/// Constraint <c>class</c> нужен, чтобы GetById мог вернуть null («не найдено»).
/// Тесты: InMemoryRepositoryTests.
/// </summary>
public class InMemoryRepository<T> where T : class, IEntity
{
    /// <summary>Количество сущностей в репозитории.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// Добавляет сущность.
    /// null → <see cref="ArgumentNullException"/>.
    /// Сущность с таким Id уже есть → <see cref="InvalidOperationException"/>.
    /// Поиск по Id должен работать за O(1) — выберите подходящую коллекцию для хранения.
    /// </summary>
    public void Add(T entity)
    {
        throw new NotImplementedException();
    }

    /// <summary>Возвращает сущность по Id или null, если её нет.</summary>
    public T? GetById(int id)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает все сущности, отсортированные по Id по возрастанию.
    /// Возвращаемый список — копия: его изменение (через приведение к List) не должно влиять на репозиторий.
    /// </summary>
    public IReadOnlyList<T> GetAll()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Заменяет сущность с тем же Id на переданную. Возвращает true, если замена произошла,
    /// и false, если сущности с таким Id нет (в этом случае ничего не добавляется).
    /// null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public bool Update(T entity)
    {
        throw new NotImplementedException();
    }

    /// <summary>Удаляет сущность по Id. true — удалена, false — такой не было.</summary>
    public bool Remove(int id)
    {
        throw new NotImplementedException();
    }
}
