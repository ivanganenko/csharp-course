using System.Diagnostics.CodeAnalysis;

namespace Module09.Tasks;

public class LruCache<TKey, TValue> where TKey : notnull
{
    // Две структуры, каждая отвечает за своё:
    // - Dictionary: ключ → узел списка, поиск за O(1);
    // - LinkedList: порядок «свежести»; перенос узла в начало и удаление с конца — O(1),
    //   потому что у нас уже есть ссылка на узел (в List<T> удаление из середины — O(n)).
    // Голова списка — самый свежий элемент, хвост — кандидат на вытеснение.
    private readonly Dictionary<TKey, LinkedListNode<Entry>> _nodes;
    private readonly LinkedList<Entry> _recency = new();

    public LruCache(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        Capacity = capacity;
        // Размер заранее известен — задаём capacity словаря, чтобы избежать rehash при росте.
        _nodes = new Dictionary<TKey, LinkedListNode<Entry>>(capacity);
    }

    public int Capacity { get; }

    public int Count => _nodes.Count;

    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_nodes.TryGetValue(key, out LinkedListNode<Entry>? node))
        {
            MoveToFront(node);
            value = node.Value.Value;
            return true;
        }

        value = default;
        return false;
    }

    public void Put(TKey key, TValue value)
    {
        if (_nodes.TryGetValue(key, out LinkedListNode<Entry>? existing))
        {
            // Обновление: размер не меняется, поэтому вытеснять никого не нужно.
            existing.Value.Value = value;
            MoveToFront(existing);
            return;
        }

        if (_nodes.Count == Capacity)
        {
            // Хвост списка — least recently used. Ключ храним в узле именно для этого момента:
            // иначе мы не смогли бы удалить запись из словаря за O(1).
            LinkedListNode<Entry> leastRecent = _recency.Last!;
            _recency.RemoveLast();
            _nodes.Remove(leastRecent.Value.Key);
        }

        LinkedListNode<Entry> node = _recency.AddFirst(new Entry(key, value));
        _nodes.Add(key, node);
    }

    // Только словарь — порядок свежести намеренно не трогаем.
    public bool ContainsKey(TKey key) => _nodes.ContainsKey(key);

    public IReadOnlyList<TKey> KeysByRecency()
    {
        var keys = new List<TKey>(_nodes.Count);
        foreach (Entry entry in _recency)
        {
            keys.Add(entry.Key);
        }

        return keys;
    }

    private void MoveToFront(LinkedListNode<Entry> node)
    {
        if (node == _recency.First)
        {
            return;
        }

        // Переиспользуем тот же узел (без новой аллокации) — ссылка в словаре остаётся валидной.
        _recency.Remove(node);
        _recency.AddFirst(node);
    }

    // Класс, а не кортеж: значение нужно менять на месте при обновлении ключа.
    private sealed class Entry
    {
        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }

        public TValue Value { get; set; }
    }
}
