namespace Module11.Tasks;

// Модели склада для задачи 11.4. Готовы — менять не нужно.

/// <summary>Позиция на складе: артикул (SKU), категория, количество.</summary>
public sealed record StockItem(string Sku, string Category, int Quantity);

/// <summary>Результат импорта артикулов: сколько строк обработано и уникальные нормализованные артикулы.</summary>
public sealed record ImportReport(int TotalCount, IReadOnlyList<string> UniqueSkus);
