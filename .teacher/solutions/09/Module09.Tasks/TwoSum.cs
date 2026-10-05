namespace Module09.Tasks;

public static class TwoSum
{
    public static (int First, int Second)? Find(int[] numbers, int target)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        // Идея: для каждого numbers[j] нужен «партнёр» target - numbers[j] СЛЕВА от j.
        // Словарь значение → индекс первого появления отвечает на вопрос «был ли партнёр?» за O(1).
        // Итого O(n) по времени и O(n) по памяти — классический обмен памяти на скорость.
        var firstIndexByValue = new Dictionary<int, int>(numbers.Length);

        for (int j = 0; j < numbers.Length; j++)
        {
            // long — чтобы target - numbers[j] не переполнился (например, 0 - int.MinValue).
            long complement = (long)target - numbers[j];

            // Сначала ищем партнёра, потом добавляем текущий элемент — иначе элемент «сложится сам с собой».
            if (complement >= int.MinValue && complement <= int.MaxValue
                && firstIndexByValue.TryGetValue((int)complement, out int i))
            {
                return (i, j);
            }

            // TryAdd не перезаписывает — сохраняем ПЕРВОЕ вхождение (минимальный i по условию).
            firstIndexByValue.TryAdd(numbers[j], j);
        }

        return null;
    }
}
