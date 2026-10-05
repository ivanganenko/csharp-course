using Module01.Tasks;

namespace Module01.Tests;

public class ExpressionCalculatorTests
{
    [Theory]
    [InlineData("2 + 3", 5.0)]
    [InlineData("7", 7.0)]
    [InlineData("  7  ", 7.0)]
    [InlineData("10 / 4", 2.5)]
    [InlineData("1.5 * 2", 3.0)]
    [InlineData("-3 + 5", 2.0)]
    [InlineData("2   *   3", 6.0)]
    public void Evaluate_SimpleExpressions_ReturnsResult(string expression, double expected)
    {
        Assert.Equal(expected, ExpressionCalculator.Evaluate(expression));
    }

    [Theory]
    [InlineData("2 + 3 * 4", 14.0)]
    [InlineData("2 * 3 + 4 * 5", 26.0)]
    [InlineData("20 - 10 / 2", 15.0)]
    public void Evaluate_MultiplicationBeforeAddition_RespectsPrecedence(string expression, double expected)
    {
        Assert.Equal(expected, ExpressionCalculator.Evaluate(expression));
    }

    [Theory]
    [InlineData("8 - 2 - 1", 5.0)]
    [InlineData("10 / 4 * 2", 5.0)]
    [InlineData("100 / 10 / 5", 2.0)]
    public void Evaluate_SamePrecedence_EvaluatesLeftToRight(string expression, double expected)
    {
        Assert.Equal(expected, ExpressionCalculator.Evaluate(expression));
    }

    [Fact]
    public void Evaluate_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ExpressionCalculator.Evaluate(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("2 +")]
    [InlineData("+ 2")]
    [InlineData("2 2")]
    [InlineData("2 ^ 3")]
    [InlineData("abc")]
    [InlineData("2 + x")]
    public void Evaluate_InvalidExpression_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => ExpressionCalculator.Evaluate(expression));
    }

    [Theory]
    [InlineData("1 / 0")]
    [InlineData("5 + 6 / 0")]
    public void Evaluate_DivisionByZero_ThrowsDivideByZeroException(string expression)
    {
        Assert.Throws<DivideByZeroException>(() => ExpressionCalculator.Evaluate(expression));
    }
}
