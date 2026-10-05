namespace Module06.Tasks;

/// <summary>
/// Задача 06.1 — равенство по значению для класса.
/// По умолчанию для класса Equals и == сравнивают ССЫЛКИ: new Point(1, 2) == new Point(1, 2) → false.
/// Сделай так, чтобы две точки с одинаковыми координатами считались равными везде:
/// Equals(Point), Equals(object), ==, !=, и чтобы они правильно работали как ключи Dictionary/HashSet.
/// </summary>
public sealed class Point : IEquatable<Point>
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }

    public int Y { get; }

    /// <summary>
    /// Строго типизированное сравнение (из IEquatable&lt;Point&gt;). other == null → false.
    /// Равны, если совпадают X и Y.
    /// </summary>
    public bool Equals(Point? other)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Переопределение object.Equals: true только если obj — Point с теми же координатами.
    /// null или объект другого типа → false.
    /// </summary>
    public override bool Equals(object? obj)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Контракт: если a.Equals(b), то a.GetHashCode() == b.GetHashCode().
    /// Подсказка: HashCode.Combine.
    /// </summary>
    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Равенство по значению. Оба null → true; один null → false.
    /// Осторожно: внутри оператора == нельзя писать left == null — это рекурсивный вызов самого себя.
    /// Используй left is null.
    /// </summary>
    public static bool operator ==(Point? left, Point? right)
    {
        throw new NotImplementedException();
    }

    /// <summary>Противоположность ==.</summary>
    public static bool operator !=(Point? left, Point? right)
    {
        throw new NotImplementedException();
    }

    public override string ToString() => $"({X}, {Y})";
}
