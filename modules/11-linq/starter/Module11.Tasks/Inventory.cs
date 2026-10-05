namespace Module11.Tasks;

/// <summary>
/// Задача 11.4 — «найди баг с deferred execution».
/// ВНИМАНИЕ: этот класс уже РЕАЛИЗОВАН, но в трёх методах есть ошибки, связанные с отложенным
/// выполнением LINQ. Тесты InventoryTests падают. Найдите и исправьте ошибки, не меняя сигнатуры
/// и не трогая тесты. Каждое исправление прокомментируйте: в чём была ошибка и почему.
/// </summary>
public class Inventory
{
    private readonly List<StockItem> _items = new();

    /// <summary>Сколько раз вызывалась нормализация артикула (для диагностики лишней работы).</summary>
    public int NormalizeCalls { get; private set; }

    public int Count => _items.Count;

    public void Add(StockItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>
    /// Возвращает СНИМОК позиций с количеством меньше <paramref name="threshold"/> на момент вызова.
    /// Последующие изменения склада не должны менять уже полученный снимок.
    /// </summary>
    public IEnumerable<StockItem> GetLowStockSnapshot(int threshold)
    {
        return _items.Where(item => item.Quantity < threshold);
    }

    /// <summary>
    /// Нормализует сырые артикулы (Trim + верхний регистр) и возвращает отчёт: сколько строк пришло
    /// и уникальные артикулы в порядке первого появления.
    /// Каждый артикул нормализуется ровно ОДИН раз; источник перечисляется ровно ОДИН раз
    /// (он может быть потоком из файла или сети, который нельзя прочитать повторно).
    /// </summary>
    public ImportReport ImportSkus(IEnumerable<string> rawSkus)
    {
        ArgumentNullException.ThrowIfNull(rawSkus);

        IEnumerable<string> normalized = rawSkus.Select(Normalize);
        int total = normalized.Count();
        List<string> unique = normalized.Distinct().ToList();

        return new ImportReport(total, unique);
    }

    /// <summary>
    /// Удаляет все позиции с нулевым количеством и возвращает, сколько удалено.
    /// </summary>
    public int RemoveEmpty()
    {
        int removed = 0;
        foreach (StockItem item in _items.Where(i => i.Quantity == 0))
        {
            _items.Remove(item);
            removed++;
        }

        return removed;
    }

    private string Normalize(string rawSku)
    {
        NormalizeCalls++;
        return rawSku.Trim().ToUpperInvariant();
    }
}
