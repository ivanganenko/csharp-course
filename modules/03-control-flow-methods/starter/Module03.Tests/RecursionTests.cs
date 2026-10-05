using Module03.Tasks;

namespace Module03.Tests;

public class RecursionTests
{
    [Theory]
    [InlineData(0, 1L)]
    [InlineData(1, 1L)]
    [InlineData(5, 120L)]
    [InlineData(10, 3_628_800L)]
    [InlineData(20, 2_432_902_008_176_640_000L)]
    public void Factorial_ValidN_ReturnsFactorial(int n, long expected)
    {
        Assert.Equal(expected, Recursion.Factorial(n));
    }

    [Fact]
    public void Factorial_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Recursion.Factorial(-1));
    }

    [Fact]
    public void Factorial_TooLarge_ThrowsOverflowException()
    {
        Assert.Throws<OverflowException>(() => Recursion.Factorial(21));
    }

    [Theory]
    [InlineData(0, 0L)]
    [InlineData(1, 1L)]
    [InlineData(2, 1L)]
    [InlineData(10, 55L)]
    [InlineData(25, 75_025L)]
    public void Fibonacci_SmallN_ReturnsValue(int n, long expected)
    {
        Assert.Equal(expected, Recursion.Fibonacci(n));
    }

    [Fact]
    public void Fibonacci_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Recursion.Fibonacci(-1));
    }

    [Theory]
    [InlineData(0, 0L)]
    [InlineData(1, 1L)]
    [InlineData(50, 12_586_269_025L)]
    [InlineData(90, 2_880_067_194_370_816_120L)]
    [InlineData(92, 7_540_113_804_746_346_429L)]
    public void FibonacciIterative_LargeN_ReturnsValue(int n, long expected)
    {
        Assert.Equal(expected, Recursion.FibonacciIterative(n));
    }

    [Fact]
    public void FibonacciIterative_MatchesRecursiveVersion()
    {
        for (var n = 0; n <= 20; n++)
        {
            Assert.Equal(Recursion.Fibonacci(n), Recursion.FibonacciIterative(n));
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(93)]
    public void FibonacciIterative_OutOfRange_Throws(int n)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Recursion.FibonacciIterative(n));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(7, 7)]
    [InlineData(1234, 10)]
    [InlineData(-1234, 10)]
    [InlineData(1_000_000, 1)]
    [InlineData(int.MaxValue, 46)]
    [InlineData(int.MinValue, 47)]
    public void SumDigits_VariousNumbers_ReturnsSum(int n, int expected)
    {
        Assert.Equal(expected, Recursion.SumDigits(n));
    }

    [Fact]
    public void Hanoi_ZeroDisks_ReturnsNoMoves()
    {
        Assert.Empty(Recursion.Hanoi(0));
    }

    [Fact]
    public void Hanoi_OneDisk_MovesDirectly()
    {
        Assert.Equal(new[] { "A->C" }, Recursion.Hanoi(1));
    }

    [Fact]
    public void Hanoi_TwoDisks_ReturnsThreeMoves()
    {
        Assert.Equal(new[] { "A->B", "A->C", "B->C" }, Recursion.Hanoi(2));
    }

    [Fact]
    public void Hanoi_ThreeDisks_ReturnsSevenMovesInOrder()
    {
        string[] expected = ["A->C", "A->B", "C->B", "A->C", "B->A", "B->C", "A->C"];

        Assert.Equal(expected, Recursion.Hanoi(3));
    }

    [Theory]
    [InlineData(5, 31)]
    [InlineData(10, 1023)]
    public void Hanoi_NDisks_Returns2PowNMinus1Moves(int disks, int expectedMoves)
    {
        Assert.Equal(expectedMoves, Recursion.Hanoi(disks).Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Hanoi_OutOfRange_Throws(int disks)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Recursion.Hanoi(disks));
    }
}
