using Module09.Tasks;

namespace Module09.Tests;

public class LruCacheTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_NonPositiveCapacity_Throws(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LruCache<string, int>(capacity));
    }

    [Fact]
    public void Put_ThenTryGet_ReturnsValue()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);

        Assert.True(cache.TryGet("a", out int value));
        Assert.Equal(1, value);
        Assert.Equal(1, cache.Count);
        Assert.Equal(2, cache.Capacity);
    }

    [Fact]
    public void TryGet_Missing_ReturnsFalse()
    {
        var cache = new LruCache<string, int>(2);

        Assert.False(cache.TryGet("missing", out int value));
        Assert.Equal(0, value);
    }

    [Fact]
    public void Put_OverCapacity_EvictsLeastRecentlyUsed()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);

        cache.Put("c", 3); // "a" — самый старый, вытесняется

        Assert.False(cache.ContainsKey("a"));
        Assert.True(cache.ContainsKey("b"));
        Assert.True(cache.ContainsKey("c"));
        Assert.Equal(2, cache.Count);
    }

    [Fact]
    public void TryGet_MakesItemMostRecent_SoAnotherIsEvicted()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.TryGet("a", out _); // теперь "b" — самый старый

        cache.Put("c", 3);

        Assert.True(cache.ContainsKey("a"));
        Assert.False(cache.ContainsKey("b"));
    }

    [Fact]
    public void Put_ExistingKey_UpdatesValueWithoutEviction()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);

        cache.Put("a", 100); // обновление: никого не вытесняем, "a" становится свежим

        Assert.Equal(2, cache.Count);
        Assert.True(cache.TryGet("a", out int value));
        Assert.Equal(100, value);
        Assert.Equal(new[] { "a", "b" }, cache.KeysByRecency());
    }

    [Fact]
    public void ContainsKey_DoesNotChangeRecency()
    {
        var cache = new LruCache<string, int>(2);
        cache.Put("a", 1);
        cache.Put("b", 2);
        Assert.True(cache.ContainsKey("a")); // НЕ освежает "a"

        cache.Put("c", 3);

        Assert.False(cache.ContainsKey("a"));
    }

    [Fact]
    public void CapacityOne_KeepsOnlyLastItem()
    {
        var cache = new LruCache<int, string>(1);
        cache.Put(1, "one");
        cache.Put(2, "two");

        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.TryGet(2, out string? value));
        Assert.Equal("two", value);
    }

    [Fact]
    public void KeysByRecency_ReflectsAccessOrder()
    {
        var cache = new LruCache<string, int>(3);
        cache.Put("a", 1);
        cache.Put("b", 2);
        cache.Put("c", 3);
        cache.TryGet("a", out _);

        Assert.Equal(new[] { "a", "c", "b" }, cache.KeysByRecency());
    }

    [Fact]
    public void ManyOperations_CountNeverExceedsCapacity()
    {
        var cache = new LruCache<int, int>(10);
        for (int i = 0; i < 1000; i++)
        {
            cache.Put(i, i * i);
        }

        Assert.Equal(10, cache.Count);
        Assert.Equal(Enumerable.Range(990, 10).Reverse(), cache.KeysByRecency());
        Assert.True(cache.TryGet(999, out int square));
        Assert.Equal(998001, square);
    }
}
