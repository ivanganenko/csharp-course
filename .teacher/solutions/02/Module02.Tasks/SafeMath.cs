namespace Module02.Tasks;

public static class SafeMath
{
    // По умолчанию арифметика int в C# unchecked: переполнение молча «заворачивает» значение
    // (int.MaxValue + 1 == int.MinValue). Так сделано ради скорости — проверка на каждой операции
    // стоит тактов. Поэтому там, где переполнение — это баг (деньги, счётчики, размеры),
    // включаем проверку явно: checked-выражением, блоком или флагом проекта
    // <CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>.
    public static int AddChecked(int a, int b) => checked(a + b);

    public static bool TryAdd(int a, int b, out int result)
    {
        // Сначала считаем в более широком типе (long) — там переполнения двух int быть не может,
        // а потом проверяем, влезает ли результат. Это дешевле и чище, чем ловить OverflowException:
        // Try-паттерн как раз и нужен, чтобы НЕ использовать исключения для ожидаемых ситуаций.
        long sum = (long)a + b;
        if (sum is < int.MinValue or > int.MaxValue)
        {
            result = 0; // out-параметр обязан быть присвоен на КАЖДОМ пути выхода
            return false;
        }

        result = (int)sum;
        return true;
    }

    public static int Midpoint(int low, int high)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(low);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(low, high);

        // Классический баг: (low + high) / 2 переполняется при больших значениях —
        // знаменитая ошибка в бинарном поиске Java, прожившая в JDK ~9 лет.
        // high - low не переполняется, т.к. 0 ≤ low ≤ high.
        return low + (high - low) / 2;
    }

    // Явное приведение (int)value по умолчанию просто отбрасывает старшие биты —
    // checked превращает молчаливую потерю данных в исключение.
    public static int ToIntChecked(long value) => checked((int)value);

    public static long SumAsLong(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        // Аккумулятор long: int + long → long (implicit widening conversion), переполнения нет.
        // values.Sum() из LINQ вернул бы int и бросил бы OverflowException (LINQ Sum — checked).
        long sum = 0;
        foreach (var value in values)
        {
            sum += value;
        }

        return sum;
    }
}
