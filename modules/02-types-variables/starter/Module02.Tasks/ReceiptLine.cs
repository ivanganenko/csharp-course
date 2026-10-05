namespace Module02.Tasks;

/// <summary>
/// Строка чека: товар, цена за единицу, количество. Этот тип уже готов — менять его не нужно.
/// Это record (запись) — компактное объявление типа-данных; подробно разберём в модуле 06.
/// </summary>
public sealed record ReceiptLine(string Name, decimal Price, int Quantity);
