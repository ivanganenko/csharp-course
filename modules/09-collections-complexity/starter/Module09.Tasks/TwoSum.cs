namespace Module09.Tasks;

/// <summary>
/// Задача 09.3 — классическая задача с собеседований «Two Sum».
/// Тесты: TwoSumTests.
/// </summary>
public static class TwoSum
{
    /// <summary>
    /// Ищет два РАЗНЫХ индекса i &lt; j, для которых numbers[i] + numbers[j] == target.
    /// Если пар несколько — вернуть пару с минимальным j, а среди них — с минимальным i
    /// (это естественный результат одного прохода слева направо, если для каждого значения
    /// помнить индекс его ПЕРВОГО появления).
    /// Если пары нет — null. null вместо массива → <see cref="ArgumentNullException"/>.
    /// Требование: O(n) по времени. Решение двумя вложенными циклами O(n²) тесты пропустят,
    /// но задача на /check не будет принята.
    /// </summary>
    public static (int First, int Second)? Find(int[] numbers, int target)
    {
        throw new NotImplementedException();
    }
}
