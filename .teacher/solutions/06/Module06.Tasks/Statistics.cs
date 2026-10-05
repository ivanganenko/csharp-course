namespace Module06.Tasks;

public static class Statistics
{
    // Кортеж вместо out-параметров или отдельного класса: для приватного/локального результата
    // из 2–3 значений это короче и читается лучше. Для публичного API со смыслом лучше record.
    public static (int Min, int Max) MinMax(IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        // Один проход вместо values.Min() + values.Max() (два прохода).
        int min = values[0];
        int max = values[0];
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] < min)
            {
                min = values[i];
            }

            if (values[i] > max)
            {
                max = values[i];
            }
        }

        return (min, max);
    }

    public static (int Count, double Average) CountAndAverage(IReadOnlyList<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count == 0)
        {
            return (0, 0.0);
        }

        // long: сумма двух int.MaxValue переполнила бы int (unchecked по умолчанию → отрицательное число).
        long sum = 0;
        foreach (var value in values)
        {
            sum += value;
        }

        return (values.Count, (double)sum / values.Count);
    }

    public static (string, int) Swap((int Number, string Text) pair)
    {
        // Деконструкция кортежа в переменные.
        var (number, text) = pair;
        return (text, number);
    }
}
