namespace Module03.Tasks;

/// <summary>
/// Задача 03.6 — рекурсия и стек вызовов.
/// Тесты: dotnet test --filter "FullyQualifiedName~RecursionTests"
/// </summary>
public static class Recursion
{
    /// <summary>
    /// n! РЕКУРСИВНО: 0! = 1, n! = n · (n-1)!.
    /// n &lt; 0 → ArgumentOutOfRangeException. Если результат не помещается в long (n ≥ 21) → OverflowException.
    /// </summary>
    public static long Factorial(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// n-е число Фибоначчи наивной рекурсией: F(0) = 0, F(1) = 1, F(n) = F(n-1) + F(n-2).
    /// n &lt; 0 → ArgumentOutOfRangeException. Тесты вызывают этот метод только для n ≤ 25.
    /// </summary>
    public static long Fibonacci(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// То же число Фибоначчи, но ИТЕРАТИВНО (циклом), за O(n) и O(1) памяти.
    /// Должно работать для n до 92 включительно (F(92) — максимальное, что помещается в long).
    /// ⚠️ Не вызывайте здесь рекурсивный Fibonacci — тест с n = 92 «зависнет» на годы.
    /// n &lt; 0 или n &gt; 92 → ArgumentOutOfRangeException.
    /// </summary>
    public static long FibonacciIterative(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сумма цифр числа РЕКУРСИВНО: 1234 → 10, -1234 → 10 (знак игнорируется), 0 → 0.
    /// Должна корректно работать и для int.MinValue (→ 47). Подсказка: Math.Abs(int.MinValue) бросает OverflowException.
    /// </summary>
    public static int SumDigits(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ханойская башня: список ходов, чтобы перенести <paramref name="disks"/> дисков со стержня 'A' на 'C'
    /// через 'B'. Ход записывается строкой "A->C". Для 2 дисков: ["A->B", "A->C", "B->C"].
    /// disks == 0 → пустой список. disks &lt; 0 или disks &gt; 20 → ArgumentOutOfRangeException.
    /// Подсказка: удобно использовать локальную рекурсивную функцию.
    /// </summary>
    public static List<string> Hanoi(int disks)
    {
        throw new NotImplementedException();
    }
}
