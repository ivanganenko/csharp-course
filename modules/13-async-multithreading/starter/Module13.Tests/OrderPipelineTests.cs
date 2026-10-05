using Module13.Tasks;

namespace Module13.Tests;

public class OrderPipelineTests
{
    private sealed class ProcessingProbe(int? failingId = null)
    {
        private int _current;
        private int _max;

        public int Max => Volatile.Read(ref _max);

        public async Task<string> ProcessAsync(int id, CancellationToken cancellationToken)
        {
            var now = Interlocked.Increment(ref _current);
            int seen;
            while (now > (seen = Volatile.Read(ref _max)) &&
                   Interlocked.CompareExchange(ref _max, now, seen) != seen)
            {
            }

            try
            {
                await Task.Delay(2, cancellationToken);
                if (id == failingId)
                {
                    throw new InvalidOperationException($"Заказ {id}: ошибка оплаты");
                }

                return $"order-{id}";
            }
            finally
            {
                Interlocked.Decrement(ref _current);
            }
        }
    }

    private static string[] Expected(int count) =>
        Enumerable.Range(1, count).Select(i => $"order-{i}").Order(StringComparer.Ordinal).ToArray();

    [Fact]
    public async Task RunAsync_ProcessesEveryOrderExactlyOnce()
    {
        var probe = new ProcessingProbe();

        var result = await AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync(Enumerable.Range(1, 50), probe.ProcessAsync, consumerCount: 3, capacity: 5));

        Assert.Equal(Expected(50), result.Order(StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public async Task RunAsync_SingleConsumer_PreservesOrder()
    {
        var probe = new ProcessingProbe();

        var result = await AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync([3, 1, 2], probe.ProcessAsync, consumerCount: 1, capacity: 1));

        Assert.Equal(new[] { "order-3", "order-1", "order-2" }, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public async Task RunAsync_NeverRunsMoreThanConsumerCountAtOnce(int consumers)
    {
        var probe = new ProcessingProbe();

        await AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync(Enumerable.Range(1, 30), probe.ProcessAsync, consumers, capacity: 2));

        Assert.InRange(probe.Max, 1, consumers);
    }

    [Fact]
    public async Task RunAsync_EmptyInput_ReturnsEmpty()
    {
        var probe = new ProcessingProbe();

        var result = await AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync([], probe.ProcessAsync, consumerCount: 2, capacity: 2));

        Assert.Empty(result);
    }

    [Fact]
    public async Task RunAsync_ProcessThrows_PipelineStopsAndExceptionPropagates()
    {
        // capacity = 1 и 100 заказов: если после ошибки producer не остановить,
        // он навсегда зависнет на записи в полный канал.
        var probe = new ProcessingProbe(failingId: 7);

        await Assert.ThrowsAsync<InvalidOperationException>(() => AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync(Enumerable.Range(1, 100), probe.ProcessAsync, consumerCount: 2, capacity: 1)));
    }

    [Fact]
    public async Task RunAsync_CallerCancelled_ThrowsOperationCanceled()
    {
        var probe = new ProcessingProbe();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AsyncGuard.WithTimeout(
            OrderPipeline.RunAsync(Enumerable.Range(1, 10), probe.ProcessAsync, 2, 2, cts.Token)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 5)]
    public async Task RunAsync_InvalidCounts_ThrowArgumentOutOfRange(int consumers, int capacity)
    {
        var probe = new ProcessingProbe();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => OrderPipeline.RunAsync([1], probe.ProcessAsync, consumers, capacity));
    }

    [Fact]
    public async Task RunAsync_NullProcess_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OrderPipeline.RunAsync([1], null!, 1, 1));
    }
}
