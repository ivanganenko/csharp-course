using Module06.Tasks;

namespace Module06.Tests;

public class StatisticsTests
{
    [Fact]
    public void MinMax_ReturnsNamedTuple()
    {
        var result = Statistics.MinMax(new[] { 4, -2, 9, 0 });

        Assert.Equal(-2, result.Min);
        Assert.Equal(9, result.Max);
    }

    [Fact]
    public void MinMax_SupportsDeconstructionAndValueEquality()
    {
        var (min, max) = Statistics.MinMax(new List<int> { 7 });

        Assert.Equal(7, min);
        Assert.Equal(7, max);
        Assert.True(Statistics.MinMax(new[] { 1, 5, 3 }) == (1, 5));
    }

    [Fact]
    public void MinMax_InvalidInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Statistics.MinMax(null!));
        Assert.Throws<InvalidOperationException>(() => Statistics.MinMax(Array.Empty<int>()));
    }

    [Fact]
    public void CountAndAverage_ComputesBoth()
    {
        var (count, average) = Statistics.CountAndAverage(new[] { 1, 2, 3, 4 });

        Assert.Equal(4, count);
        Assert.Equal(2.5, average, 10);
    }

    [Fact]
    public void CountAndAverage_LargeValues_NoIntOverflow()
    {
        var result = Statistics.CountAndAverage(new[] { int.MaxValue, int.MaxValue });

        Assert.Equal(int.MaxValue, result.Average, 1);
    }

    [Fact]
    public void CountAndAverage_Empty_ReturnsZeros()
    {
        Assert.True(Statistics.CountAndAverage(Array.Empty<int>()) == (0, 0.0));
        Assert.Throws<ArgumentNullException>(() => Statistics.CountAndAverage(null!));
    }

    [Fact]
    public void Swap_ReturnsSwappedTuple()
    {
        var swapped = Statistics.Swap((1, "a"));

        Assert.Equal("a", swapped.Item1);
        Assert.Equal(1, swapped.Item2);
    }
}
