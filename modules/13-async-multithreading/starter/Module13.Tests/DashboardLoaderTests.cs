using Module13.Tasks;

namespace Module13.Tests;

public class DashboardLoaderTests
{
    // Фейковый источник с «воротами»: каждый LoadAsync ждёт, пока тест не откроет ворота.
    // Так мы ДЕТЕРМИНИРОВАННО проверяем, что все загрузки стартовали одновременно,
    // без замеров времени.
    private sealed class GatedDataSource(string? failingId = null) : IDataSource
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _started;

        public int Started => Volatile.Read(ref _started);

        public List<CancellationToken> ReceivedTokens { get; } = [];

        public void Open() => _gate.TrySetResult();

        public async Task<string> LoadAsync(string widgetId, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _started);
            lock (ReceivedTokens)
            {
                ReceivedTokens.Add(cancellationToken);
            }

            await _gate.Task;

            if (widgetId == failingId)
            {
                throw new InvalidOperationException($"Виджет {widgetId} недоступен");
            }

            return $"data:{widgetId}";
        }
    }

    [Fact]
    public async Task LoadAllAsync_StartsAllLoadsBeforeAnyCompletes()
    {
        var source = new GatedDataSource();

        var task = DashboardLoader.LoadAllAsync(source, ["sales", "users", "orders"]);

        // Ворота ещё закрыты: последовательная реализация успела бы начать только первую загрузку.
        Assert.Equal(3, source.Started);

        source.Open();
        await AsyncGuard.WithTimeout(task);
    }

    [Fact]
    public async Task LoadAllAsync_ReturnsResultsInOriginalOrder()
    {
        var source = new GatedDataSource();
        source.Open();

        var result = await AsyncGuard.WithTimeout(
            DashboardLoader.LoadAllAsync(source, ["sales", "users", "orders"]));

        Assert.Equal(new[] { "data:sales", "data:users", "data:orders" }, result);
    }

    [Fact]
    public async Task LoadAllAsync_EmptyList_ReturnsEmpty()
    {
        var source = new GatedDataSource();

        var result = await AsyncGuard.WithTimeout(DashboardLoader.LoadAllAsync(source, []));

        Assert.Empty(result);
        Assert.Equal(0, source.Started);
    }

    [Fact]
    public async Task LoadAllAsync_OneLoadFails_ExceptionPropagates()
    {
        var source = new GatedDataSource(failingId: "users");
        source.Open();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => AsyncGuard.WithTimeout(DashboardLoader.LoadAllAsync(source, ["sales", "users", "orders"])));
    }

    [Fact]
    public async Task LoadAllAsync_PassesCancellationTokenToEveryLoad()
    {
        var source = new GatedDataSource();
        source.Open();
        using var cts = new CancellationTokenSource();

        await AsyncGuard.WithTimeout(DashboardLoader.LoadAllAsync(source, ["a", "b"], cts.Token));

        Assert.Equal(2, source.ReceivedTokens.Count);
        Assert.All(source.ReceivedTokens, token => Assert.Equal(cts.Token, token));
    }

    [Fact]
    public async Task LoadAllAsync_NullSource_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => DashboardLoader.LoadAllAsync(null!, ["a"]));
    }
}
