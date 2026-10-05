namespace Module07.Tasks;

/// <summary>
/// Задача 07.6 ⭐ — Result&lt;T&gt;: результат операции, которая может «штатно» не удаться,
/// без исключений. Либо успех со значением, либо неудача с текстом ошибки.
/// Объект неизменяемый; создаётся только через Success/Failure.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess => throw new NotImplementedException();

    public bool IsFailure => throw new NotImplementedException();

    /// <summary>Значение. Для неудачного результата → InvalidOperationException.</summary>
    public T Value => throw new NotImplementedException();

    /// <summary>Текст ошибки. Для успешного результата → InvalidOperationException.</summary>
    public string Error => throw new NotImplementedException();

    public static Result<T> Success(T value)
    {
        throw new NotImplementedException();
    }

    /// <summary>error null/пусто/пробелы → ArgumentException (или наследник).</summary>
    public static Result<T> Failure(string error)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Успех → Success(map(Value)); неудача → Failure с той же ошибкой, map НЕ вызывается.
    /// map == null → ArgumentNullException.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Как Map, но функция сама возвращает Result (может «упасть»): успех → bind(Value); неудача → Failure с той же ошибкой.
    /// bind == null → ArgumentNullException.
    /// </summary>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        throw new NotImplementedException();
    }

    /// <summary>Вызывает onSuccess(Value) или onFailure(Error) и возвращает результат вызванной функции.</summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure)
    {
        throw new NotImplementedException();
    }

    /// <summary>Value для успеха, иначе defaultValue.</summary>
    public T GetValueOrDefault(T defaultValue)
    {
        throw new NotImplementedException();
    }
}

/// <summary>Пример использования Result: разбор возраста из пользовательского ввода.</summary>
public static class AgeParser
{
    /// <summary>
    /// null/пусто/пробелы → Failure("Age is required");
    /// не целое число (InvariantCulture, пробелы по краям допустимы) → Failure("Age must be a number");
    /// вне 0..150 → Failure("Age must be between 0 and 150");
    /// иначе → Success(возраст). Исключения не бросает.
    /// </summary>
    public static Result<int> Parse(string? input)
    {
        throw new NotImplementedException();
    }
}
