using Module13.Tasks;

namespace Module13.Tests;

public class ThrottledDownloaderTests
{
    // Фейковая загрузка, которая запоминает максимальное число одновременных вызовов.
    // Проверяем только ВЕРХНЮЮ границу (max <= limit) — она верна при любом расписании потоков.
    private sealed class ConcurrencyProbe(string? failingUrl = null)
    {
        private int _current;
        private int _max;
        private int _calls;

        public int Max => Volatile.Read(ref _max);

        public int Calls => Volatile.Read(ref _calls);

        public async Task<string> DownloadAsync(string url, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _calls);
            var now = Interlocked.Increment(ref _current);
            UpdateMax(now);
            try
            {
                await Task.Delay(5, cancellationToken);
                if (url == failingUrl)
                {
                    throw new InvalidOperationException($"404: {url}");
                }

                return url.ToUpperInvariant();
            }
            finally
            {
                Interlocked.Decrement(ref _current);
            }
        }

        private void UpdateMax(int value)
        {
            int seen;
            while (value > (seen = Volatile.Read(ref _max)))
            {
                if (Interlocked.CompareExchange(ref _max, value, seen) == seen)
                {
                    return;
                }
            }
        }
    }

    private static IReadOnlyList<string> Urls(int count) =>
        Enumerable.Range(1, count).Select(i => $"file-{i}").ToArray();

    [Fact]
    public async Task DownloadAllAsync_ReturnsResultsInOriginalOrder()
    {
        var probe = new ConcurrencyProbe();

        var result = await AsyncGuard.WithTimeout(
            ThrottledDownloader.DownloadAllAsync(["a", "b", "c"], probe.DownloadAsync, maxConcurrency: 2));

        Assert.Equal(new[] { "A", "B", "C" }, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task DownloadAllAsync_NeverExceedsMaxConcurrency(int limit)
    {
        var probe = new ConcurrencyProbe();

        var result = await AsyncGuard.WithTimeout(
            ThrottledDownloader.DownloadAllAsync(Urls(12), probe.DownloadAsync, limit));

        Assert.Equal(12, result.Count);
        Assert.InRange(probe.Max, 1, limit);
    }

    [Fact]
    public async Task DownloadAllAsync_EmptyList_ReturnsEmpty()
    {
        var probe = new ConcurrencyProbe();

        var result = await AsyncGuard.WithTimeout(
            ThrottledDownloader.DownloadAllAsync([], probe.DownloadAsync, maxConcurrency: 2));

        Assert.Empty(result);
    }

    [Fact]
    public async Task DownloadAllAsync_OneFails_OthersStillRun_AndExceptionPropagates()
    {
        // limit = 1 и первая загрузка падает: если слот семафора не освободить в finally,
        // остальные загрузки зависнут навсегда.
        var probe = new ConcurrencyProbe(failingUrl: "file-1");

        await Assert.ThrowsAsync<InvalidOperationException>(() => AsyncGuard.WithTimeout(
            ThrottledDownloader.DownloadAllAsync(Urls(5), probe.DownloadAsync, maxConcurrency: 1)));

        Assert.Equal(5, probe.Calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DownloadAllAsync_InvalidMaxConcurrency_Throws(int limit)
    {
        var probe = new ConcurrencyProbe();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => ThrottledDownloader.DownloadAllAsync(["a"], probe.DownloadAsync, limit));
    }

    [Fact]
    public async Task DownloadAllAsync_NullDownload_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => ThrottledDownloader.DownloadAllAsync(["a"], null!, 1));
    }
}
