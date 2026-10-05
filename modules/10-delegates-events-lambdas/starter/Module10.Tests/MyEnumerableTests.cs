using Module10.Tasks;

namespace Module10.Tests;

public class MyEnumerableTests
{
    [Fact]
    public void Filter_ReturnsMatchingElementsInOrder()
    {
        var numbers = new[] { 1, 2, 3, 4, 5, 6 };

        Assert.Equal(new[] { 2, 4, 6 }, numbers.Filter(n => n % 2 == 0));
    }

    [Fact]
    public void Filter_IsLazy_PredicateNotCalledUntilEnumeration()
    {
        int calls = 0;
        var numbers = new[] { 1, 2, 3 };

        var query = numbers.Filter(n => { calls++; return n > 1; });
        Assert.Equal(0, calls);

        var result = query.ToList();

        Assert.Equal(3, calls);
        Assert.Equal(new[] { 2, 3 }, result);
    }

    [Fact]
    public void Filter_NullArguments_ThrowImmediately()
    {
        int[]? nullSource = null;

        Assert.Throws<ArgumentNullException>(() => nullSource!.Filter(n => true));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Filter(null!));
    }

    [Fact]
    public void Map_TransformsEachElement()
    {
        var words = new[] { "a", "bb", "ccc" };

        Assert.Equal(new[] { 1, 2, 3 }, words.Map(w => w.Length));
    }

    [Fact]
    public void Map_WorksWithMethodGroup()
    {
        var numbers = new[] { 1, 2, 3 };

        Assert.Equal(new[] { "1", "2", "3" }, numbers.Map(Convert.ToString));
    }

    [Fact]
    public void Map_IsLazy()
    {
        int calls = 0;

        var query = new[] { 1, 2, 3 }.Map(n => { calls++; return n * 2; });
        Assert.Equal(0, calls);

        Assert.Equal(2, query.First());
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Map_NullArguments_ThrowImmediately()
    {
        Assert.Throws<ArgumentNullException>(() => ((int[])null!).Map(n => n));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Map<int, int>(null!));
    }

    [Fact]
    public void Reduce_SumsNumbers()
    {
        int sum = new[] { 1, 2, 3, 4 }.Reduce(0, (acc, n) => acc + n);

        Assert.Equal(10, sum);
    }

    [Fact]
    public void Reduce_CanChangeAccumulatorType()
    {
        string csv = new[] { 1, 2, 3 }.Reduce("", (acc, n) => acc.Length == 0 ? n.ToString() : acc + "," + n);

        Assert.Equal("1,2,3", csv);
    }

    [Fact]
    public void Reduce_Empty_ReturnsSeed()
    {
        Assert.Equal(42, Array.Empty<int>().Reduce(42, (acc, n) => acc + n));
    }

    [Fact]
    public void Reduce_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => ((int[])null!).Reduce(0, (acc, n) => acc + n));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.Reduce<int, int>(0, null!));
    }

    [Fact]
    public void CountWhere_UsesPredicate()
    {
        var names = new string?[] { "Anna", "", null, "Bob" };

        Assert.Equal(2, names.CountWhere(string.IsNullOrEmpty));
        Assert.Equal(1, names.CountWhere(n => n == "Bob"));
    }

    [Fact]
    public void CountWhere_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => ((int[])null!).CountWhere(n => true));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.CountWhere(null!));
    }
}
