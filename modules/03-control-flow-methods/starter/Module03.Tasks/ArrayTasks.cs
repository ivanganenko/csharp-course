namespace Module03.Tasks;

/// <summary>
/// Задача 03.5 — массивы: одномерные, многомерные (int[,]) и jagged (int[][]).
/// Тесты: dotnet test --filter "FullyQualifiedName~ArrayTasksTests"
/// </summary>
public static class ArrayTasks
{
    /// <summary>
    /// Поворачивает матрицу на 90° по часовой стрелке и возвращает НОВУЮ матрицу (исходная не меняется).
    /// Матрица rows × cols превращается в cols × rows:
    /// <code>
    /// 1 2 3      4 1
    /// 4 5 6  →   5 2
    ///            6 3
    /// </code>
    /// Пустая матрица (0 × 0) → пустая матрица. matrix == null → ArgumentNullException.
    /// </summary>
    public static int[,] RotateClockwise(int[,] matrix)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ищет value в матрице построчно (сначала вся строка 0 слева направо, потом строка 1 и т.д.).
    /// Найдено → true и координаты ПЕРВОГО вхождения; не найдено → false, row = col = -1.
    /// matrix == null → ArgumentNullException.
    /// </summary>
    public static bool TryFind(int[,] matrix, int value, out int row, out int col)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Треугольник Паскаля из <paramref name="rows"/> строк в виде jagged-массива:
    /// строка i содержит i + 1 элементов, крайние равны 1, остальные — сумма двух чисел над ними.
    /// PascalTriangle(4) → [1], [1,1], [1,2,1], [1,3,3,1]. rows == 0 → пустой массив.
    /// rows &lt; 0 или rows &gt; 30 → ArgumentOutOfRangeException.
    /// </summary>
    public static int[][] PascalTriangle(int rows)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Циклический сдвиг массива вправо на k позиций, результат — НОВЫЙ массив:
    /// RotateRight([1,2,3,4,5], 2) → [4,5,1,2,3]. k может быть больше длины (7 ≡ 2 для длины 5)
    /// и отрицательным (сдвиг влево: -1 → [2,3,4,5,1]). Пустой массив → пустой массив.
    /// array == null → ArgumentNullException.
    /// </summary>
    public static int[] RotateRight(int[] array, int k)
    {
        throw new NotImplementedException();
    }
}
