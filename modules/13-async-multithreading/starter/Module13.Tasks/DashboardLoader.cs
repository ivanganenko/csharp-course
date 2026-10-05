namespace Module13.Tasks;

/// <summary>
/// Источник данных для виджетов дашборда (в реальности — HTTP API или база данных).
/// Это IO-bound операция: пока ждём ответ, поток не нужен.
/// </summary>
public interface IDataSource
{
    Task<string> LoadAsync(string widgetId, CancellationToken cancellationToken);
}

/// <summary>
/// Задача 13.1 — параллельная загрузка через Task.WhenAll.
/// </summary>
public static class DashboardLoader
{
    /// <summary>
    /// Загружает данные всех виджетов ОДНОВРЕМЕННО (не по очереди!) и возвращает результаты
    /// в том же порядке, что и <paramref name="widgetIds"/>.
    /// - Все вызовы source.LoadAsync должны стартовать до того, как завершится хотя бы один.
    /// - Не используй Task.Run: загрузка — IO-bound, отдельный поток ей не нужен.
    /// - cancellationToken передаётся в каждый вызов LoadAsync.
    /// - Если какая-то загрузка упала — исключение пробрасывается вызывающему.
    /// - Пустой список → пустой результат. source или widgetIds == null → ArgumentNullException.
    /// </summary>
    public static Task<IReadOnlyList<string>> LoadAllAsync(
        IDataSource source,
        IReadOnlyList<string> widgetIds,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
