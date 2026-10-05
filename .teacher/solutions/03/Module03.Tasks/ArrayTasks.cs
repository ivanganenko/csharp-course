namespace Module03.Tasks;

public static class ArrayTasks
{
    private const int MaxPascalRows = 30;

    public static int[,] RotateClockwise(int[,] matrix)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        // GetLength(dimension) — размер по измерению; Length у int[,] — общее число элементов.
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        // Новая матрица, а не поворот «на месте»: на месте можно повернуть только квадратную,
        // а изменение входного аргумента — неожиданный побочный эффект для вызывающего кода.
        var result = new int[cols, rows];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                // Строка r становится столбцом (rows - 1 - r): первая строка уходит в правый столбец.
                result[c, rows - 1 - r] = matrix[r, c];
            }
        }

        return result;
    }

    public static bool TryFind(int[,] matrix, int value, out int row, out int col)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        for (var r = 0; r < matrix.GetLength(0); r++)
        {
            for (var c = 0; c < matrix.GetLength(1); c++)
            {
                if (matrix[r, c] == value)
                {
                    row = r;
                    col = c;
                    return true; // return выходит сразу из обоих циклов — break вышел бы только из внутреннего
                }
            }
        }

        row = -1;
        col = -1;
        return false;
    }

    public static int[][] PascalTriangle(int rows)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rows);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rows, MaxPascalRows); // дальше значения не влезут в int

        // Jagged array (массив массивов): строки РАЗНОЙ длины — то, что int[,] не умеет.
        var triangle = new int[rows][];
        for (var i = 0; i < rows; i++)
        {
            var row = new int[i + 1];
            row[0] = 1;
            row[i] = 1;
            for (var j = 1; j < i; j++)
            {
                row[j] = triangle[i - 1][j - 1] + triangle[i - 1][j];
            }

            triangle[i] = row;
        }

        return triangle;
    }

    public static int[] RotateRight(int[] array, int k)
    {
        ArgumentNullException.ThrowIfNull(array);

        var n = array.Length;
        var result = new int[n];
        if (n == 0)
        {
            return result; // и защита от деления на ноль в k % n
        }

        // Нормализуем k в диапазон [0, n): % в C# сохраняет знак делимого (-1 % 5 == -1),
        // поэтому для отрицательных k добавляем n. Без нормализации k = 1_000_000 гонял бы цикл зря.
        var shift = k % n;
        if (shift < 0)
        {
            shift += n;
        }

        for (var i = 0; i < n; i++)
        {
            result[(i + shift) % n] = array[i];
        }

        return result;
    }
}
