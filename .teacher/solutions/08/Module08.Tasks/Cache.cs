using System.Diagnostics.CodeAnalysis;

namespace Module08.Tasks;

public class Cache<TKey, TValue> where TKey : notnull
{
    // notnull на TKey повторяет constraint самого Dictionary<TKey, TValue> — иначе компилятор
    // предупредил бы, что TKey может быть nullable.
    private readonly Dictionary<TKey, TValue> _values = new();

    public int Count => _values.Count;

    // Свойства только для чтения снаружи: статистику меняет только сам кэш (инкапсуляция).
    public int Hits { get; private set; }

    public int Misses { get; private set; }

    public void Set(TKey key, TValue value)
    {
        // Индексатор: «добавить или заменить». Для null-ключа Dictionary сам бросит ArgumentNullException.
        _values[key] = value;
    }

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_values.TryGetValue(key, out value))
        {
            Hits++;
            return true;
        }

        Misses++;
        return false;
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        // Проверяем factory сразу, а не только при промахе — иначе ошибка проявлялась бы «иногда».
        ArgumentNullException.ThrowIfNull(factory);

        if (TryGet(key, out TValue? existing))
        {
            return existing;
        }

        // Переиспользуем TryGet, чтобы статистика считалась в одном месте.
        TValue created = factory(key);
        _values[key] = created;
        return created;
    }

    public bool Remove(TKey key) => _values.Remove(key);
}
