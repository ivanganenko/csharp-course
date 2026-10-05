using Module08.Tasks;

namespace Module08.Tests;

public class CacheTests
{
    [Fact]
    public void Set_ThenTryGet_ReturnsValueAndCountsHit()
    {
        var cache = new Cache<string, decimal>();
        cache.Set("EUR", 1.08m);

        bool found = cache.TryGet("EUR", out decimal rate);

        Assert.True(found);
        Assert.Equal(1.08m, rate);
        Assert.Equal(1, cache.Hits);
        Assert.Equal(0, cache.Misses);
    }

    [Fact]
    public void TryGet_MissingKey_ReturnsFalseDefaultAndCountsMiss()
    {
        var cache = new Cache<string, int>();

        bool found = cache.TryGet("missing", out int value);

        Assert.False(found);
        Assert.Equal(0, value);
        Assert.Equal(0, cache.Hits);
        Assert.Equal(1, cache.Misses);
    }

    [Fact]
    public void Set_ExistingKey_OverwritesValue()
    {
        var cache = new Cache<int, string>();
        cache.Set(1, "old");
        cache.Set(1, "new");

        Assert.True(cache.TryGet(1, out string? value));
        Assert.Equal("new", value);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void GetOrAdd_CallsFactoryOnlyOncePerKey()
    {
        var cache = new Cache<int, string>();
        int factoryCalls = 0;
        string Factory(int id)
        {
            factoryCalls++;
            return $"Reader #{id}";
        }

        string first = cache.GetOrAdd(7, Factory);
        string second = cache.GetOrAdd(7, Factory);

        Assert.Equal("Reader #7", first);
        Assert.Equal("Reader #7", second);
        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, cache.Misses);
        Assert.Equal(1, cache.Hits);
    }

    [Fact]
    public void GetOrAdd_DifferentKeys_CallsFactoryForEach()
    {
        var cache = new Cache<string, int>();

        cache.GetOrAdd("a", key => key.Length);
        cache.GetOrAdd("bbb", key => key.Length);

        Assert.Equal(2, cache.Count);
        Assert.Equal(2, cache.Misses);
        Assert.True(cache.TryGet("bbb", out int length));
        Assert.Equal(3, length);
    }

    [Fact]
    public void GetOrAdd_NullFactory_ThrowsArgumentNullException()
    {
        var cache = new Cache<string, int>();

        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("a", null!));
    }

    [Fact]
    public void Remove_ExistingAndMissing_ReturnsCorrectFlag()
    {
        var cache = new Cache<string, int>();
        cache.Set("a", 1);

        Assert.True(cache.Remove("a"));
        Assert.False(cache.Remove("a"));
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Set_NullKey_ThrowsArgumentNullException()
    {
        var cache = new Cache<string, int>();

        Assert.Throws<ArgumentNullException>(() => cache.Set(null!, 1));
    }
}
