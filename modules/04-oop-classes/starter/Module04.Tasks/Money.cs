namespace Module04.Tasks;

/// <summary>
/// Задача 04.7 ⭐ — Money как value object (объект-значение): сумма + валюта, неизменяемый (immutable).
/// Все операции возвращают НОВЫЙ объект, исходный не меняется.
/// В модуле 06 мы переделаем его в readonly record struct с операторами и равенством по значению.
/// </summary>
public sealed class Money
{
    /// <summary>
    /// currency: null/пусто/пробелы → ArgumentException (или наследник).
    /// Код валюты приводится к верхнему регистру ("usd" → "USD", пробелы по краям убираются)
    /// и после этого должен состоять ровно из 3 латинских букв A–Z, иначе → ArgumentException.
    /// amount округляется до 2 знаков, середина — от нуля: 2.345 → 2.35, 10.005 → 10.01
    /// (Math.Round(x, 2, MidpointRounding.AwayFromZero)).
    /// </summary>
    public Money(decimal amount, string currency)
    {
        throw new NotImplementedException();
    }

    public decimal Amount => throw new NotImplementedException();

    public string Currency => throw new NotImplementedException();

    /// <summary>Ноль в указанной валюте.</summary>
    public static Money Zero(string currency)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сумма. other == null → ArgumentNullException; другая валюта → InvalidOperationException.
    /// </summary>
    public Money Add(Money other)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Разность (результат может быть отрицательным). Проверки — как в Add.
    /// </summary>
    public Money Subtract(Money other)
    {
        throw new NotImplementedException();
    }

    /// <summary>Умножение на коэффициент; результат округляется так же, как в конструкторе.</summary>
    public Money Multiply(decimal factor)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Делит сумму на parts частей так, чтобы сумма частей ТОЧНО равнялась исходной (ни цента не потерять).
    /// Лишние центы раздаются первым частям по одному: 100.00 на 3 → [33.34, 33.33, 33.33].
    /// parts &lt;= 0 → ArgumentOutOfRangeException; Amount &lt; 0 → InvalidOperationException.
    /// </summary>
    public IReadOnlyList<Money> Allocate(int parts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Формат: сумма с двумя знаками и точкой (InvariantCulture), пробел, код: "5.00 EUR", "-2.50 USD".
    /// </summary>
    public override string ToString()
    {
        throw new NotImplementedException();
    }
}
