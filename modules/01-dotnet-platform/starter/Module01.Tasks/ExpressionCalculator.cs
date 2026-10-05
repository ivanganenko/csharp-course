namespace Module01.Tasks;

/// <summary>
/// Задача 01.7 ⭐ — калькулятор выражений для консольного приложения.
/// Тесты: dotnet test --filter "FullyQualifiedName~ExpressionCalculatorTests"
/// </summary>
public static class ExpressionCalculator
{
    /// <summary>
    /// Вычисляет выражение, в котором числа и операторы разделены пробелами: "2 + 3 * 4" → 14.
    /// <list type="bullet">
    /// <item>Операторы: + - * /. Умножение и деление выполняются раньше сложения и вычитания;
    /// операторы одного приоритета — слева направо: "8 - 2 - 1" → 5, "10 / 4 * 2" → 5.</item>
    /// <item>Числа — double в формате InvariantCulture (разделитель — точка): "1.5 * 2" → 3.
    /// Число может быть отрицательным: "-3 + 5" → 2.</item>
    /// <item>Лишние пробелы по краям и между токенами допустимы: "  7  " → 7.</item>
    /// <item>expression == null → ArgumentNullException.</item>
    /// <item>Пустая строка, строка из пробелов, неизвестный оператор, не-число,
    /// неправильный порядок токенов ("2 +", "+ 2", "2 2") → FormatException.</item>
    /// <item>Деление на 0 → DivideByZeroException (double сам по себе вернул бы бесконечность — нам это не подходит).</item>
    /// </list>
    /// </summary>
    public static double Evaluate(string expression)
    {
        throw new NotImplementedException();
    }
}
