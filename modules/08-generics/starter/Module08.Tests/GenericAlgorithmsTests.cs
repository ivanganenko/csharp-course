using System.Text;
using Module08.Tasks;

namespace Module08.Tests;

public class GenericAlgorithmsTests
{
    [Fact]
    public void Max_Ints_ReturnsLargest()
    {
        Assert.Equal(42, GenericAlgorithms.Max(new[] { 3, 42, -7, 15 }));
        Assert.Equal(7, GenericAlgorithms.Max(new[] { 7 }));
    }

    [Fact]
    public void Max_NegativeDoubles_ReturnsLargest()
    {
        Assert.Equal(-0.5, GenericAlgorithms.Max(new[] { -3.5, -0.5, -10.0 }));
    }

    [Fact]
    public void Max_Dates_ReturnsLatest()
    {
        var dates = new List<DateOnly>
        {
            new(2024, 3, 1),
            new(2025, 1, 15),
            new(2023, 12, 31),
        };

        Assert.Equal(new DateOnly(2025, 1, 15), GenericAlgorithms.Max(dates));
    }

    [Fact]
    public void Max_EmptyOrNull_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => GenericAlgorithms.Max(Array.Empty<int>()));
        Assert.Throws<ArgumentNullException>(() => GenericAlgorithms.Max<int>(null!));
    }

    [Theory]
    [InlineData(5, 1, 10, 5)]
    [InlineData(-3, 1, 10, 1)]
    [InlineData(42, 1, 10, 10)]
    [InlineData(1, 1, 10, 1)]
    [InlineData(10, 1, 10, 10)]
    [InlineData(7, 7, 7, 7)]
    public void Clamp_Ints_ReturnsValueInsideRange(int value, int min, int max, int expected)
    {
        Assert.Equal(expected, GenericAlgorithms.Clamp(value, min, max));
    }

    [Fact]
    public void Clamp_MinGreaterThanMax_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => GenericAlgorithms.Clamp(5, 10, 1));
    }

    [Fact]
    public void Swap_ExchangesValues_ForValueAndReferenceTypes()
    {
        int a = 1, b = 2;
        string left = "left", right = "right";

        GenericAlgorithms.Swap(ref a, ref b);
        GenericAlgorithms.Swap(ref left, ref right);

        Assert.Equal(2, a);
        Assert.Equal(1, b);
        Assert.Equal("right", left);
        Assert.Equal("left", right);
    }

    [Fact]
    public void CountDefaults_Ints_CountsZeros()
    {
        Assert.Equal(2, GenericAlgorithms.CountDefaults(new[] { 0, 1, 0, 2 }));
        Assert.Equal(0, GenericAlgorithms.CountDefaults(new List<bool>()));
    }

    [Fact]
    public void CountDefaults_Strings_CountsNulls()
    {
        var items = new string?[] { "a", null, "b", null, "" };

        // "" — не default: default(string) — это null.
        Assert.Equal(2, GenericAlgorithms.CountDefaults(items));
    }

    [Fact]
    public void CountDefaults_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => GenericAlgorithms.CountDefaults<int>(null!));
    }

    [Fact]
    public void CreateMany_ReturnsDistinctNewInstances()
    {
        List<StringBuilder> builders = GenericAlgorithms.CreateMany<StringBuilder>(3);

        Assert.Equal(3, builders.Count);
        Assert.All(builders, b => Assert.Equal(0, b.Length));
        Assert.NotSame(builders[0], builders[1]);
        Assert.NotSame(builders[1], builders[2]);
        Assert.Empty(GenericAlgorithms.CreateMany<StringBuilder>(0));
    }

    [Fact]
    public void CreateMany_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GenericAlgorithms.CreateMany<StringBuilder>(-1));
    }
}
