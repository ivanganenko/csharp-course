using System.Diagnostics.CodeAnalysis;

namespace Module09.Tasks;

/// <summary>
/// Задача 09.7 ⭐ — LRU-кэш (Least Recently Used): при переполнении вытесняется элемент,
/// к которому дольше всего не обращались. Классическая live coding задача.
/// Все операции (TryGet, Put, ContainsKey) — O(1).
/// Подсказка: <see cref="Dictionary{TKey,TValue}"/> + <see cref="LinkedList{T}"/>.
/// Тесты: LruCacheTests.
/// </summary>
public class LruCache<TKey, TValue> where TKey : notnull
{
    /// <summary>capacity &lt;= 0 → <see cref="ArgumentOutOfRangeException"/>.</summary>
    public LruCache(int capacity)
    {
        throw new NotImplementedException();
    }

    /// <summary>Максимальное число элементов.</summary>
    public int Capacity => throw new NotImplementedException();

    /// <summary>Текущее число элементов.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// Если ключ есть — возвращает true, значение и делает элемент «самым свежим» (most recently used).
    /// Если нет — false и default.
    /// </summary>
    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Добавляет или обновляет значение; элемент становится «самым свежим».
    /// Если добавление НОВОГО ключа превышает Capacity — сначала вытесняется самый «старый» элемент.
    /// Обновление существующего ключа ничего не вытесняет.
    /// </summary>
    public void Put(TKey key, TValue value)
    {
        throw new NotImplementedException();
    }

    /// <summary>Проверяет наличие ключа, НЕ меняя порядок «свежести».</summary>
    public bool ContainsKey(TKey key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ключи от самого свежего к самому старому (для отладки и тестов). Сложность O(n) допустима.
    /// </summary>
    public IReadOnlyList<TKey> KeysByRecency()
    {
        throw new NotImplementedException();
    }
}
