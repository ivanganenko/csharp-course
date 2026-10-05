using Module11.Tasks;

namespace Module11.Tests;

public class MyLinqExtensionsTests
{
    private static IEnumerable<int> Naturals()
    {
        for (int i = 1; ; i++)
        {
            yield return i;
        }
    }

    [Fact]
    public void Batch_SplitsIntoBatches_LastMayBeSmaller()
    {
        var batches = Enumerable.Range(1, 7).Batch(3).ToList();

        Assert.Equal(3, batches.Count);
        Assert.Equal(new[] { 1, 2, 3 }, batches[0]);
        Assert.Equal(new[] { 4, 5, 6 }, batches[1]);
        Assert.Equal(new[] { 7 }, batches[2]);
    }

    [Fact]
    public void Batch_ExactMultiple_NoEmptyTrailingBatch()
    {
        var batches = Enumerable.Range(1, 4).Batch(2).ToList();

        Assert.Equal(2, batches.Count);
        Assert.Equal(new[] { 3, 4 }, batches[1]);
    }

    [Fact]
    public void Batch_Empty_ReturnsNoBatches()
    {
        Assert.Empty(Array.Empty<int>().Batch(3));
    }

    [Fact]
    public void Batch_IsLazy_WorksWithInfiniteSource()
    {
        int pulled = 0;
        IEnumerable<int> Counting()
        {
            foreach (int n in Naturals())
            {
                pulled++;
                yield return n;
            }
        }

        var query = Counting().Batch(3);
        Assert.Equal(0, pulled);

        var first = query.First();

        Assert.Equal(new[] { 1, 2, 3 }, first);
        Assert.Equal(3, pulled);
    }

    [Fact]
    public void Batch_InvalidArguments_ThrowImmediately()
    {
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null!).Batch(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new[] { 1 }.Batch(0));
    }

    [Fact]
    public void DistinctByKey_KeepsFirstPerKey_InOrder()
    {
        var people = new[] { ("Anna", 30), ("Boris", 25), ("Alla", 30), ("Vera", 25), ("Gleb", 40) };

        var result = people.DistinctByKey(p => p.Item2).Select(p => p.Item1);

        Assert.Equal(new[] { "Anna", "Boris", "Gleb" }, result);
    }

    [Fact]
    public void DistinctByKey_UsesComparer()
    {
        var emails = new[] { "A@example.com", "b@example.com", "a@EXAMPLE.com" };

        var result = emails.DistinctByKey(e => e, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(new[] { "A@example.com", "b@example.com" }, result);
    }

    [Fact]
    public void DistinctByKey_IsLazy_WorksWithInfiniteSource()
    {
        var firstThreeRemainders = Naturals().DistinctByKey(n => n % 3).Take(3);

        Assert.Equal(new[] { 1, 2, 3 }, firstThreeRemainders);
    }

    [Fact]
    public void DistinctByKey_InvalidArguments_ThrowImmediately()
    {
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null!).DistinctByKey(n => n));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.DistinctByKey<int, int>(null!));
    }

    [Fact]
    public void Pairwise_ReturnsNeighbourPairs()
    {
        var pairs = new[] { 'a', 'b', 'c', 'd' }.Pairwise().ToList();

        Assert.Equal(new[] { ('a', 'b'), ('b', 'c'), ('c', 'd') }, pairs);
    }

    [Fact]
    public void Pairwise_LessThanTwoElements_ReturnsEmpty()
    {
        Assert.Empty(new[] { 1 }.Pairwise());
        Assert.Empty(Array.Empty<int>().Pairwise());
    }

    [Fact]
    public void Pairwise_PriceChanges_Example()
    {
        decimal[] prices = { 100m, 105m, 103m };

        var changes = prices.Pairwise().Select(p => p.Current - p.Previous);

        Assert.Equal(new[] { 5m, -2m }, changes);
    }

    [Fact]
    public void Pairwise_IsLazy_AndValidatesImmediately()
    {
        Assert.Equal((1, 2), Naturals().Pairwise().First());
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<int>)null!).Pairwise());
    }
}
