namespace Module13.Tasks;

public static class OrderImporter
{
    public static async Task<int> ImportAsync(
        IEnumerable<string> orderLines,
        Func<string, CancellationToken, Task> importOne,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(orderLines);
        ArgumentNullException.ThrowIfNull(importOne);

        var count = 0;
        foreach (var line in orderLines)
        {
            // Отмена в .NET кооперативная: никто не «убивает» наш код снаружи,
            // мы сами проверяем токен в безопасных точках — между заказами,
            // чтобы не оставить заказ импортированным наполовину.
            cancellationToken.ThrowIfCancellationRequested();

            // Токен передаём дальше: вложенная операция (HTTP, БД) тоже сможет прерваться.
            await importOne(line, cancellationToken);
            count++;
        }

        return count;
    }
}
