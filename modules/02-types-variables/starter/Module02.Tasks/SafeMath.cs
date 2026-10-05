namespace Module02.Tasks;

/// <summary>
/// Задача 02.2 — переполнение целых чисел и checked.
/// Тесты: dotnet test --filter "FullyQualifiedName~SafeMathTests"
/// </summary>
public static class SafeMath
{
    /// <summary>
    /// Возвращает a + b. При переполнении int — OverflowException (а не «тихий» переход через int.MinValue).
    /// </summary>
    public static int AddChecked(int a, int b)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Try-вариант сложения: true и сумма в result, если переполнения нет;
    /// false и result = 0 — если есть. Исключений не бросает.
    /// </summary>
    public static bool TryAdd(int a, int b, out int result)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Середина отрезка [low, high], округлённая вниз: Midpoint(2, 7) → 4.
    /// Должна работать для любых 0 ≤ low ≤ high ≤ int.MaxValue БЕЗ переполнения:
    /// Midpoint(int.MaxValue - 1, int.MaxValue) → int.MaxValue - 1.
    /// low &lt; 0 или low &gt; high → ArgumentOutOfRangeException.
    /// </summary>
    public static int Midpoint(int low, int high)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Приводит long к int. Если значение не помещается в int — OverflowException.
    /// </summary>
    public static int ToIntChecked(long value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сумма элементов массива в long — без переполнения даже для больших int.
    /// Пустой массив → 0. values == null → ArgumentNullException.
    /// </summary>
    public static long SumAsLong(int[] values)
    {
        throw new NotImplementedException();
    }
}
