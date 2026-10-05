using Module01.Tasks;

namespace Module01.Tests;

public class UnitConverterTests
{
    [Theory]
    [InlineData(0.0, 32.0)]
    [InlineData(100.0, 212.0)]
    [InlineData(-40.0, -40.0)]
    [InlineData(37.0, 98.6)]
    public void CelsiusToFahrenheit_KnownValues_Converts(double celsius, double expected)
    {
        // double хранит значения приближённо, поэтому сравниваем с точностью до 6 знаков.
        Assert.Equal(expected, UnitConverter.CelsiusToFahrenheit(celsius), 6);
    }

    [Theory]
    [InlineData(32.0, 0.0)]
    [InlineData(212.0, 100.0)]
    [InlineData(-40.0, -40.0)]
    [InlineData(98.6, 37.0)]
    public void FahrenheitToCelsius_KnownValues_Converts(double fahrenheit, double expected)
    {
        Assert.Equal(expected, UnitConverter.FahrenheitToCelsius(fahrenheit), 6);
    }

    [Fact]
    public void CelsiusToFahrenheit_ExactlyAbsoluteZero_DoesNotThrow()
    {
        var result = UnitConverter.CelsiusToFahrenheit(UnitConverter.AbsoluteZeroCelsius);

        Assert.Equal(-459.67, result, 6);
    }

    [Fact]
    public void CelsiusToFahrenheit_BelowAbsoluteZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UnitConverter.CelsiusToFahrenheit(-300));
    }

    [Fact]
    public void FahrenheitToCelsius_BelowAbsoluteZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UnitConverter.FahrenheitToCelsius(-500));
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(1.609344, 1.0)]
    [InlineData(42.195, 26.218757)]
    public void KilometersToMiles_KnownValues_Converts(double km, double expectedMiles)
    {
        Assert.Equal(expectedMiles, UnitConverter.KilometersToMiles(km), 6);
    }

    [Fact]
    public void KilometersToMiles_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UnitConverter.KilometersToMiles(-1));
    }

    [Theory]
    [InlineData(21.456, "21.5 °C")]
    [InlineData(100.0, "100.0 °C")]
    [InlineData(0.0, "0.0 °C")]
    [InlineData(-12.34, "-12.3 °C")]
    public void FormatCelsius_UsesDotAndOneDecimal(double celsius, string expected)
    {
        Assert.Equal(expected, UnitConverter.FormatCelsius(celsius));
    }
}
