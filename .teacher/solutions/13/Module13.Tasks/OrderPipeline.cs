using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Module13.Tasks;

public static class OrderPipeline
{
    public static async Task<IReadOnlyList<string>> RunAsync(
        IEnumerable<int> orderIds,
        Func<int, CancellationToken, Task<string>> process,
        int consumerCount,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        ArgumentNullException.ThrowIfNull(process);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(consumerCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        // Bounded канал + FullMode.Wait = backpressure: если consumers не успевают,
        // producer притормаживает и память не растёт бесконечно (unbounded очередь — скрытая утечка).
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = consumerCount == 1,
        });

        // Общий токен конвейера: сработает при отмене вызывающим ИЛИ при ошибке любого consumer.
        using var pipelineCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = pipelineCts.Token;

        // ConcurrentQueue — несколько consumers добавляют результаты одновременно;
        // обычный List<T> в такой ситуации может потерять элементы или сломать внутреннее состояние.
        var results = new ConcurrentQueue<string>();

        var consumers = Enumerable.Range(0, consumerCount)
            .Select(_ => ConsumeAsync())
            .ToArray();
        var producer = ProduceAsync();

        // Ждём ВСЕХ. Отменённые задачи не добавляют исключений в WhenAll,
        // поэтому наружу выйдет настоящая причина остановки (ошибка consumer),
        // а при отмене вызывающим — OperationCanceledException.
        var all = consumers.Append(producer).ToArray();
        await Task.WhenAll(all);

        return results.ToArray();

        async Task ProduceAsync()
        {
            try
            {
                foreach (var id in orderIds)
                {
                    // Если канал полон — асинхронно ждём место (поток не блокируется).
                    await channel.Writer.WriteAsync(id, token);
                }
            }
            finally
            {
                // Complete обязателен: иначе consumers в ReadAllAsync будут ждать новых данных вечно.
                // TryComplete — потому что повторное завершение канала через Complete бросает исключение.
                channel.Writer.TryComplete();
            }
        }

        async Task ConsumeAsync()
        {
            try
            {
                await foreach (var id in channel.Reader.ReadAllAsync(token))
                {
                    results.Enqueue(await process(id, token));
                }
            }
            catch
            {
                // Ошибка одного consumer останавливает весь конвейер: producer перестаёт ждать
                // места в канале, остальные consumers перестают читать. Без этого producer
                // мог бы навсегда зависнуть на WriteAsync в заполненный канал.
                pipelineCts.Cancel();
                throw;
            }
        }
    }
}
