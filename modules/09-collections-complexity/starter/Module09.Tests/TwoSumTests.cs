using Module09.Tasks;

namespace Module09.Tests;

public class TwoSumTests
{
    [Theory]
    [InlineData(new[] { 2, 7, 11, 15 }, 9, 0, 1)]
    [InlineData(new[] { 3, 2, 4 }, 6, 1, 2)]
    [InlineData(new[] { 3, 3 }, 6, 0, 1)]
    [InlineData(new[] { -5, 10, 20, 15 }, 10, 0, 3)]
    [InlineData(new[] { 0, 4, 0 }, 0, 0, 2)]
    public void Find_PairExists_ReturnsIndices(int[] numbers, int target, int expectedFirst, int expectedSecond)
    {
        var result = TwoSum.Find(numbers, target);

        Assert.NotNull(result);
        Assert.Equal((expectedFirst, expectedSecond), result.Value);
    }

    [Fact]
    public void Find_SeveralPairs_ReturnsPairWithSmallestSecondIndex()
    {
        // Пары: (0,3) 1+4, (1,2) 2+3, (0,5) 1+4 ... минимальный j = 2 → (1, 2).
        var result = TwoSum.Find(new[] { 1, 2, 3, 4, 0, 4 }, 5);

        Assert.Equal((1, 2), result!.Value);
    }

    [Fact]
    public void Find_SameValueRepeated_UsesFirstOccurrenceAsFirstIndex()
    {
        // Для j = 3 (значение 7) подходят i = 0 и i = 1 (оба значения 5) — берём минимальный i.
        // Если в словаре перезаписывать индекс последним вхождением, получится (1, 3) — неверно.
        var result = TwoSum.Find(new[] { 5, 5, 1, 7 }, 12);

        Assert.Equal((0, 3), result!.Value);
    }

    [Fact]
    public void Find_ElementCannotBeUsedTwice()
    {
        // 3 + 3 = 6, но тройка одна.
        Assert.Null(TwoSum.Find(new[] { 3, 1 }, 6));
    }

    [Theory]
    [InlineData(new int[0], 5)]
    [InlineData(new[] { 5 }, 5)]
    [InlineData(new[] { 1, 2, 3 }, 100)]
    public void Find_NoPair_ReturnsNull(int[] numbers, int target)
    {
        Assert.Null(TwoSum.Find(numbers, target));
    }

    [Fact]
    public void Find_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TwoSum.Find(null!, 1));
    }
}
