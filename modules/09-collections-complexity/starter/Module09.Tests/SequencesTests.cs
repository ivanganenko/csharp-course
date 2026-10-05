using Module09.Tasks;

namespace Module09.Tests;

public class SequencesTests
{
    [Fact]
    public void Range_DefaultStep_ReturnsConsecutiveNumbers()
    {
        Assert.Equal(new[] { 3, 4, 5, 6 }, Sequences.Range(3, 7));
    }

    [Fact]
    public void Range_WithStep_StopsBeforeUpperBound()
    {
        Assert.Equal(new[] { 0, 5, 10 }, Sequences.Range(0, 15, 5));
        Assert.Equal(new[] { 0, 5, 10, 15 }, Sequences.Range(0, 16, 5));
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(10, 1)]
    public void Range_FromNotLessThanTo_ReturnsEmpty(int from, int to)
    {
        Assert.Empty(Sequences.Range(from, to));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Range_NonPositiveStep_ThrowsImmediately_WithoutEnumeration(int step)
    {
        // Обрати внимание: результат НЕ перебирается. Исключение должно вылететь при вызове метода.
        Assert.Throws<ArgumentOutOfRangeException>(() => Sequences.Range(0, 10, step));
    }

    [Fact]
    public void Range_NearIntMaxValue_DoesNotOverflow()
    {
        // При наивном `current += step` число «перескочит» через int.MaxValue в отрицательные
        // и цикл никогда не закончится. Take(10) — страховка, чтобы тест не завис.
        var result = Sequences.Range(int.MaxValue - 2, int.MaxValue, 5).Take(10).ToList();

        Assert.Equal(new[] { int.MaxValue - 2 }, result);
    }

    [Fact]
    public void Naturals_IsInfiniteAndLazy()
    {
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, Sequences.Naturals().Take(5));
    }

    [Fact]
    public void EveryNth_ReturnsEveryNthElement()
    {
        var letters = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };

        Assert.Equal(new[] { 'c', 'f' }, Sequences.EveryNth(letters, 3));
    }

    [Fact]
    public void EveryNth_NEqualsOne_ReturnsAll()
    {
        Assert.Equal(new[] { 1, 2, 3 }, Sequences.EveryNth(new[] { 1, 2, 3 }, 1));
    }

    [Fact]
    public void EveryNth_WorksWithInfiniteSource()
    {
        Assert.Equal(new[] { 2, 4, 6 }, Sequences.EveryNth(Sequences.Naturals(), 2).Take(3));
    }

    [Fact]
    public void EveryNth_IsLazy_PullsOnlyNeededElements()
    {
        var pulled = 0;
        IEnumerable<int> CountingSource()
        {
            for (int i = 1; i <= 100; i++)
            {
                pulled++;
                yield return i;
            }
        }

        var sequence = Sequences.EveryNth(CountingSource(), 10);
        Assert.Equal(0, pulled); // ещё ничего не перебрано

        var first = sequence.First();

        Assert.Equal(10, first);
        Assert.Equal(10, pulled); // перебрано ровно 10 элементов, а не все 100
    }

    [Fact]
    public void EveryNth_InvalidArguments_ThrowImmediately()
    {
        Assert.Throws<ArgumentNullException>(() => Sequences.EveryNth<int>(null!, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => Sequences.EveryNth(new[] { 1 }, 0));
    }
}
