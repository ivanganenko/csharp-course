using Module03.Tasks;

namespace Module03.Tests;

public class BinarySearchTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(5, 2)]
    [InlineData(9, 4)]
    [InlineData(7, 3)]
    public void IndexOf_ExistingValue_ReturnsIndex(int target, int expectedIndex)
    {
        int[] sorted = [1, 3, 5, 7, 9];

        Assert.Equal(expectedIndex, BinarySearch.IndexOf(sorted, target));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void IndexOf_MissingValue_ReturnsMinusOne(int target)
    {
        int[] sorted = [1, 3, 5, 7, 9];

        Assert.Equal(-1, BinarySearch.IndexOf(sorted, target));
    }

    [Fact]
    public void IndexOf_EmptyArray_ReturnsMinusOne()
    {
        Assert.Equal(-1, BinarySearch.IndexOf([], 5));
    }

    [Fact]
    public void IndexOf_SingleElement_FindsIt()
    {
        Assert.Equal(0, BinarySearch.IndexOf([42], 42));
        Assert.Equal(-1, BinarySearch.IndexOf([42], 41));
    }

    [Fact]
    public void IndexOf_Duplicates_ReturnsIndexOfAnyOccurrence()
    {
        int[] sorted = [1, 2, 2, 2, 3];

        var index = BinarySearch.IndexOf(sorted, 2);

        Assert.InRange(index, 1, 3);
    }

    [Fact]
    public void IndexOf_LargeArray_FindsEveryHundredthElement()
    {
        var sorted = new int[100_000];
        for (var i = 0; i < sorted.Length; i++)
        {
            sorted[i] = i * 2; // только чётные числа
        }

        for (var i = 0; i < sorted.Length; i += 100)
        {
            Assert.Equal(i, BinarySearch.IndexOf(sorted, i * 2));
            Assert.Equal(-1, BinarySearch.IndexOf(sorted, i * 2 + 1));
        }
    }

    [Fact]
    public void IndexOf_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => BinarySearch.IndexOf(null!, 1));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 4)]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(5, 4)]
    [InlineData(9, 5)]
    public void LowerBound_VariousTargets_ReturnsFirstIndexNotLess(int target, int expected)
    {
        int[] sorted = [1, 2, 2, 2, 5];

        Assert.Equal(expected, BinarySearch.LowerBound(sorted, target));
    }

    [Fact]
    public void LowerBound_EmptyArray_ReturnsZero()
    {
        Assert.Equal(0, BinarySearch.LowerBound([], 5));
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(1, 1)]
    [InlineData(5, 1)]
    [InlineData(4, 0)]
    [InlineData(100, 0)]
    public void CountOccurrences_VariousTargets_ReturnsCount(int target, int expected)
    {
        int[] sorted = [1, 2, 2, 2, 5];

        Assert.Equal(expected, BinarySearch.CountOccurrences(sorted, target));
    }

    [Fact]
    public void CountOccurrences_MaxValue_DoesNotOverflow()
    {
        int[] sorted = [1, int.MaxValue, int.MaxValue];

        Assert.Equal(2, BinarySearch.CountOccurrences(sorted, int.MaxValue));
    }

    [Fact]
    public void CountOccurrences_AllEqual_ReturnsLength()
    {
        Assert.Equal(4, BinarySearch.CountOccurrences([7, 7, 7, 7], 7));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(15, 3)]
    [InlineData(16, 4)]
    [InlineData(17, 4)]
    [InlineData(1_000_000, 1000)]
    [InlineData(int.MaxValue, 46340)]
    public void IntegerSqrt_VariousN_ReturnsFloorOfRoot(int n, int expected)
    {
        Assert.Equal(expected, BinarySearch.IntegerSqrt(n));
    }

    [Fact]
    public void IntegerSqrt_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BinarySearch.IntegerSqrt(-1));
    }
}
