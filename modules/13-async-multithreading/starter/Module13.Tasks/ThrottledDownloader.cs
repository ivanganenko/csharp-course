namespace Module13.Tasks;

/// <summary>
/// Задача 13.4 — ограничение параллелизма через SemaphoreSlim.
/// Нужно скачать 1000 файлов, но внешний API разрешает не больше N одновременных запросов.
/// </summary>
public static class ThrottledDownloader
{
    /// <summary>
    /// Скачивает все urls через download, но одновременно выполняется НЕ БОЛЬШЕ maxConcurrency вызовов.
    /// Результаты — в том же порядке, что и urls.
    /// - Используй SemaphoreSlim и его асинхронное ожидание (WaitAsync), а не блокирующее Wait.
    /// - Если download бросил исключение — слот семафора всё равно освобождается (остальные загрузки
    ///   продолжаются), а исключение пробрасывается вызывающему после завершения остальных.
    /// - cancellationToken передаётся и в ожидание семафора, и в download.
    /// - maxConcurrency &lt;= 0 → ArgumentOutOfRangeException; urls или download == null → ArgumentNullException.
    /// </summary>
    public static Task<IReadOnlyList<string>> DownloadAllAsync(
        IReadOnlyList<string> urls,
        Func<string, CancellationToken, Task<string>> download,
        int maxConcurrency,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
