using System.Globalization;

namespace Module14.Tasks;

// Итог TDD-каты после нескольких циклов red-green-refactor.
// Каждый метод появился как «рефакторинг» после зелёного теста: сначала всё было в Add.
public static class StringCalculator
{
    private const int MaxValue = 1000;

    public static int Add(string? numbers)
    {
        if (string.IsNullOrEmpty(numbers))
        {
            return 0;
        }

        var (delimiters, body) = ParseHeader(numbers);
        if (body.Length == 0)
        {
            return 0;
        }

        var values = body
            .Split(delimiters)
            .Select(part => int.Parse(part, NumberStyles.Integer, CultureInfo.InvariantCulture))
            .ToList(); // материализуем: значения проходим дважды (отрицательные + сумма)

        var negatives = values.Where(v => v < 0).ToList();
        if (negatives.Count > 0)
        {
            // Перечисляем ВСЕ отрицательные — пользователю не нужно исправлять ввод по одному числу.
            // InvariantCulture: знак минуса в некоторых культурах отличается от '-'.
            var list = string.Join(", ", negatives.Select(v => v.ToString(CultureInfo.InvariantCulture)));
            throw new ArgumentException($"Negatives not allowed: {list}");
        }

        return values.Where(v => v <= MaxValue).Sum();
    }

    private static (char[] Delimiters, string Body) ParseHeader(string numbers)
    {
        if (!numbers.StartsWith("//", StringComparison.Ordinal))
        {
            return (new[] { ',', '\n' }, numbers);
        }

        // Формат заголовка строго "//X\n": разделитель на позиции 2, перевод строки на позиции 3.
        if (numbers.Length < 4 || numbers[3] != '\n')
        {
            throw new FormatException("Custom delimiter header must look like \"//X\\n\".");
        }

        return (new[] { ',', '\n', numbers[2] }, numbers[4..]);
    }
}
