namespace Module13.Tasks;

public static class ThrottledDownloader
{
    public static async Task<IReadOnlyList<string>> DownloadAllAsync(
        IReadOnlyList<string> urls,
        Func<string, CancellationToken, Task<string>> download,
        int maxConcurrency,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(urls);
        ArgumentNullException.ThrowIfNull(download);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxConcurrency);

        // SemaphoreSlim — «турникет» на N мест. В отличие от lock, его можно ждать асинхронно
        // (WaitAsync), и он не привязан к потоку: Release можно вызвать из другого потока
        // после await — lock так не умеет (и не разрешает await внутри).
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        var tasks = urls.Select(async url =>
        {
            // WaitAsync не блокирует поток: пока мест нет, поток пула свободен для другой работы.
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await download(url, cancellationToken);
            }
            finally
            {
                // finally — чтобы слот освободился даже при исключении.
                // Иначе после нескольких ошибок все слоты «утекут» и загрузка зависнет.
                semaphore.Release();
            }
        }).ToArray();

        // Все задачи созданы сразу, но реально работают не больше maxConcurrency.
        // semaphore освобождается (using) только после того, как WhenAll дождался всех задач.
        return await Task.WhenAll(tasks);
    }
}
