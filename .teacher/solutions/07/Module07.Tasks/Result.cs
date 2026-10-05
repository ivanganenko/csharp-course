using System.Globalization;

namespace Module07.Tasks;

// Result<T> делает возможную неудачу частью СИГНАТУРЫ метода: по Result<int> видно, что разбор может
// не удаться, а по int Parse(...) — нет (исключение «невидимо»). Плюс нет стоимости исключений
// для ожидаемых ошибок (валидация ввода, бизнес-правила).
public sealed class Result<T>
{
    private readonly T _value;
    private readonly string? _error;

    // Private: создать «успех без значения» или «неудачу без ошибки» невозможно.
    private Result(bool isSuccess, T value, string? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    // Обращение к Value у неудачи — баг вызывающего кода (не проверил IsSuccess), поэтому исключение.
    public T Value => IsSuccess
        ? _value
        : throw new InvalidOperationException($"Cannot read Value of a failed result: {_error}");

    public string Error => IsSuccess
        ? throw new InvalidOperationException("Cannot read Error of a successful result.")
        : _error!;

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new Result<T>(false, default!, error);
    }

    // Map/Bind позволяют строить цепочки без if (result.IsFailure) return ... на каждом шаге:
    // первая же неудача «проезжает» до конца цепочки (railway-oriented programming).
    public Result<TOut> Map<TOut>(Func<T, TOut> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsSuccess ? Result<TOut>.Success(map(_value)) : Result<TOut>.Failure(_error!);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return IsSuccess ? bind(_value) : Result<TOut>.Failure(_error!);
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value) : onFailure(_error!);
    }

    public T GetValueOrDefault(T defaultValue) => IsSuccess ? _value : defaultValue;

    public override string ToString() => IsSuccess ? $"Success({_value})" : $"Failure({_error})";
}

public static class AgeParser
{
    public const int MaxAge = 150;

    // Пользовательский ввод «с ошибкой» — ожидаемая ситуация, не исключительная.
    // Поэтому TryParse + Result, а не int.Parse в try/catch.
    public static Result<int> Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Result<int>.Failure("Age is required");
        }

        if (!int.TryParse(input.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var age))
        {
            return Result<int>.Failure("Age must be a number");
        }

        if (age < 0 || age > MaxAge)
        {
            return Result<int>.Failure("Age must be between 0 and 150");
        }

        return Result<int>.Success(age);
    }
}
