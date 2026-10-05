using System.Text;
using Module12.Tasks;

namespace Module12.Tests;

public class SimpleObjectPoolTests
{
    private sealed class Connection
    {
        public int InUse;
    }

    [Fact]
    public void Rent_EmptyPool_CreatesNewObject()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 4);

        var sb = pool.Rent();

        Assert.NotNull(sb);
        Assert.Equal(1, pool.CreatedCount);
        Assert.Equal(0, pool.AvailableCount);
    }

    [Fact]
    public void RentAfterReturn_ReusesSameInstance()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 4);
        var first = pool.Rent();
        pool.Return(first);

        var second = pool.Rent();

        Assert.Same(first, second);
        Assert.Equal(1, pool.CreatedCount);
    }

    [Fact]
    public void Rent_IsLifo_ReturnsLastReturnedObject()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 4);
        var a = pool.Rent();
        var b = pool.Rent();
        pool.Return(a);
        pool.Return(b);

        Assert.Same(b, pool.Rent());
        Assert.Same(a, pool.Rent());
    }

    [Fact]
    public void Return_CallsReset()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 4, reset: sb => sb.Clear());
        var sb = pool.Rent();
        sb.Append("old report");

        pool.Return(sb);

        Assert.Equal(0, pool.Rent().Length);
    }

    [Fact]
    public void Return_WhenPoolIsFull_DropsExtraObjects()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 2);
        var items = new[] { pool.Rent(), pool.Rent(), pool.Rent() };

        foreach (var item in items)
        {
            pool.Return(item);
        }

        Assert.Equal(2, pool.AvailableCount);
        Assert.Equal(3, pool.CreatedCount);
    }

    [Fact]
    public void Return_Null_ThrowsArgumentNullException()
    {
        var pool = new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained: 1);

        Assert.Throws<ArgumentNullException>(() => pool.Return(null!));
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SimpleObjectPool<StringBuilder>(null!, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Constructor_InvalidMaxRetained_ThrowsArgumentOutOfRange(int maxRetained)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SimpleObjectPool<StringBuilder>(() => new StringBuilder(), maxRetained));
    }

    [Fact]
    public void ConcurrentRentReturn_ObjectIsNeverSharedBetweenThreads()
    {
        var pool = new SimpleObjectPool<Connection>(() => new Connection(), maxRetained: 8);
        var conflicts = 0;

        Parallel.For(0, 10_000, _ =>
        {
            var connection = pool.Rent();
            // Если кто-то уже использует этот объект — пул выдал его дважды.
            if (Interlocked.Exchange(ref connection.InUse, 1) != 0)
            {
                Interlocked.Increment(ref conflicts);
            }

            Interlocked.Exchange(ref connection.InUse, 0);
            pool.Return(connection);
        });

        Assert.Equal(0, conflicts);
        Assert.InRange(pool.AvailableCount, 1, 8);
    }
}
