namespace Module06.Tasks;

/// <summary>
/// Позиция на складе. ИЗМЕНЯЕМАЯ структура (mutable struct) — специально, чтобы увидеть ловушку.
/// Уже готова — менять не нужно.
/// </summary>
public struct StockEntry
{
    public StockEntry(string sku, int quantity)
    {
        Sku = sku;
        Quantity = quantity;
    }

    public string Sku { get; }

    public int Quantity { get; set; }

    /// <summary>Изменяет Quantity ЭТОЙ копии структуры.</summary>
    public void Add(int amount) => Quantity += amount;
}

/// <summary>
/// Задача 06.4 — склад, хранящий позиции в List&lt;StockEntry&gt;.
/// Ловушка: _entries[i] возвращает КОПИЮ структуры. Код _entries[i].Add(5) компилируется,
/// но меняет копию, а элемент списка остаётся прежним. То же с переменной foreach.
/// Правильно: взять копию → изменить → записать обратно _entries[i] = copy.
/// SKU сравниваются с учётом регистра (ordinal).
/// </summary>
public sealed class Warehouse
{
    /// <summary>Позиции склада в порядке первого поступления. Только для чтения.</summary>
    public IReadOnlyList<StockEntry> Entries => throw new NotImplementedException();

    /// <summary>
    /// Поступление товара: если SKU уже есть — увеличить Quantity, иначе добавить новую позицию в конец.
    /// sku null/пусто/пробелы → ArgumentException (или наследник); quantity &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public void Receive(string sku, int quantity)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Отгрузка: если SKU есть и остатка хватает — уменьшить Quantity и вернуть true
    /// (позиция с нулевым остатком остаётся в списке). Иначе — false, ничего не меняется.
    /// quantity &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public bool TryShip(string sku, int quantity)
    {
        throw new NotImplementedException();
    }

    /// <summary>Остаток по SKU; 0, если такого SKU нет.</summary>
    public int GetQuantity(string sku)
    {
        throw new NotImplementedException();
    }
}
