using System.Globalization;

namespace Module06.Tasks;

// readonly record struct:
// - struct: маленький (decimal + ссылка), без идентичности, создаётся часто — без аллокаций в куче;
// - readonly: все поля неизменяемы, компилятор это проверяет, и нет «защитных копий»;
// - record: Equals/GetHashCode/==/!=/ToString/Deconstruct по значению генерирует компилятор.
public readonly record struct Money(decimal Amount, string Currency)
{
    // Объявляем Currency вручную, чтобы валидация работала и в конструкторе (через инициализатор поля,
    // использующий параметр первичного конструктора), и в with (через init-аксессор).
    // Синтезированное равенство сравнит это поле — оно хранит уже нормализованное значение.
    private readonly string _currency = NormalizeCurrency(Currency);

    public string Currency
    {
        get => _currency;
        init => _currency = NormalizeCurrency(value);
    }

    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal factor) =>
        new(Math.Round(money.Amount * factor, 2, MidpointRounding.AwayFromZero), money.Currency);

    // Операторы сравнения объявляются парами (< и >, <= и >=) — этого требует компилятор.
    public static bool operator <(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount < right.Amount;
    }

    public static bool operator >(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount > right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount <= right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount >= right.Amount;
    }

    // Переопределяем ToString record'а: синтезированный ("Money { Amount = 12.5, Currency = EUR }")
    // зависит от культуры и годится только для отладки.
    public override string ToString() =>
        $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";

    private static string NormalizeCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        var code = currency.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        return code;
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Cannot combine {left.Currency} and {right.Currency}.");
        }
    }
}
