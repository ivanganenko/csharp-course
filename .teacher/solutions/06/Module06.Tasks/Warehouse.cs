namespace Module06.Tasks;

public struct StockEntry
{
    public StockEntry(string sku, int quantity)
    {
        Sku = sku;
        Quantity = quantity;
    }

    public string Sku { get; }

    public int Quantity { get; set; }

    public void Add(int amount) => Quantity += amount;
}

public sealed class Warehouse
{
    private readonly List<StockEntry> _entries = new();

    // AsReadOnly: индексатор возвращает копии структур, а Add/Remove недоступны.
    public IReadOnlyList<StockEntry> Entries => _entries.AsReadOnly();

    public void Receive(string sku, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        int index = IndexOf(sku);
        if (index < 0)
        {
            _entries.Add(new StockEntry(sku, quantity));
            return;
        }

        // Индексатор List<T> возвращает КОПИЮ структуры. _entries[index].Add(quantity)
        // скомпилируется, но изменит временную копию — список останется прежним.
        // Поэтому: копия → изменение → запись обратно.
        var entry = _entries[index];
        entry.Add(quantity);
        _entries[index] = entry;
    }

    public bool TryShip(string sku, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        int index = IndexOf(sku);
        if (index < 0)
        {
            return false;
        }

        var entry = _entries[index];
        if (entry.Quantity < quantity)
        {
            return false;
        }

        entry.Quantity -= quantity;
        _entries[index] = entry;
        return true;
    }

    public int GetQuantity(string sku)
    {
        int index = IndexOf(sku);
        return index < 0 ? 0 : _entries[index].Quantity;
    }

    // Правильное лечение на будущее — сделать StockEntry readonly struct/record struct и заменять
    // элементы целиком (_entries[i] = entry with { Quantity = ... }): тогда ловушка просто не скомпилируется.
    private int IndexOf(string sku) =>
        _entries.FindIndex(e => string.Equals(e.Sku, sku, StringComparison.Ordinal));
}
