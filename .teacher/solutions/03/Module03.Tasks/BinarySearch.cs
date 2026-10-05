namespace Module03.Tasks;

public static class BinarySearch
{
    public static int IndexOf(int[] sorted, int target)
    {
        ArgumentNullException.ThrowIfNull(sorted);

        // Инвариант: если target есть в массиве, он в отрезке [low, high] (обе границы включительно).
        var low = 0;
        var high = sorted.Length - 1;

        while (low <= high)
        {
            // НЕ (low + high) / 2: на массивах > 1 млрд элементов сумма переполняет int
            // (баг прожил в java.util.Arrays.binarySearch ~9 лет).
            var mid = low + (high - low) / 2;

            if (sorted[mid] == target)
            {
                return mid;
            }

            if (sorted[mid] < target)
            {
                low = mid + 1;   // +1 / -1 обязательны: иначе при low == high цикл бесконечен
            }
            else
            {
                high = mid - 1;
            }
        }

        return -1;
    }

    public static int LowerBound(int[] sorted, int target)
    {
        ArgumentNullException.ThrowIfNull(sorted);
        return FirstIndexWhere(sorted, value => value >= target);
    }

    public static int CountOccurrences(int[] sorted, int target)
    {
        ArgumentNullException.ThrowIfNull(sorted);

        // Upper bound — первый элемент > target. Через LowerBound(target + 1) было бы короче,
        // но при target == int.MaxValue сложение переполнится. Сравнение `>` переполнения не боится.
        var first = FirstIndexWhere(sorted, value => value >= target);
        var afterLast = FirstIndexWhere(sorted, value => value > target);
        return afterLast - first;
    }

    public static int IntegerSqrt(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        // Бинарный поиск «по ответу»: ищем наибольшее x с x*x <= n в диапазоне [0, n].
        // mid * mid считаем в long: 46341² уже не влезает в int.
        var low = 0;
        var high = n;
        var answer = 0;

        while (low <= high)
        {
            var mid = low + (high - low) / 2;
            if ((long)mid * mid <= n)
            {
                answer = mid;      // mid подходит — запоминаем и пробуем больше
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return answer;
    }

    // Общий шаблон «первый индекс, где условие истинно» для монотонного условия
    // (false…false true…true). Полуинтервал [low, high): high = Length означает «не нашли».
    // Func<int, bool> — делегат (подробно в модуле 10); здесь он убирает дублирование двух почти одинаковых циклов.
    private static int FirstIndexWhere(int[] sorted, Func<int, bool> condition)
    {
        var low = 0;
        var high = sorted.Length;

        while (low < high)
        {
            var mid = low + (high - low) / 2;
            if (condition(sorted[mid]))
            {
                high = mid;        // mid может быть ответом — не исключаем его
            }
            else
            {
                low = mid + 1;
            }
        }

        return low;
    }
}
