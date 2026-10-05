namespace Module07.Tasks;

/// <summary>
/// Задача 07.1 — собственное исключение для ошибок валидации.
/// Наследуется от Exception, хранит имя невалидного свойства.
/// Конвенция .NET: имя заканчивается на Exception, есть конструкторы (сообщение) и (сообщение, inner).
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// propertyName null/пусто/пробелы → ArgumentException (или наследник).
    /// Message исключения = message.
    /// </summary>
    public ValidationException(string propertyName, string message)
        : base(message)
    {
        throw new NotImplementedException();
    }

    /// <summary>То же + InnerException = innerException (исходная причина ошибки).</summary>
    public ValidationException(string propertyName, string message, Exception? innerException)
        : base(message, innerException)
    {
        throw new NotImplementedException();
    }

    /// <summary>Имя свойства, не прошедшего проверку: "Username", "Email", "Age".</summary>
    public string PropertyName => throw new NotImplementedException();
}

/// <summary>Запрос на регистрацию. Уже готов — менять не нужно.</summary>
public sealed class RegistrationRequest
{
    public string? Username { get; init; }

    public string? Email { get; init; }

    public int Age { get; init; }
}

/// <summary>
/// Правила:
/// Username — 3..20 символов, только латинские буквы, цифры и '_' (пробелы не допускаются, Trim не делаем);
/// Email — непустой, без пробельных символов, ровно один '@', и до и после '@' есть хотя бы один символ;
/// Age — от MinAge до MaxAge включительно.
/// </summary>
public static class RegistrationValidator
{
    public const int MinAge = 14;
    public const int MaxAge = 120;
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 20;

    /// <summary>
    /// request == null → ArgumentNullException (это ошибка программиста, а не пользователя).
    /// Проверяет поля в порядке Username, Email, Age и при ПЕРВОЙ ошибке бросает ValidationException
    /// с PropertyName = "Username" / "Email" / "Age". Если всё валидно — ничего не делает.
    /// </summary>
    public static void Validate(RegistrationRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Альтернатива без исключений: возвращает имена ВСЕХ невалидных свойств в порядке Username, Email, Age
    /// (пустой список — всё валидно). request == null → ArgumentNullException.
    /// Подсказка: вынеси проверки в private-методы и используй их и здесь, и в Validate.
    /// </summary>
    public static IReadOnlyList<string> GetErrors(RegistrationRequest request)
    {
        throw new NotImplementedException();
    }
}
