namespace Module13.Tasks;

/// <summary>
/// Задача 13.7 ⭐ — producer/consumer через System.Threading.Channels.
/// Один producer кладёт номера заказов в ограниченный (bounded) канал,
/// несколько consumers параллельно забирают и обрабатывают их.
/// </summary>
public static class OrderPipeline
{
    /// <summary>
    /// - Создай bounded Channel&lt;int&gt; ёмкостью capacity (producer ждёт, если канал полон).
    /// - Producer записывает все orderIds и завершает канал (Writer.Complete/TryComplete).
    /// - consumerCount consumers читают канал и вызывают process(id, token); результаты собираются
    ///   (порядок любой; при consumerCount == 1 — порядок исходный).
    /// - Одновременно выполняется не больше consumerCount вызовов process.
    /// - Если process бросил исключение — весь конвейер останавливается (никто не зависает),
    ///   и это исключение пробрасывается вызывающему.
    /// - Вызывающий отменил токен → OperationCanceledException.
    /// - consumerCount &lt;= 0 или capacity &lt;= 0 → ArgumentOutOfRangeException;
    ///   orderIds или process == null → ArgumentNullException.
    /// </summary>
    public static Task<IReadOnlyList<string>> RunAsync(
        IEnumerable<int> orderIds,
        Func<int, CancellationToken, Task<string>> process,
        int consumerCount,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
