using Module10.Tasks;

namespace Module10.Tests;

public class ClosureLabTests
{
    [Fact]
    public void CreateCounter_ReturnsIncreasingNumbers()
    {
        Func<int> counter = ClosureLab.CreateCounter();

        Assert.Equal(1, counter());
        Assert.Equal(2, counter());
        Assert.Equal(3, counter());
    }

    [Fact]
    public void CreateCounter_CountersAreIndependent()
    {
        var first = ClosureLab.CreateCounter();
        var second = ClosureLab.CreateCounter();

        first();
        first();

        Assert.Equal(3, first());
        Assert.Equal(1, second());
    }

    [Fact]
    public void CreateMultipliers_EachFunctionReturnsItsOwnIndexTimesTen()
    {
        var functions = ClosureLab.CreateMultipliers(4);

        Assert.Equal(4, functions.Count);
        Assert.Equal(new[] { 0, 10, 20, 30 }, functions.Select(f => f()));
    }

    [Fact]
    public void CreateMultipliers_Zero_ReturnsEmpty()
    {
        Assert.Empty(ClosureLab.CreateMultipliers(0));
    }

    [Fact]
    public void CreateMultipliers_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ClosureLab.CreateMultipliers(-1));
    }

    [Fact]
    public void CreateLogger_AddsPrefixedMessagesToSink()
    {
        var sink = new List<string>();
        Action<string> log = ClosureLab.CreateLogger(sink, "Orders");

        log("created");
        log("paid");

        Assert.Equal(new[] { "[Orders] created", "[Orders] paid" }, sink);
    }

    [Fact]
    public void CreateLogger_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => ClosureLab.CreateLogger(null!, "x"));
        Assert.Throws<ArgumentNullException>(() => ClosureLab.CreateLogger(new List<string>(), null!));
    }

    [Fact]
    public void Compose_AppliesFirstThenSecond()
    {
        Func<int, int> addOne = x => x + 1;
        Func<int, int> twice = x => x * 2;

        Assert.Equal(8, ClosureLab.Compose(addOne, twice)(3));  // (3 + 1) * 2
        Assert.Equal(7, ClosureLab.Compose(twice, addOne)(3));  // 3 * 2 + 1
    }

    [Fact]
    public void Compose_NullArguments_Throw()
    {
        Func<int, int> id = x => x;

        Assert.Throws<ArgumentNullException>(() => ClosureLab.Compose(null!, id));
        Assert.Throws<ArgumentNullException>(() => ClosureLab.Compose(id, null!));
    }

    [Fact]
    public void Memoize_CallsOriginalOncePerArgument()
    {
        int calls = 0;
        Func<int, long> slowSquare = n => { calls++; return (long)n * n; };

        var fast = ClosureLab.Memoize(slowSquare);

        Assert.Equal(16L, fast(4));
        Assert.Equal(16L, fast(4));
        Assert.Equal(25L, fast(5));
        Assert.Equal(2, calls);
    }

    [Fact]
    public void Memoize_SeparateMemoizedFunctionsHaveSeparateCaches()
    {
        int calls = 0;
        Func<string, int> length = s => { calls++; return s.Length; };

        var a = ClosureLab.Memoize(length);
        var b = ClosureLab.Memoize(length);
        a("abc");
        b("abc");

        Assert.Equal(2, calls);
    }

    [Fact]
    public void Memoize_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ClosureLab.Memoize<int, int>(null!));
    }
}
