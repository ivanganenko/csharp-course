using Module03.Tasks;

namespace Module03.Tests;

public class ArrayTasksTests
{
    // Многомерные массивы сравниваем вручную: размеры по каждому измерению + каждый элемент.
    private static void AssertMatrixEqual(int[,] expected, int[,] actual)
    {
        Assert.Equal(expected.GetLength(0), actual.GetLength(0));
        Assert.Equal(expected.GetLength(1), actual.GetLength(1));
        for (var r = 0; r < expected.GetLength(0); r++)
        {
            for (var c = 0; c < expected.GetLength(1); c++)
            {
                Assert.Equal(expected[r, c], actual[r, c]);
            }
        }
    }

    [Fact]
    public void RotateClockwise_RectangularMatrix_ReturnsRotated()
    {
        int[,] matrix =
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
        };
        int[,] expected =
        {
            { 4, 1 },
            { 5, 2 },
            { 6, 3 },
        };

        AssertMatrixEqual(expected, ArrayTasks.RotateClockwise(matrix));
    }

    [Fact]
    public void RotateClockwise_SquareMatrix_ReturnsRotated()
    {
        int[,] matrix =
        {
            { 1, 2, 3 },
            { 4, 5, 6 },
            { 7, 8, 9 },
        };
        int[,] expected =
        {
            { 7, 4, 1 },
            { 8, 5, 2 },
            { 9, 6, 3 },
        };

        AssertMatrixEqual(expected, ArrayTasks.RotateClockwise(matrix));
    }

    [Fact]
    public void RotateClockwise_DoesNotModifyOriginal()
    {
        int[,] matrix = { { 1, 2 }, { 3, 4 } };

        var rotated = ArrayTasks.RotateClockwise(matrix);

        Assert.NotSame(matrix, rotated);
        AssertMatrixEqual(new[,] { { 1, 2 }, { 3, 4 } }, matrix);
    }

    [Fact]
    public void RotateClockwise_FourTimes_ReturnsOriginal()
    {
        int[,] matrix = { { 1, 2, 3 }, { 4, 5, 6 } };

        var result = matrix;
        for (var i = 0; i < 4; i++)
        {
            result = ArrayTasks.RotateClockwise(result);
        }

        AssertMatrixEqual(matrix, result);
    }

    [Fact]
    public void RotateClockwise_EmptyMatrix_ReturnsEmpty()
    {
        var result = ArrayTasks.RotateClockwise(new int[0, 0]);

        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void RotateClockwise_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ArrayTasks.RotateClockwise(null!));
    }

    [Fact]
    public void TryFind_ValuePresent_ReturnsFirstPositionRowByRow()
    {
        int[,] matrix =
        {
            { 5, 1, 7 },
            { 7, 2, 9 },
        };

        var found = ArrayTasks.TryFind(matrix, 7, out var row, out var col);

        Assert.True(found);
        Assert.Equal(0, row);
        Assert.Equal(2, col);
    }

    [Fact]
    public void TryFind_ValueAbsent_ReturnsFalseAndMinusOne()
    {
        int[,] matrix = { { 1, 2 }, { 3, 4 } };

        var found = ArrayTasks.TryFind(matrix, 42, out var row, out var col);

        Assert.False(found);
        Assert.Equal(-1, row);
        Assert.Equal(-1, col);
    }

    [Fact]
    public void PascalTriangle_FiveRows_ReturnsJaggedArray()
    {
        var triangle = ArrayTasks.PascalTriangle(5);

        Assert.Equal(5, triangle.Length);
        Assert.Equal(new[] { 1 }, triangle[0]);
        Assert.Equal(new[] { 1, 1 }, triangle[1]);
        Assert.Equal(new[] { 1, 2, 1 }, triangle[2]);
        Assert.Equal(new[] { 1, 3, 3, 1 }, triangle[3]);
        Assert.Equal(new[] { 1, 4, 6, 4, 1 }, triangle[4]);
    }

    [Fact]
    public void PascalTriangle_ThirtyRows_LastRowMiddleIsCorrect()
    {
        var triangle = ArrayTasks.PascalTriangle(30);

        Assert.Equal(30, triangle[29].Length);
        Assert.Equal(77_558_760, triangle[29][14]); // C(29, 14)
    }

    [Fact]
    public void PascalTriangle_Zero_ReturnsEmpty()
    {
        Assert.Empty(ArrayTasks.PascalTriangle(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public void PascalTriangle_OutOfRange_Throws(int rows)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ArrayTasks.PascalTriangle(rows));
    }

    [Theory]
    [InlineData(2, new[] { 4, 5, 1, 2, 3 })]
    [InlineData(7, new[] { 4, 5, 1, 2, 3 })]
    [InlineData(0, new[] { 1, 2, 3, 4, 5 })]
    [InlineData(5, new[] { 1, 2, 3, 4, 5 })]
    [InlineData(-1, new[] { 2, 3, 4, 5, 1 })]
    [InlineData(-6, new[] { 2, 3, 4, 5, 1 })]
    public void RotateRight_VariousK_ReturnsShiftedCopy(int k, int[] expected)
    {
        int[] array = [1, 2, 3, 4, 5];

        var result = ArrayTasks.RotateRight(array, k);

        Assert.Equal(expected, result);
        Assert.NotSame(array, result);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, array); // исходный массив не изменился
    }

    [Fact]
    public void RotateRight_Empty_ReturnsEmpty()
    {
        Assert.Empty(ArrayTasks.RotateRight([], 3));
    }

    [Fact]
    public void RotateRight_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ArrayTasks.RotateRight(null!, 1));
    }
}
