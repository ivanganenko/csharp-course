using System.Globalization;

namespace Module03.Tasks;

public static class MethodBasics
{
    private const int MaxDecimals = 4;

    public static void Swap(ref int a, ref int b)
    {
        // Без ref метод получил бы КОПИИ значений, и обмен остался бы внутри метода.
        // Кортежный обмен (a, b) = (b, a) — идиоматичный C# без временной переменной в коде.
        (a, b) = (b, a);
    }

    public static int Sum(params int[] numbers)
    {
        // params — синтаксический сахар: Sum(1, 2, 3) компилятор превращает в Sum(new[] { 1, 2, 3 }),
        // а Sum() — в Sum(пустой массив). Но вызвать Sum(null) всё ещё можно — проверяем.
        ArgumentNullException.ThrowIfNull(numbers);

        var sum = 0;
        foreach (var number in numbers)
        {
            sum += number;
        }

        return sum;
    }

    public static void MinMax(int[] numbers, out int min, out int max)
    {
        ArgumentNullException.ThrowIfNull(numbers);
        if (numbers.Length == 0)
        {
            // У пустого массива нет минимума — возвращать 0 или int.MaxValue было бы враньём.
            throw new ArgumentException("Массив не должен быть пустым.", nameof(numbers));
        }

        // Стартуем с первого элемента, а не с int.MaxValue/int.MinValue:
        // так нет «магических» начальных значений и лишних сравнений.
        min = numbers[0];
        max = numbers[0];
        for (var i = 1; i < numbers.Length; i++)
        {
            if (numbers[i] < min)
            {
                min = numbers[i];
            }
            else if (numbers[i] > max)
            {
                max = numbers[i];
            }
        }
        // Почему out, а не возвращаемый кортеж? Здесь — чтобы потренировать out. В новом коде
        // чаще возвращают (int Min, int Max): кортеж читается проще и не требует объявлять переменные заранее.
    }

    public static string FormatPrice(decimal amount, string currency = "EUR", int decimals = 2)
    {
        // Значения по умолчанию optional-параметров — константы времени компиляции,
        // они «вшиваются» в место вызова (как const). Поэтому менять их в публичной библиотеке опасно.
        ArgumentException.ThrowIfNullOrEmpty(currency);
        ArgumentOutOfRangeException.ThrowIfNegative(decimals);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(decimals, MaxDecimals);

        // "F" + число — стандартный формат с фиксированным количеством знаков.
        var number = amount.ToString("F" + decimals.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        return $"{number} {currency}";
    }

    // Overloading: одно имя — разные списки параметров. Компилятор выбирает перегрузку
    // по количеству и типам аргументов ВО ВРЕМЯ КОМПИЛЯЦИИ (не в runtime).
    // Различаться только возвращаемым типом перегрузки не могут.
    public static double Area(double radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(radius);
        return Math.PI * radius * radius;
    }

    public static double Area(double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);
        return width * height;
    }
}
