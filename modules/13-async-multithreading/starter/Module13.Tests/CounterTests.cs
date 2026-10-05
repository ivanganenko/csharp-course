using Module13.Tasks;

namespace Module13.Tests;

// Корректный потокобезопасный счётчик ВСЕГДА даёт точный результат —
// поэтому эти тесты детерминированы, хотя и используют много потоков.
public class CounterTests
{
    private static ICounter Create(string kind) => kind switch
    {
        "lock" => new LockCounter(),
        "interlocked" => new InterlockedCounter(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    [Theory]
    [InlineData("lock")]
    [InlineData("interlocked")]
    public void NewCounter_ValueIsZero(string kind)
    {
        Assert.Equal(0, Create(kind).Value);
    }

    [Theory]
    [InlineData("lock")]
    [InlineData("interlocked")]
    public void Increment_FromManyThreads_NoLostUpdates(string kind)
    {
        var counter = Create(kind);

        Parallel.For(0, 100_000, _ => counter.Increment());

        Assert.Equal(100_000, counter.Value);
    }

    [Theory]
    [InlineData("lock")]
    [InlineData("interlocked")]
    public void Add_FromManyThreads_SumsAllAmounts(string kind)
    {
        var counter = Create(kind);

        Parallel.For(0, 1_000, i => counter.Add(i));

        Assert.Equal(499_500, counter.Value); // 0 + 1 + ... + 999
    }

    [Theory]
    [InlineData("lock")]
    [InlineData("interlocked")]
    public void Add_NegativeAmount_Decreases(string kind)
    {
        var counter = Create(kind);

        counter.Add(10);
        counter.Add(-15);

        Assert.Equal(-5, counter.Value);
    }

    [Theory]
    [InlineData("lock")]
    [InlineData("interlocked")]
    public void IncrementAndAdd_Mixed_FromManyThreads(string kind)
    {
        var counter = Create(kind);

        Parallel.For(0, 20_000, i =>
        {
            if (i % 2 == 0)
            {
                counter.Increment();
            }
            else
            {
                counter.Add(2);
            }
        });

        Assert.Equal(10_000 * 1 + 10_000 * 2, counter.Value);
    }

    [Fact]
    public void TryIncrementUpTo_FromManyThreads_NeverExceedsLimit()
    {
        var counter = new LockCounter();
        var successes = 0;

        Parallel.For(0, 10_000, _ =>
        {
            if (counter.TryIncrementUpTo(500))
            {
                Interlocked.Increment(ref successes);
            }
        });

        Assert.Equal(500, counter.Value);
        Assert.Equal(500, successes);
    }

    [Fact]
    public void TryIncrementUpTo_AtLimit_ReturnsFalse()
    {
        var counter = new LockCounter();
        counter.Add(3);

        Assert.False(counter.TryIncrementUpTo(3));
        Assert.True(counter.TryIncrementUpTo(4));
        Assert.Equal(4, counter.Value);
    }
}
