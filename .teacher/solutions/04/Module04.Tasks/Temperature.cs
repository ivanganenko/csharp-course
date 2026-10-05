using System.Globalization;

namespace Module04.Tasks;

public sealed class Temperature
{
    public const double AbsoluteZeroCelsius = -273.15;

    // Единственный источник правды — градусы Цельсия. Fahrenheit/Kelvin вычисляются,
    // поэтому три значения никогда не «разъедутся» между собой.
    private double _celsius;

    public Temperature(double celsius)
    {
        // Присваиваем через свойство, а не в поле: валидация живёт в одном месте (setter).
        Celsius = celsius;
    }

    public double Celsius
    {
        get => _celsius;
        set
        {
            // NaN нужно проверять отдельно: любое сравнение с NaN (в том числе NaN < x) даёт false,
            // и без этой проверки NaN «проскочил» бы валидацию.
            if (double.IsNaN(value) || value < AbsoluteZeroCelsius)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, $"Temperature cannot be below {AbsoluteZeroCelsius} °C.");
            }

            _celsius = value;
        }
    }

    public double Fahrenheit
    {
        get => Celsius * 9 / 5 + 32;
        // Пересчитываем в °C и идём через setter Celsius — получаем ту же валидацию бесплатно.
        set => Celsius = (value - 32) * 5 / 9;
    }

    public double Kelvin => Celsius - AbsoluteZeroCelsius;

    public bool IsFreezing => Celsius <= 0;

    // Статические фабричные методы с говорящими именами понятнее, чем перегрузки конструктора:
    // new Temperature(212) — это °C или °F? А Temperature.FromFahrenheit(212) — однозначно.
    public static Temperature FromFahrenheit(double fahrenheit) => new((fahrenheit - 32) * 5 / 9);

    public static Temperature FromKelvin(double kelvin) => new(kelvin + AbsoluteZeroCelsius);

    // InvariantCulture: иначе на русской Windows получим "21,5 °C", а тесты/логи/API ждут точку.
    public override string ToString() =>
        $"{Celsius.ToString("0.0", CultureInfo.InvariantCulture)} °C";
}
