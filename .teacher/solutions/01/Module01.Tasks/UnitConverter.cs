using System.Globalization;

namespace Module01.Tasks;

public static class UnitConverter
{
    public const double AbsoluteZeroCelsius = -273.15;

    public const double AbsoluteZeroFahrenheit = -459.67;

    public const double KilometersPerMile = 1.609344;

    public static double CelsiusToFahrenheit(double celsius)
    {
        // Проверяем ВХОД, а не результат: сравнение вычисленного значения с константой
        // могло бы ошибиться на последнем бите из-за погрешности double.
        ArgumentOutOfRangeException.ThrowIfLessThan(celsius, AbsoluteZeroCelsius);

        // 9.0 / 5 и 9 / 5 — не одно и то же! 9 / 5 в int даёт 1. Здесь celsius уже double,
        // поэтому всё выражение вычисляется в double, но привычка следить за типами операндов важна.
        return celsius * 9 / 5 + 32;
    }

    public static double FahrenheitToCelsius(double fahrenheit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fahrenheit, AbsoluteZeroFahrenheit);

        return (fahrenheit - 32) * 5 / 9;
    }

    public static double KilometersToMiles(double kilometers)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(kilometers);

        return kilometers / KilometersPerMile;
    }

    public static string FormatCelsius(double celsius)
    {
        // InvariantCulture: формат не зависит от региональных настроек машины.
        // Без него на русской Windows получим "21,5 °C", и тест (и любой парсер логов) сломается.
        // Для текста, который видит пользователь, иногда как раз нужна CurrentCulture —
        // но для машинно-читаемого вывода и тестов — всегда Invariant.
        return celsius.ToString("F1", CultureInfo.InvariantCulture) + " °C";
    }
}
