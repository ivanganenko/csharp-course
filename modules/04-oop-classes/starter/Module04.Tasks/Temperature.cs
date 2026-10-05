namespace Module04.Tasks;

/// <summary>
/// Задача 04.2 — температура с валидацией в свойстве.
/// Хранится ОДНО значение — в градусах Цельсия. Fahrenheit и Kelvin — вычисляемые свойства (computed properties).
/// Все проверки — в одном месте (setter свойства Celsius), остальные члены используют его.
/// </summary>
public sealed class Temperature
{
    /// <summary>Абсолютный ноль в °C. Ниже температура быть не может.</summary>
    public const double AbsoluteZeroCelsius = -273.15;

    /// <summary>
    /// Создаёт температуру в °C. Значение ниже AbsoluteZeroCelsius или NaN → ArgumentOutOfRangeException.
    /// </summary>
    public Temperature(double celsius)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Температура в °C. Setter проверяет значение: ниже AbsoluteZeroCelsius или double.NaN →
    /// ArgumentOutOfRangeException, а старое значение остаётся.
    /// Подсказка: понадобится private-поле (backing field).
    /// </summary>
    public double Celsius
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <summary>
    /// Температура в °F: F = C * 9 / 5 + 32. Setter переводит значение в °C и присваивает Celsius
    /// (а значит, проходит ту же валидацию).
    /// </summary>
    public double Fahrenheit
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <summary>Температура в кельвинах: K = C - AbsoluteZeroCelsius (т.е. C + 273.15). Только чтение.</summary>
    public double Kelvin => throw new NotImplementedException();

    /// <summary>true, если Celsius &lt;= 0.</summary>
    public bool IsFreezing => throw new NotImplementedException();

    /// <summary>Фабричный метод: создать температуру из °F. Валидация — как в конструкторе.</summary>
    public static Temperature FromFahrenheit(double fahrenheit)
    {
        throw new NotImplementedException();
    }

    /// <summary>Фабричный метод: создать температуру из кельвинов. Валидация — как в конструкторе.</summary>
    public static Temperature FromKelvin(double kelvin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Формат: один знак после запятой, точка как разделитель (независимо от культуры ОС), затем " °C".
    /// 21.5 → "21.5 °C", 0 → "0.0 °C", -40 → "-40.0 °C".
    /// Подсказка: ToString("0.0", CultureInfo.InvariantCulture) и using System.Globalization.
    /// </summary>
    public override string ToString()
    {
        throw new NotImplementedException();
    }
}
