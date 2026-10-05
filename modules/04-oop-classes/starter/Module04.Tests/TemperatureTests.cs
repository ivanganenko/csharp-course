using Module04.Tasks;

namespace Module04.Tests;

public class TemperatureTests
{
    [Fact]
    public void Constructor_ValidValue_SetsCelsius()
    {
        var t = new Temperature(21.5);

        Assert.Equal(21.5, t.Celsius, 10);
    }

    [Theory]
    [InlineData(-273.16)]
    [InlineData(-300)]
    [InlineData(double.NaN)]
    public void Constructor_InvalidValue_ThrowsArgumentOutOfRange(double celsius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Temperature(celsius));
    }

    [Fact]
    public void Constructor_AbsoluteZero_IsAllowed()
    {
        var t = new Temperature(Temperature.AbsoluteZeroCelsius);

        Assert.Equal(0.0, t.Kelvin, 10);
    }

    [Fact]
    public void Celsius_SetInvalidValue_ThrowsAndKeepsOldValue()
    {
        var t = new Temperature(10);

        Assert.Throws<ArgumentOutOfRangeException>(() => t.Celsius = -500);
        Assert.Throws<ArgumentOutOfRangeException>(() => t.Celsius = double.NaN);
        Assert.Equal(10.0, t.Celsius, 10);
    }

    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    [InlineData(37, 98.6)]
    public void Fahrenheit_Get_ConvertsFromCelsius(double celsius, double expectedFahrenheit)
    {
        var t = new Temperature(celsius);

        Assert.Equal(expectedFahrenheit, t.Fahrenheit, 6);
    }

    [Fact]
    public void Fahrenheit_Set_UpdatesCelsius()
    {
        var t = new Temperature(0);

        t.Fahrenheit = 212;

        Assert.Equal(100.0, t.Celsius, 6);
    }

    [Fact]
    public void Fahrenheit_SetBelowAbsoluteZero_ThrowsAndKeepsOldValue()
    {
        var t = new Temperature(20);

        Assert.Throws<ArgumentOutOfRangeException>(() => t.Fahrenheit = -500);
        Assert.Equal(20.0, t.Celsius, 10);
    }

    [Theory]
    [InlineData(0, 273.15)]
    [InlineData(100, 373.15)]
    public void Kelvin_ConvertsFromCelsius(double celsius, double expectedKelvin)
    {
        Assert.Equal(expectedKelvin, new Temperature(celsius).Kelvin, 6);
    }

    [Fact]
    public void FactoryMethods_CreateEquivalentTemperatures()
    {
        Assert.Equal(100.0, Temperature.FromFahrenheit(212).Celsius, 6);
        Assert.Equal(-40.0, Temperature.FromFahrenheit(-40).Celsius, 6);
        Assert.Equal(0.0, Temperature.FromKelvin(273.15).Celsius, 6);
        Assert.Throws<ArgumentOutOfRangeException>(() => Temperature.FromKelvin(-1));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(-5, true)]
    [InlineData(0.1, false)]
    public void IsFreezing_DependsOnCelsius(double celsius, bool expected)
    {
        Assert.Equal(expected, new Temperature(celsius).IsFreezing);
    }

    [Theory]
    [InlineData(21.5, "21.5 °C")]
    [InlineData(0, "0.0 °C")]
    [InlineData(-40, "-40.0 °C")]
    public void ToString_UsesOneDecimalAndInvariantCulture(double celsius, string expected)
    {
        Assert.Equal(expected, new Temperature(celsius).ToString());
    }
}
