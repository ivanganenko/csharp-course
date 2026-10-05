namespace Module04.Tasks;

public sealed class Book
{
    public const int MinYear = 1;
    public const int MaxYear = 2100;

    // readonly-поля можно присваивать в init-аксессорах — это и даёт «неизменяемость после создания».
    // Начальное "" нужно только чтобы успокоить nullable-анализ: required гарантирует,
    // что Title/Author всё равно будут присвоены в object initializer.
    private readonly string _title = "";
    private readonly string _author = "";
    private readonly int? _year;
    private readonly string? _isbn;

    // required: компилятор не даст создать Book без Title (ошибка CS9035).
    // Это проверка на этапе компиляции — дешевле и надёжнее, чем исключение в runtime.
    public required string Title
    {
        get => _title;
        init => _title = RequireText(value);
    }

    public required string Author
    {
        get => _author;
        init => _author = RequireText(value);
    }

    public int? Year
    {
        get => _year;
        init
        {
            // Pattern matching: проверяем диапазон только если значение задано (не null).
            if (value is int year && (year < MinYear || year > MaxYear))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value), value, $"Year must be between {MinYear} and {MaxYear}.");
            }

            _year = value;
        }
    }

    public string? Isbn
    {
        get => _isbn;
        // Нормализуем при записи, а не при каждом чтении: храним уже «чистое» значение.
        init => _isbn = value is null ? null : NormalizeIsbn(value);
    }

    // Вычисляемое свойство: не хранит данные, а строит их из других свойств — не может «устареть».
    public string Description =>
        Year is null ? $"{Title} by {Author}" : $"{Title} by {Author} ({Year})";

    private static string RequireText(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static string NormalizeIsbn(string raw)
    {
        var digits = raw.Replace("-", "").Replace(" ", "");

        // char.IsAsciiDigit, а не char.IsDigit: IsDigit вернёт true и для арабских/деванагари цифр.
        if ((digits.Length != 10 && digits.Length != 13) || !digits.All(char.IsAsciiDigit))
        {
            throw new ArgumentException("ISBN must contain exactly 10 or 13 digits.", nameof(raw));
        }

        return digits;
    }
}
