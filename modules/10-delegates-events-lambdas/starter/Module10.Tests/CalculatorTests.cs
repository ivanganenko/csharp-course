using Module10.Tasks;

namespace Module10.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(2.0, "+", 3.0, 5.0)]
    [InlineData(2.0, "-", 3.0, -1.0)]
    [InlineData(2.5, "*", 4.0, 10.0)]
    [InlineData(10.0, "/", 4.0, 2.5)]
    public void Calculate_BuiltInOperations(double left, string operation, double right, double expected)
    {
        var calculator = new Calculator();

        Assert.Equal(expected, calculator.Calculate(left, operation, right), precision: 10);
    }

    [Fact]
    public void Calculate_DivideByZero_ThrowsDivideByZeroException()
    {
        var calculator = new Calculator();

        Assert.Throws<DivideByZeroException>(() => calculator.Calculate(1, "/", 0));
    }

    [Fact]
    public void Calculate_UnknownOperation_ThrowsNotSupportedException()
    {
        var calculator = new Calculator();

        Assert.Throws<NotSupportedException>(() => calculator.Calculate(1, "%", 2));
    }

    [Fact]
    public void SupportedOperations_ContainsBuiltIns()
    {
        var calculator = new Calculator();

        Assert.Equal(4, calculator.SupportedOperations.Count);
        Assert.Contains("+", calculator.SupportedOperations);
        Assert.Contains("/", calculator.SupportedOperations);
    }

    [Fact]
    public void Register_MethodGroup_AddsOperation()
    {
        var calculator = new Calculator();

        calculator.Register("pow", Math.Pow);

        Assert.Equal(1024, calculator.Calculate(2, "pow", 10));
        Assert.Contains("pow", calculator.SupportedOperations);
    }

    [Fact]
    public void Register_Lambda_AddsOperation()
    {
        var calculator = new Calculator();

        calculator.Register("avg", (a, b) => (a + b) / 2);

        Assert.Equal(5, calculator.Calculate(4, "avg", 6));
    }

    [Fact]
    public void Register_ExistingOperation_ThrowsArgumentException()
    {
        var calculator = new Calculator();

        Assert.Throws<ArgumentException>(() => calculator.Register("+", (a, b) => a * b));
        Assert.Equal(5, calculator.Calculate(2, "+", 3)); // старая операция не перезаписана
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Register_InvalidName_ThrowsArgumentException(string? name)
    {
        var calculator = new Calculator();

        Assert.ThrowsAny<ArgumentException>(() => calculator.Register(name!, (a, b) => a));
    }

    [Fact]
    public void Register_NullFunction_ThrowsArgumentNullException()
    {
        var calculator = new Calculator();

        Assert.Throws<ArgumentNullException>(() => calculator.Register("x", null!));
    }

    [Theory]
    [InlineData("2 + 3", 5.0)]
    [InlineData("10 / 4", 2.5)]
    [InlineData("1.5 * 2", 3.0)]
    [InlineData("-2 * 3", -6.0)]
    [InlineData("  7   -   2 ", 5.0)]
    public void Evaluate_ParsesAndCalculates(string expression, double expected)
    {
        var calculator = new Calculator();

        Assert.Equal(expected, calculator.Evaluate(expression), precision: 10);
    }

    [Fact]
    public void Evaluate_UsesRegisteredOperations()
    {
        var calculator = new Calculator();
        calculator.Register("max", Math.Max);

        Assert.Equal(7, calculator.Evaluate("3 max 7"));
    }

    [Theory]
    [InlineData("2 +")]
    [InlineData("2 + 3 + 4")]
    [InlineData("abc + 1")]
    [InlineData("")]
    public void Evaluate_InvalidFormat_ThrowsFormatException(string expression)
    {
        var calculator = new Calculator();

        Assert.Throws<FormatException>(() => calculator.Evaluate(expression));
    }

    [Fact]
    public void Evaluate_UnknownOperation_ThrowsNotSupportedException()
    {
        var calculator = new Calculator();

        Assert.Throws<NotSupportedException>(() => calculator.Evaluate("2 ^ 3"));
    }

    [Fact]
    public void Evaluate_Null_ThrowsArgumentNullException()
    {
        var calculator = new Calculator();

        Assert.Throws<ArgumentNullException>(() => calculator.Evaluate(null!));
    }
}
