namespace Module06.Tasks;

/// <summary>
/// Задача 06.6 ⭐ — составной ключ для Dictionary: место в кинотеатре (зал, ряд, кресло).
/// Особенность: название зала сравнивается БЕЗ учёта регистра ("Red" и "RED" — один зал).
/// record struct тут не подойдёт «из коробки» — он сравнивает строки с учётом регистра.
/// Поэтому равенство пишем вручную, и GetHashCode обязан быть согласован с Equals.
/// </summary>
public readonly struct SeatKey : IEquatable<SeatKey>
{
    /// <summary>
    /// hall null/пусто/пробелы → ArgumentException (или наследник); пробелы по краям убираются,
    /// регистр СОХРАНЯЕТСЯ как передан. row/seat &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public SeatKey(string hall, int row, int seat)
    {
        throw new NotImplementedException();
    }

    public string Hall => throw new NotImplementedException();

    public int Row => throw new NotImplementedException();

    public int Seat => throw new NotImplementedException();

    /// <summary>Равны, если совпадают Row, Seat и Hall (Hall — без учёта регистра, ordinal).</summary>
    public bool Equals(SeatKey other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Должен давать одинаковый результат для "Red"/5/7 и "RED"/5/7.
    /// Учти, что у default(SeatKey) Hall == null — метод не должен падать.
    /// Подсказка: StringComparer.OrdinalIgnoreCase.GetHashCode(...) + HashCode.Combine.
    /// </summary>
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public static bool operator ==(SeatKey left, SeatKey right) => throw new NotImplementedException();

    public static bool operator !=(SeatKey left, SeatKey right) => throw new NotImplementedException();

    /// <summary>"Red R5 S7".</summary>
    public override string ToString() => throw new NotImplementedException();
}

/// <summary>Бронирование мест на основе Dictionary&lt;SeatKey, string&gt; (место → имя клиента).</summary>
public sealed class BookingService
{
    /// <summary>Сколько мест забронировано.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// Забронировать место. Если место уже занято (в том числе записанное в другом регистре зала) → false.
    /// customer null/пусто/пробелы → ArgumentException (или наследник). Имя сохраняется без пробелов по краям.
    /// </summary>
    public bool TryBook(SeatKey seat, string customer)
    {
        throw new NotImplementedException();
    }

    public bool IsBooked(SeatKey seat)
    {
        throw new NotImplementedException();
    }

    /// <summary>Имя клиента, забронировавшего место, или null.</summary>
    public string? GetCustomer(SeatKey seat)
    {
        throw new NotImplementedException();
    }

    /// <summary>Снять бронь. true — если бронь была.</summary>
    public bool Cancel(SeatKey seat)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Все места клиента (имя — без учёта регистра), отсортированные по Hall (без учёта регистра), Row, Seat.
    /// </summary>
    public IReadOnlyList<SeatKey> GetSeatsOf(string customer)
    {
        throw new NotImplementedException();
    }
}
