namespace Module06.Tasks;

/// <summary>
/// Задача 06.5 — Money из модуля 04, переделанный в readonly record struct:
/// value type, неизменяемый, равенство по значению генерирует компилятор.
///
/// Нужно:
/// 1) Валидация и нормализация Currency: пробелы по краям убираются, код приводится к верхнему регистру
///    и должен состоять ровно из 3 латинских букв, иначе → ArgumentException (или наследник; null тоже).
///    Это должно работать и в конструкторе, и в with: (money with { Currency = "eur" }).Currency == "EUR".
///    Подсказка: объяви свойство Currency вручную с private readonly полем и init-аксессором;
///    поле можно инициализировать параметром первичного конструктора: = Normalize(Currency).
/// 2) Операторы. Разные валюты в +, -, &lt;, &gt;, &lt;=, &gt;= → InvalidOperationException.
///    Умножение округляет результат до 2 знаков AwayFromZero. Сумма в конструкторе НЕ округляется.
/// 3) ToString: "12.50 EUR" (две цифры после точки, InvariantCulture).
/// </summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    /// <summary>Ноль в указанной валюте (валюта нормализуется как обычно).</summary>
    public static Money Zero(string currency) => throw new NotImplementedException();

    public static Money operator +(Money left, Money right) => throw new NotImplementedException();

    public static Money operator -(Money left, Money right) => throw new NotImplementedException();

    public static Money operator *(Money money, decimal factor) => throw new NotImplementedException();

    public static bool operator <(Money left, Money right) => throw new NotImplementedException();

    public static bool operator >(Money left, Money right) => throw new NotImplementedException();

    public static bool operator <=(Money left, Money right) => throw new NotImplementedException();

    public static bool operator >=(Money left, Money right) => throw new NotImplementedException();

    public override string ToString() => throw new NotImplementedException();
}
