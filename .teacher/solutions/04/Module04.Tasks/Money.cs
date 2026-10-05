using System.Globalization;

namespace Module04.Tasks;

// sealed: value object не предназначен для наследования — наследник мог бы добавить
// изменяемое состояние и сломать гарантию неизменяемости.
public sealed class Money
{
    public Money(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        // Нормализуем на входе: дальше во всём классе можно сравнивать валюты простым ==.
        var code = currency.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code, e.g. USD.", nameof(currency));
        }

        // decimal, а не double: деньги в десятичной системе, double не может точно хранить 0.1.
        // AwayFromZero — «школьное» округление; Math.Round по умолчанию банковское (ToEven).
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = code;
    }

    // Только get: после создания объект не меняется. Значит, его можно безопасно
    // передавать куда угодно и использовать из разных потоков — никто его «не испортит».
    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Zero(string currency) => new(0m, currency);

    // Операции возвращают НОВЫЙ объект — как string.Trim() или DateTime.AddDays().
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public IReadOnlyList<Money> Allocate(int parts)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parts);
        if (Amount < 0)
        {
            throw new InvalidOperationException("Cannot allocate a negative amount.");
        }

        // Работаем в целых центах: так остаток от деления считается точно и ни один цент не теряется.
        // Наивное Amount / parts дало бы 33.33 * 3 = 99.99 — минус цент у бухгалтерии.
        long totalCents = (long)(Amount * 100);
        long baseCents = totalCents / parts;
        long remainder = totalCents % parts;

        var result = new List<Money>(parts);
        for (int i = 0; i < parts; i++)
        {
            long cents = baseCents + (i < remainder ? 1 : 0);
            result.Add(new Money(cents / 100m, Currency));
        }

        return result;
    }

    public override string ToString() =>
        $"{Amount.ToString("0.00", CultureInfo.InvariantCulture)} {Currency}";

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Сложить 10 USD и 10 EUR без курса нельзя — это логическая ошибка вызывающего кода,
        // поэтому InvalidOperationException, а не тихое «20 USD».
        if (other.Currency != Currency)
        {
            throw new InvalidOperationException(
                $"Cannot combine {Currency} and {other.Currency}.");
        }
    }
}
