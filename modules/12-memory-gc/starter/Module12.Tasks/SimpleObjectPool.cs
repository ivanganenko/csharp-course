namespace Module12.Tasks;

/// <summary>
/// Задача 12.6 ⭐ — простой потокобезопасный пул объектов.
/// Идея: дорогие в создании объекты (StringBuilder, буферы, парсеры) не создавать заново,
/// а брать из пула и возвращать обратно — меньше аллокаций и работы для GC.
/// </summary>
public sealed class SimpleObjectPool<T> where T : class
{
    /// <summary>
    /// factory — как создать новый объект; maxRetained — сколько объектов пул максимум хранит;
    /// reset — (необязательно) как «очистить» объект при возврате в пул.
    /// factory == null → ArgumentNullException; maxRetained &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public SimpleObjectPool(Func<T> factory, int maxRetained, Action<T>? reset = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>Сколько объектов сейчас лежит в пуле и готово к выдаче.</summary>
    public int AvailableCount => throw new NotImplementedException();

    /// <summary>Сколько раз пул вызвал factory (создал новый объект).</summary>
    public int CreatedCount => throw new NotImplementedException();

    /// <summary>
    /// Выдаёт объект: последний возвращённый (LIFO), если пул не пуст; иначе создаёт новый через factory.
    /// Потокобезопасно: один объект никогда не выдаётся двум вызывающим одновременно.
    /// </summary>
    public T Rent()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает объект в пул: сначала вызывает reset (если задан), затем кладёт в пул.
    /// Если в пуле уже maxRetained объектов — объект просто отбрасывается (его соберёт GC).
    /// item == null → ArgumentNullException.
    /// </summary>
    public void Return(T item)
    {
        throw new NotImplementedException();
    }
}
