namespace Module06.Tasks;

// sealed: равенство с наследниками — источник багов (a.Equals(b) != b.Equals(a), если наследник
// добавил поле). Запечатанный класс эту проблему снимает.
public sealed class Point : IEquatable<Point>
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    // Неизменяемые свойства — обязательное условие для ключа словаря: если X поменяется после
    // добавления в HashSet, хэш станет другим, и объект «потеряется» в своём бакете.
    public int X { get; }

    public int Y { get; }

    // IEquatable<T>.Equals — основной метод: строго типизированный, без приведения типов.
    // Dictionary/HashSet (через EqualityComparer<T>.Default) вызывают именно его.
    public bool Equals(Point? other) =>
        other is not null && X == other.X && Y == other.Y;

    // object.Equals делегирует строго типизированному — логика сравнения в одном месте.
    public override bool Equals(object? obj) => obj is Point other && Equals(other);

    // Контракт: равные объекты → равные хэши. HashCode.Combine хорошо перемешивает биты;
    // наивное X ^ Y даёт одинаковый хэш для (1, 2) и (2, 1), и коллизий будет много.
    public override int GetHashCode() => HashCode.Combine(X, Y);

    // В операторе используем is null, а не == null: иначе оператор вызвал бы сам себя (бесконечная рекурсия).
    public static bool operator ==(Point? left, Point? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Point? left, Point? right) => !(left == right);

    public override string ToString() => $"({X}, {Y})";
}
