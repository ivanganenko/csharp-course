namespace Module13.Tasks;

public static class TimeoutHelper
{
    public static async Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be positive.");
        }

        // Связанный (linked) токен отменится, если отменят внешний токен ИЛИ если мы сами
        // решим отменить операцию по таймауту. Операция видит один токен и не знает причин.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var task = operation(timeoutCts.Token);

        try
        {
            // WaitAsync (.NET 6+) ждёт задачу не дольше timeout и бросает TimeoutException.
            // Почему не только CancelAfter: операция может игнорировать токен — тогда ожидание
            // по токену длилось бы вечно. WaitAsync перестаёт ЖДАТЬ в любом случае.
            // Внешний токен передаём сюда же: отмена вызывающим → OperationCanceledException.
            return await task.WaitAsync(timeout, cancellationToken);
        }
        catch (TimeoutException)
        {
            // Мы перестали ждать, но операция может продолжать работать в фоне.
            // Отменяем её токен, чтобы она освободила ресурсы (соединения, файлы).
            await timeoutCts.CancelAsync();
            throw;
        }
    }
}
