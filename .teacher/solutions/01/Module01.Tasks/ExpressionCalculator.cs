using System.Globalization;

namespace Module01.Tasks;

public static class ExpressionCalculator
{
    public static double Evaluate(string expression)
    {
        // null — ошибка программиста (ArgumentNullException), а пустая строка — некорректный
        // пользовательский ввод (FormatException). Разные причины — разные исключения.
        ArgumentNullException.ThrowIfNull(expression);

        // Split без аргументов-разделителей (null) режет по любым whitespace,
        // RemoveEmptyEntries убирает «пустые токены» от двойных пробелов.
        var tokens = expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        // Корректное выражение: число (оператор число)* — значит, токенов всегда нечётное число.
        if (tokens.Length == 0 || tokens.Length % 2 == 0)
        {
            throw new FormatException($"Некорректное выражение: '{expression}'.");
        }

        // Идея без стека и рекурсии: копим сумму (total) и текущее слагаемое (term).
        // * и / сразу применяются к term (у них выше приоритет), а + и - «закрывают» term:
        // прибавляем его к total и начинаем новый. Один проход, O(n).
        double total = 0;
        var term = ParseNumber(tokens[0]);

        for (var i = 1; i < tokens.Length; i += 2)
        {
            var op = tokens[i];
            var number = ParseNumber(tokens[i + 1]);

            switch (op)
            {
                case "*":
                    term *= number;
                    break;
                case "/":
                    // double при делении на 0 вернул бы Infinity без исключения —
                    // для калькулятора это скрытая ошибка, поэтому проверяем явно.
                    if (number == 0)
                    {
                        throw new DivideByZeroException("Деление на ноль.");
                    }

                    term /= number;
                    break;
                case "+":
                    total += term;
                    term = number;
                    break;
                case "-":
                    // Вычитание = прибавление отрицательного слагаемого.
                    // Так "8 - 2 - 1" корректно считается слева направо: 8 + (-2) + (-1).
                    total += term;
                    term = -number;
                    break;
                default:
                    throw new FormatException($"Неизвестный оператор '{op}'.");
            }
        }

        return total + term;
    }

    private static double ParseNumber(string token)
    {
        // TryParse вместо Parse + try/catch: некорректный ввод — ожидаемая ситуация,
        // исключения дорогие и не для управления потоком. Ошибку бросаем сами — со смыслом.
        // InvariantCulture: "1.5" должно читаться одинаково на любой Windows.
        if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new FormatException($"'{token}' — не число.");
        }

        return value;
    }
}
