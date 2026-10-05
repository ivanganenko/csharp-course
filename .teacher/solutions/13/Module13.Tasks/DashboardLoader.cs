namespace Module13.Tasks;

public interface IDataSource
{
    Task<string> LoadAsync(string widgetId, CancellationToken cancellationToken);
}

public static class DashboardLoader
{
    public static async Task<IReadOnlyList<string>> LoadAllAsync(
        IDataSource source,
        IReadOnlyList<string> widgetIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(widgetIds);

        // Ключевой момент: сначала ЗАПУСКАЕМ все операции (ToArray материализует Select —
        // без него LINQ был бы ленивым и задачи стартовали бы только внутри WhenAll),
        // и только потом ждём их все вместе. Общее время ≈ время самой долгой загрузки,
        // а не сумма всех.
        // Task.Run не нужен: операции IO-bound, пока ждём сеть, поток свободен.
        var tasks = widgetIds
            .Select(id => source.LoadAsync(id, cancellationToken))
            .ToArray();

        // WhenAll сохраняет порядок результатов как в массиве задач,
        // независимо от того, какая задача завершилась первой.
        // Если какие-то задачи упали, await пробросит первое исключение
        // (все остальные доступны через Task.Exception).
        return await Task.WhenAll(tasks);
    }
}
