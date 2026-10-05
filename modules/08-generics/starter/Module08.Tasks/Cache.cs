using System.Diagnostics.CodeAnalysis;

namespace Module08.Tasks;

/// <summary>
/// Задача 08.4 — generic-кэш с двумя параметрами типа и статистикой попаданий.
/// Пример использования: кэш курсов валют <c>Cache&lt;string, decimal&gt;</c> или
/// кэш профилей <c>Cache&lt;int, Reader&gt;</c>.
/// Constraint <c>notnull</c>: ключ словаря не может быть null.
/// Тесты: CacheTests.
/// </summary>
public class Cache<TKey, TValue> where TKey : notnull
{
    /// <summary>Количество закэшированных значений.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>Сколько раз значение было найдено в кэше (TryGet → true или GetOrAdd без вызова factory).</summary>
    public int Hits => throw new NotImplementedException();

    /// <summary>Сколько раз значения в кэше не оказалось (TryGet → false или GetOrAdd с вызовом factory).</summary>
    public int Misses => throw new NotImplementedException();

    /// <summary>Добавляет или перезаписывает значение по ключу. Статистику не меняет.</summary>
    public void Set(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Try-pattern: true и значение — если ключ есть (Hits + 1);
    /// false и default(TValue) — если нет (Misses + 1).
    /// </summary>
    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает значение из кэша (Hits + 1), а если его нет — вызывает <paramref name="factory"/>
    /// ровно один раз, сохраняет результат и возвращает его (Misses + 1).
    /// <c>Func&lt;TKey, TValue&gt;</c> — это делегат «функция, принимающая TKey и возвращающая TValue»
    /// (подробно — в модуле 10). Вызывается как обычный метод: <c>factory(key)</c>.
    /// null вместо factory → <see cref="ArgumentNullException"/>.
    /// </summary>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        throw new NotImplementedException();
    }

    /// <summary>Удаляет значение по ключу. true — удалено, false — ключа не было.</summary>
    public bool Remove(TKey key)
    {
        throw new NotImplementedException();
    }
}
