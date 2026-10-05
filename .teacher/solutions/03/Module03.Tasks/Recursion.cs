namespace Module03.Tasks;

public static class Recursion
{
    private const int MaxFibonacciInLong = 92;
    private const int MaxHanoiDisks = 20;

    public static long Factorial(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        // Базовый случай (base case) обязателен: без него рекурсия идёт до StackOverflowException,
        // которое НЕЛЬЗЯ поймать catch'ем — процесс просто падает.
        if (n <= 1)
        {
            return 1;
        }

        // checked: 21! не влезает в long, и без проверки мы молча получили бы отрицательное «число».
        return checked(n * Factorial(n - 1));
    }

    public static long Fibonacci(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        // Наивная рекурсия: красиво, но O(2^n) — F(n-2) пересчитывается и в ветке F(n-1).
        // F(50) — это уже ~40 млрд вызовов. Оставлено намеренно, для сравнения с итеративной версией.
        return n < 2 ? n : Fibonacci(n - 1) + Fibonacci(n - 2);
    }

    public static long FibonacciIterative(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, MaxFibonacciInLong);

        // Храним только два последних значения: O(n) времени, O(1) памяти, глубина стека не растёт.
        long previous = 0;
        long current = 1;
        if (n == 0)
        {
            return previous;
        }

        for (var i = 2; i <= n; i++)
        {
            (previous, current) = (current, previous + current);
        }

        return current;
    }

    public static int SumDigits(int n)
    {
        // Работаем с остатком, а не с Math.Abs(n): Math.Abs(int.MinValue) бросает OverflowException,
        // потому что +2147483648 не помещается в int. Остаток от деления на 10 всегда в [-9; 9].
        if (n is > -10 and < 10)
        {
            return Math.Abs(n);
        }

        return Math.Abs(n % 10) + SumDigits(n / 10);
    }

    public static List<string> Hanoi(int disks)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(disks);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(disks, MaxHanoiDisks); // 2^20 - 1 ≈ миллион ходов — уже много

        var moves = new List<string>((1 << disks) - 1); // capacity известна заранее: 2^n - 1 ходов
        Move(disks, 'A', 'C', 'B');
        return moves;

        // Локальная функция: видна только внутри Hanoi и захватывает moves — не нужно
        // передавать список параметром или заводить приватный метод, который никто больше не вызывает.
        void Move(int count, char from, char to, char via)
        {
            if (count == 0)
            {
                return;
            }

            Move(count - 1, from, via, to);   // убрать n-1 дисков с дороги
            moves.Add($"{from}->{to}");       // перенести самый большой
            Move(count - 1, via, to, from);   // вернуть n-1 дисков сверху
        }
    }
}
