namespace Module02.Tasks;

/// <summary>
/// Задача 02.5 — построение текстового отчёта (чека) через StringBuilder.
/// Тесты: dotnet test --filter "FullyQualifiedName~ReceiptBuilderTests"
/// </summary>
public static class ReceiptBuilder
{
    /// <summary>
    /// Строит текст чека. Формат (каждая строка, включая последнюю, заканчивается символом '\n'):
    /// <code>
    /// Receipt #42 (2026-03-15)
    /// Tea x2 = 5.00
    /// Coffee x1 = 3.50
    /// Total: 8.50
    /// </code>
    /// <list type="bullet">
    /// <item>Дата — в формате yyyy-MM-dd.</item>
    /// <item>Сумма строки = Price * Quantity; все суммы — ровно 2 знака после ТОЧКИ (формат "0.00", InvariantCulture).</item>
    /// <item>Пустой массив строк → только заголовок и "Total: 0.00".</item>
    /// <item>Используйте '\n', а не AppendLine/Environment.NewLine: результат не должен зависеть от ОС.</item>
    /// <item>lines == null → ArgumentNullException. Строка с Quantity ≤ 0 или Price &lt; 0 → ArgumentException.</item>
    /// </list>
    /// </summary>
    public static string BuildReceipt(int number, DateOnly date, ReceiptLine[] lines)
    {
        throw new NotImplementedException();
    }
}
