namespace Module10.Tasks;

/// <summary>
/// Задача 10.2 — калькулятор, в котором операции — это делегаты в словаре
/// <c>Dictionary&lt;string, Func&lt;double, double, double&gt;&gt;</c>, а не огромный switch.
/// Новую операцию можно добавить без изменения кода калькулятора (принцип Open/Closed).
/// Тесты: CalculatorTests.
/// </summary>
public class Calculator
{
    /// <summary>
    /// Создаёт калькулятор с четырьмя встроенными операциями: "+", "-", "*", "/".
    /// Деление на 0 должно бросать <see cref="DivideByZeroException"/> (а не возвращать Infinity).
    /// </summary>
    public Calculator()
    {
        throw new NotImplementedException();
    }

    /// <summary>Имена всех доступных операций.</summary>
    public IReadOnlyCollection<string> SupportedOperations => throw new NotImplementedException();

    /// <summary>
    /// Применяет операцию: Calculate(2, "+", 3) → 5.
    /// Неизвестная операция → <see cref="NotSupportedException"/>.
    /// </summary>
    public double Calculate(double left, string operation, double right)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Регистрирует новую операцию, например Register("pow", Math.Pow).
    /// operation null/пустая/из пробелов → <see cref="ArgumentException"/> (или наследник);
    /// function == null → <see cref="ArgumentNullException"/>;
    /// операция с таким именем уже есть → <see cref="ArgumentException"/>.
    /// </summary>
    public void Register(string operation, Func<double, double, double> function)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Вычисляет выражение вида "левое операция правое", разделённое пробелами: "2 + 3", "10 / 4", "2 pow 10".
    /// Числа — в инвариантной культуре (десятичный разделитель — точка): "1.5 * 2".
    /// Лишние пробелы между частями допустимы. Не три части или некорректное число → <see cref="FormatException"/>.
    /// Неизвестная операция → <see cref="NotSupportedException"/>. null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public double Evaluate(string expression)
    {
        throw new NotImplementedException();
    }
}
