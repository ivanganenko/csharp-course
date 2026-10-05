namespace Module01.Tasks;

/// <summary>
/// Задача 01.5 — конвертер единиц для консольной утилиты.
/// Тесты: dotnet test --filter "FullyQualifiedName~UnitConverterTests"
/// </summary>
public static class UnitConverter
{
    /// <summary>Абсолютный ноль в градусах Цельсия.</summary>
    public const double AbsoluteZeroCelsius = -273.15;

    /// <summary>Абсолютный ноль в градусах Фаренгейта.</summary>
    public const double AbsoluteZeroFahrenheit = -459.67;

    /// <summary>Километров в одной миле.</summary>
    public const double KilometersPerMile = 1.609344;

    /// <summary>
    /// °C → °F по формуле F = C * 9 / 5 + 32.
    /// Если celsius &lt; AbsoluteZeroCelsius → ArgumentOutOfRangeException (ровно -273.15 — допустимо).
    /// </summary>
    public static double CelsiusToFahrenheit(double celsius)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// °F → °C по формуле C = (F - 32) * 5 / 9.
    /// Если fahrenheit &lt; AbsoluteZeroFahrenheit → ArgumentOutOfRangeException.
    /// </summary>
    public static double FahrenheitToCelsius(double fahrenheit)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Километры → мили (km / KilometersPerMile). Отрицательное расстояние → ArgumentOutOfRangeException.
    /// </summary>
    public static double KilometersToMiles(double kilometers)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Форматирует температуру для вывода в консоль: одна цифра после ТОЧКИ и суффикс " °C".
    /// 21.456 → "21.5 °C", 100 → "100.0 °C".
    /// Важно: результат не должен зависеть от языковых настроек Windows
    /// (на русской Windows обычный ToString("F1") даст "21,5") — используйте CultureInfo.InvariantCulture.
    /// </summary>
    public static string FormatCelsius(double celsius)
    {
        throw new NotImplementedException();
    }
}
