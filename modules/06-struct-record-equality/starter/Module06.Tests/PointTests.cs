using Module06.Tasks;

namespace Module06.Tests;

public class PointTests
{
    [Fact]
    public void Equals_SameCoordinates_TrueForBothOverloads()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 2);

        Assert.NotSame(a, b);
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.True(b.Equals(a));
    }

    [Theory]
    [InlineData(1, 3)]
    [InlineData(2, 2)]
    [InlineData(2, 1)]
    public void Equals_DifferentCoordinates_False(int x, int y)
    {
        var a = new Point(1, 2);

        Assert.False(a.Equals(new Point(x, y)));
        Assert.False(a == new Point(x, y));
        Assert.True(a != new Point(x, y));
    }

    [Fact]
    public void Equals_NullOrOtherType_False()
    {
        var a = new Point(1, 2);

        Assert.False(a.Equals((Point?)null));
        Assert.False(a.Equals((object?)null));
        Assert.False(a.Equals((object)"(1, 2)"));
    }

    [Fact]
    public void EqualityOperators_CompareByValueAndHandleNull()
    {
        Point? a = new Point(3, 4);
        Point? b = new Point(3, 4);
        Point? none1 = null;
        Point? none2 = null;

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(none1 == none2);
        Assert.False(a == none1);
        Assert.False(none1 == a);
        Assert.True(a != none1);
    }

    [Fact]
    public void GetHashCode_EqualPoints_SameHash()
    {
        Assert.Equal(new Point(10, -5).GetHashCode(), new Point(10, -5).GetHashCode());
    }

    [Fact]
    public void HashSet_FindsEqualPointAndRejectsDuplicate()
    {
        var set = new HashSet<Point> { new Point(1, 1), new Point(2, 2) };

        Assert.True(set.Contains(new Point(1, 1)));
        Assert.False(set.Add(new Point(2, 2)));
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void Dictionary_LooksUpByEqualKey()
    {
        var names = new Dictionary<Point, string> { [new Point(0, 0)] = "origin" };

        Assert.True(names.TryGetValue(new Point(0, 0), out var name));
        Assert.Equal("origin", name);
    }
}
