namespace Module11.Tasks;

public class Inventory
{
    private readonly List<StockItem> _items = new();

    public int NormalizeCalls { get; private set; }

    public int Count => _items.Count;

    public void Add(StockItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    public IEnumerable<StockItem> GetLowStockSnapshot(int threshold)
    {
        // БАГ 1 (был: return _items.Where(...)).
        // Where не выполняет фильтрацию, а возвращает «рецепт» запроса поверх живого _items.
        // Каждый перебор заново читал текущее состояние склада: снимок «менялся» после Add,
        // а перебор с одновременным Add падал с InvalidOperationException (Collection was modified).
        // ToList выполняет запрос сразу (immediate execution) и фиксирует результат.
        return _items.Where(item => item.Quantity < threshold).ToList();
    }

    public ImportReport ImportSkus(IEnumerable<string> rawSkus)
    {
        ArgumentNullException.ThrowIfNull(rawSkus);

        // БАГ 2 (был: Select без материализации, затем Count() и Distinct().ToList()).
        // Каждый терминальный оператор заново перечислял ленивый Select: Normalize вызывался 2n раз,
        // а одноразовый источник (поток, DataReader) падал на втором перечислении.
        // Материализуем ровно один раз и дальше работаем со списком.
        List<string> normalized = rawSkus.Select(Normalize).ToList();

        // Уникальные в порядке первого появления: HashSet.Add + List.
        // (Distinct на практике сохраняет порядок, но документация этого не гарантирует.)
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var unique = new List<string>();
        foreach (string sku in normalized)
        {
            if (seen.Add(sku))
            {
                unique.Add(sku);
            }
        }

        return new ImportReport(normalized.Count, unique);
    }

    public int RemoveEmpty()
    {
        // БАГ 3 (был: foreach по _items.Where(...) с _items.Remove внутри).
        // Where лениво перебирает _items, а Remove меняет его прямо во время перебора —
        // enumerator List<T> обнаруживает изменение версии и бросает InvalidOperationException.
        // RemoveAll удаляет за один проход O(n) и возвращает количество удалённых.
        return _items.RemoveAll(item => item.Quantity == 0);
    }

    private string Normalize(string rawSku)
    {
        NormalizeCalls++;
        return rawSku.Trim().ToUpperInvariant();
    }
}
