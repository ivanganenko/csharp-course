namespace Module07.Tasks;

// Своё исключение — когда вызывающему коду нужно ОТЛИЧИТЬ эту ошибку от других и получить
// дополнительные данные (PropertyName), например, чтобы вернуть пользователю 400 с именем поля.
public class ValidationException : Exception
{
    // Цепочка конструкторов: вся логика инициализации в одном месте.
    public ValidationException(string propertyName, string message)
        : this(propertyName, message, null)
    {
    }

    // innerException сохраняет исходную причину: при логировании видна вся цепочка ошибок.
    public ValidationException(string propertyName, string message, Exception? innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}

public sealed class RegistrationRequest
{
    public string? Username { get; init; }

    public string? Email { get; init; }

    public int Age { get; init; }
}

public static class RegistrationValidator
{
    public const int MinAge = 14;
    public const int MaxAge = 120;
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 20;

    public static void Validate(RegistrationRequest request)
    {
        // null-запрос — баг вызывающего кода (контракт метода нарушен), поэтому ArgumentNullException,
        // а не ValidationException: это разные категории ошибок, и обрабатываться они должны по-разному.
        ArgumentNullException.ThrowIfNull(request);

        // Каждая проверка возвращает текст ошибки или null — одна и та же логика для Validate и GetErrors.
        foreach (var (property, error) in CheckAll(request))
        {
            if (error is not null)
            {
                throw new ValidationException(property, error);
            }
        }
    }

    public static IReadOnlyList<string> GetErrors(RegistrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Альтернатива исключениям: для валидации пользовательского ввода ошибки — ожидаемая ситуация,
        // и удобно показать пользователю ВСЕ проблемы сразу, а не по одной.
        return CheckAll(request)
            .Where(check => check.Error is not null)
            .Select(check => check.Property)
            .ToList();
    }

    private static IEnumerable<(string Property, string? Error)> CheckAll(RegistrationRequest request)
    {
        yield return (nameof(RegistrationRequest.Username), CheckUsername(request.Username));
        yield return (nameof(RegistrationRequest.Email), CheckEmail(request.Email));
        yield return (nameof(RegistrationRequest.Age), CheckAge(request.Age));
    }

    private static string? CheckUsername(string? username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return "Username is required.";
        }

        if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
        {
            return $"Username must be {MinUsernameLength}-{MaxUsernameLength} characters long.";
        }

        // IsAsciiLetterOrDigit: только латиница и цифры 0-9 (char.IsLetterOrDigit пропустил бы кириллицу).
        if (!username.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
        {
            return "Username may contain only latin letters, digits and '_'.";
        }

        return null;
    }

    private static string? CheckEmail(string? email)
    {
        if (string.IsNullOrEmpty(email) || email.Any(char.IsWhiteSpace))
        {
            return "Email is required and must not contain spaces.";
        }

        int at = email.IndexOf('@');
        bool valid = at > 0 && at == email.LastIndexOf('@') && at < email.Length - 1;
        return valid ? null : "Email must look like name@domain.";
    }

    private static string? CheckAge(int age) =>
        age is >= MinAge and <= MaxAge ? null : $"Age must be between {MinAge} and {MaxAge}.";
}
