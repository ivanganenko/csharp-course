namespace Module04.Tasks;

// Extension methods живут в static-классе: у них нет состояния, это просто «синтаксический сахар»
// над вызовом StringExtensions.Truncate(value, 5). Класс string при этом не меняется.
public static class StringExtensions
{
    // Параметр string? — метод осознанно поддерживает вызов на null.
    public static bool IsNullOrBlank(this string? value) => string.IsNullOrWhiteSpace(value);

    public static string Truncate(this string value, int maxLength)
    {
        // Extension method МОЖНО вызвать на null (это статический вызов), поэтому проверка обязательна:
        // иначе вместо понятного ArgumentNullException получим NullReferenceException внутри.
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    public static int WordCount(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        // separator = null означает «любые пробельные символы»; приведение (char[]?) убирает
        // неоднозначность между перегрузками Split. RemoveEmptyEntries схлопывает подряд идущие пробелы.
        return value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static string Mask(this string value, int visibleSuffix = 4, char maskChar = '*')
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(visibleSuffix);

        if (value.Length <= visibleSuffix)
        {
            return value;
        }

        // new string(char, count) и range-оператор [^n..] — без циклов и без лишних промежуточных строк.
        return new string(maskChar, value.Length - visibleSuffix) + value[^visibleSuffix..];
    }
}
