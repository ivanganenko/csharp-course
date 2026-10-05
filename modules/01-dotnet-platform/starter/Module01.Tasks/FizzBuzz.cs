namespace Module01.Tasks;

/// <summary>
/// Задача 01.2 — разминка: вариант FizzBuzz с тремя правилами.
/// Тесты: dotnet test --filter "FullyQualifiedName~FizzBuzzTests"
/// </summary>
public static class FizzBuzz
{
    /// <summary>
    /// Возвращает строку для числа <paramref name="n"/> (n ≥ 1) по правилам:
    /// <list type="bullet">
    /// <item>кратно 3 → добавить "Fizz";</item>
    /// <item>кратно 5 → добавить "Buzz";</item>
    /// <item>кратно 7 → добавить "Whizz";</item>
    /// <item>если ни одно правило не сработало — само число: 8 → "8".</item>
    /// </list>
    /// Слова склеиваются в этом порядке: 15 → "FizzBuzz", 21 → "FizzWhizz", 35 → "BuzzWhizz", 105 → "FizzBuzzWhizz".
    /// Для n ≤ 0 бросьте ArgumentOutOfRangeException.
    /// </summary>
    public static string ForNumber(int n)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает массив из <paramref name="count"/> элементов: ForNumber(1), ForNumber(2), …, ForNumber(count).
    /// count == 0 → пустой массив. count &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static string[] Sequence(int count)
    {
        throw new NotImplementedException();
    }
}
