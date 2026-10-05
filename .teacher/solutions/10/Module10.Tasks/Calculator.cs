using System.Globalization;

namespace Module10.Tasks;

public class Calculator
{
    // Словарь «имя → делегат» вместо switch: добавление операции не требует менять код Calculate
    // (Open/Closed principle). Ordinal — имена операций сравниваются как точные строки.
    private readonly Dictionary<string, Func<double, double, double>> _operations = new(StringComparer.Ordinal);

    public Calculator()
    {
        _operations.Add("+", (a, b) => a + b);
        _operations.Add("-", (a, b) => a - b);
        _operations.Add("*", (a, b) => a * b);
        // Для double деление на 0 даёт Infinity/NaN без исключения — по условию нужно исключение,
        // поэтому проверяем явно. throw-выражение внутри ?: — компактно.
        _operations.Add("/", (a, b) => b == 0 ? throw new DivideByZeroException() : a / b);
    }

    // KeyCollection реализует IReadOnlyCollection<string>: отдаём представление без копирования.
    public IReadOnlyCollection<string> SupportedOperations => _operations.Keys;

    public double Calculate(double left, string operation, double right)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (!_operations.TryGetValue(operation, out Func<double, double, double>? function))
        {
            throw new NotSupportedException($"Operation '{operation}' is not supported.");
        }

        return function(left, right);
    }

    public void Register(string operation, Func<double, double, double> function)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        ArgumentNullException.ThrowIfNull(function);

        // TryAdd + явное ArgumentException с понятным сообщением вместо исключения из Dictionary.Add:
        // тип тот же, но сообщение говорит об операции, а не о «ключе словаря».
        if (!_operations.TryAdd(operation, function))
        {
            throw new ArgumentException($"Operation '{operation}' is already registered.", nameof(operation));
        }
    }

    public double Evaluate(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        string[] parts = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            throw new FormatException($"Expected 'left operation right', got '{expression}'.");
        }

        // InvariantCulture: "1.5" должно читаться одинаково на любой машине (в ru-RU разделитель — запятая).
        double left = ParseNumber(parts[0]);
        double right = ParseNumber(parts[2]);
        return Calculate(left, parts[1], right);
    }

    private static double ParseNumber(string text)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new FormatException($"'{text}' is not a valid number.");
        }

        return value;
    }
}
