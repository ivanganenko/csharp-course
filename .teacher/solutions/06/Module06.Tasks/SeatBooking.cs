namespace Module06.Tasks;

// readonly struct: маленький неизменяемый ключ. Реализуем IEquatable<SeatKey>, иначе Dictionary
// пошёл бы через object.Equals(object) — с boxing на каждое сравнение и медленным ValueType.Equals (рефлексия).
public readonly struct SeatKey : IEquatable<SeatKey>
{
    public SeatKey(string hall, int row, int seat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hall);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(row);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(seat);

        Hall = hall.Trim();
        Row = row;
        Seat = seat;
    }

    public string Hall { get; }

    public int Row { get; }

    public int Seat { get; }

    // Сначала дешёвые сравнения int, потом строка.
    public bool Equals(SeatKey other) =>
        Row == other.Row
        && Seat == other.Seat
        && string.Equals(Hall, other.Hall, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is SeatKey other && Equals(other);

    // Главное правило: хэш должен быть согласован с Equals. Equals игнорирует регистр Hall —
    // значит, и хэш Hall считаем без учёта регистра. Обычный Hall.GetHashCode() дал бы для "Red" и "RED"
    // разные хэши, Dictionary искал бы в другом бакете и не нашёл бы бронь.
    // default(SeatKey) имеет Hall == null — StringComparer.GetHashCode(null) бросил бы исключение.
    public override int GetHashCode() =>
        HashCode.Combine(
            Hall is null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Hall),
            Row,
            Seat);

    public static bool operator ==(SeatKey left, SeatKey right) => left.Equals(right);

    public static bool operator !=(SeatKey left, SeatKey right) => !left.Equals(right);

    public override string ToString() => $"{Hall} R{Row} S{Seat}";
}

public sealed class BookingService
{
    // Ключ сам знает, как сравниваться, поэтому словарю не нужен особый IEqualityComparer.
    private readonly Dictionary<SeatKey, string> _bookings = new();

    public int Count => _bookings.Count;

    public bool TryBook(SeatKey seat, string customer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customer);

        // TryAdd — одна операция поиска вместо ContainsKey + Add.
        return _bookings.TryAdd(seat, customer.Trim());
    }

    public bool IsBooked(SeatKey seat) => _bookings.ContainsKey(seat);

    public string? GetCustomer(SeatKey seat) =>
        _bookings.TryGetValue(seat, out var customer) ? customer : null;

    public bool Cancel(SeatKey seat) => _bookings.Remove(seat);

    public IReadOnlyList<SeatKey> GetSeatsOf(string customer) =>
        _bookings
            .Where(pair => string.Equals(pair.Value, customer, StringComparison.OrdinalIgnoreCase))
            .Select(pair => pair.Key)
            .OrderBy(seat => seat.Hall, StringComparer.OrdinalIgnoreCase)
            .ThenBy(seat => seat.Row)
            .ThenBy(seat => seat.Seat)
            .ToList();
}
