namespace Module03.Tasks;

/// <summary>
/// Задача 03.4 — параметры методов: ref, out, params, optional/named, overloading.
/// Тесты: dotnet test --filter "FullyQualifiedName~MethodBasicsTests"
/// </summary>
public static class MethodBasics
{
    /// <summary>Меняет значения двух переменных местами (ref — изменения видны вызывающему коду).</summary>
    public static void Swap(ref int a, ref int b)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сумма произвольного количества чисел: Sum() → 0, Sum(1, 2, 3) → 6, Sum(array) → сумма массива.
    /// numbers == null (явно переданный null) → ArgumentNullException.
    /// </summary>
    public static int Sum(params int[] numbers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Минимум и максимум за ОДИН проход, через два out-параметра.
    /// numbers == null → ArgumentNullException; пустой массив → ArgumentException.
    /// </summary>
    public static void MinMax(int[] numbers, out int min, out int max)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Форматирует цену: число с <paramref name="decimals"/> знаками после ТОЧКИ, пробел, валюта.
    /// FormatPrice(12.5m) → "12.50 EUR"; FormatPrice(12.5m, "USD") → "12.50 USD";
    /// FormatPrice(3m, decimals: 0) → "3 EUR". Используйте CultureInfo.InvariantCulture.
    /// decimals вне 0–4 → ArgumentOutOfRangeException. currency null или пустая → ArgumentException.
    /// </summary>
    public static string FormatPrice(decimal amount, string currency = "EUR", int decimals = 2)
    {
        throw new NotImplementedException();
    }

    /// <summary>Площадь круга радиуса radius (Math.PI * r²). radius &lt; 0 → ArgumentOutOfRangeException.</summary>
    public static double Area(double radius)
    {
        throw new NotImplementedException();
    }

    /// <summary>Площадь прямоугольника width × height. Отрицательная сторона → ArgumentOutOfRangeException.</summary>
    public static double Area(double width, double height)
    {
        throw new NotImplementedException();
    }
}
