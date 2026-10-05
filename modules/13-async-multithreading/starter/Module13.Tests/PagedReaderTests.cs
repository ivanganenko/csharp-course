using Module13.Tasks;

namespace Module13.Tests;

public class PagedReaderTests
{
    private sealed class FakePages(params string[][] pages)
    {
        public List<int> Loaded { get; } = [];

        public Task<IReadOnlyList<string>> LoadAsync(int page, CancellationToken cancellationToken)
        {
            Loaded.Add(page);
            IReadOnlyList<string> items = page < pages.Length ? pages[page] : Array.Empty<string>();
            return Task.FromResult(items);
        }
    }

    private static async Task<List<string>> CollectAsync(IAsyncEnumerable<string> source)
    {
        var result = new List<string>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }

    [Fact]
    public async Task ReadAllAsync_ReadsItemsFromAllPages()
    {
        var pages = new FakePages(new[] { "a", "b" }, new[] { "c" });

        var items = await AsyncGuard.WithTimeout(CollectAsync(PagedReader.ReadAllAsync(pages.LoadAsync)));

        Assert.Equal(new[] { "a", "b", "c" }, items);
        Assert.Equal(new[] { 0, 1, 2 }, pages.Loaded); // страница 2 пустая — конец данных
    }

    [Fact]
    public async Task ReadAllAsync_FirstPageEmpty_ReturnsNothing()
    {
        var pages = new FakePages();

        var items = await AsyncGuard.WithTimeout(CollectAsync(PagedReader.ReadAllAsync(pages.LoadAsync)));

        Assert.Empty(items);
        Assert.Equal(new[] { 0 }, pages.Loaded);
    }

    [Fact]
    public async Task ReadAllAsync_IsLazy_StopsLoadingWhenConsumerStops()
    {
        var pages = new FakePages(new[] { "a", "b" }, new[] { "c" });
        string? first = null;

        await foreach (var item in PagedReader.ReadAllAsync(pages.LoadAsync))
        {
            first = item;
            break;
        }

        Assert.Equal("a", first);
        Assert.Equal(new[] { 0 }, pages.Loaded);
    }

    [Fact]
    public async Task ReadAllAsync_AlreadyCancelled_ThrowsOperationCanceled()
    {
        var pages = new FakePages(new[] { "a" });
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => AsyncGuard.WithTimeout(CollectAsync(PagedReader.ReadAllAsync(pages.LoadAsync, cts.Token))));

        Assert.Empty(pages.Loaded);
    }

    [Fact]
    public async Task ReadAllAsync_WithCancellation_StopsBeforeNextPage()
    {
        var pages = new FakePages(new[] { "a" }, new[] { "b" });
        using var cts = new CancellationTokenSource();
        var received = new List<string>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            // Токен передаётся НЕ параметром, а через WithCancellation — работает только с [EnumeratorCancellation].
            await foreach (var item in PagedReader.ReadAllAsync(pages.LoadAsync).WithCancellation(cts.Token))
            {
                received.Add(item);
                cts.Cancel();
            }
        });

        Assert.Equal(new[] { "a" }, received);
        Assert.Equal(new[] { 0 }, pages.Loaded);
    }

    [Fact]
    public async Task ReadAllAsync_NullLoader_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CollectAsync(PagedReader.ReadAllAsync(null!)));
    }
}
