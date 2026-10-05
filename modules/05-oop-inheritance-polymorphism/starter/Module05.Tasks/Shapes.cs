namespace Module05.Tasks;

/// <summary>
/// Задача 05.1 — иерархия фигур.
/// Shape — abstract class: экземпляр создать нельзя, наследники ОБЯЗАНЫ реализовать Area и Perimeter.
/// </summary>
public abstract class Shape
{
    /// <summary>Имя фигуры. По умолчанию — имя класса (GetType().Name). Наследник может переопределить.</summary>
    public virtual string Name => GetType().Name;

    /// <summary>Площадь.</summary>
    public abstract double Area();

    /// <summary>Периметр.</summary>
    public abstract double Perimeter();

    /// <summary>
    /// Описание (НЕ virtual — одинаковый алгоритм для всех фигур, меняются только Name/Area/Perimeter):
    /// "{Name}: area {Area}, perimeter {Perimeter}", числа — с двумя знаками и точкой (InvariantCulture).
    /// Rectangle 2×3 → "Rectangle: area 6.00, perimeter 10.00".
    /// </summary>
    public string Describe()
    {
        throw new NotImplementedException();
    }

    /// <summary>Возвращает Describe() — чтобы фигура красиво печаталась в Console.WriteLine и в отладчике.</summary>
    public override string ToString() => Describe();
}

/// <summary>Круг. radius &lt;= 0 → ArgumentOutOfRangeException.</summary>
public sealed class Circle : Shape
{
    public Circle(double radius)
    {
        throw new NotImplementedException();
    }

    public double Radius => throw new NotImplementedException();

    public override double Area() => throw new NotImplementedException();

    public override double Perimeter() => throw new NotImplementedException();
}

/// <summary>Прямоугольник. width/height &lt;= 0 → ArgumentOutOfRangeException.</summary>
public class Rectangle : Shape
{
    public Rectangle(double width, double height)
    {
        throw new NotImplementedException();
    }

    public double Width => throw new NotImplementedException();

    public double Height => throw new NotImplementedException();

    public override double Area() => throw new NotImplementedException();

    public override double Perimeter() => throw new NotImplementedException();
}

/// <summary>
/// Квадрат. side &lt;= 0 → ArgumentOutOfRangeException.
/// Обрати внимание: Square наследуется от Shape, а НЕ от Rectangle — почему, разберём в задаче 05.6 (LSP).
/// </summary>
public sealed class Square : Shape
{
    public Square(double side)
    {
        throw new NotImplementedException();
    }

    public double Side => throw new NotImplementedException();

    public override double Area() => throw new NotImplementedException();

    public override double Perimeter() => throw new NotImplementedException();
}

/// <summary>
/// Треугольник по трём сторонам. Любая сторона &lt;= 0 → ArgumentOutOfRangeException.
/// Нарушено неравенство треугольника (каждая сторона должна быть строго меньше суммы двух других,
/// например 1, 2, 3 — «вырожденный») → ArgumentException.
/// Площадь — по формуле Герона: p = периметр / 2; S = sqrt(p(p-a)(p-b)(p-c)).
/// </summary>
public sealed class Triangle : Shape
{
    public Triangle(double a, double b, double c)
    {
        throw new NotImplementedException();
    }

    public double A => throw new NotImplementedException();

    public double B => throw new NotImplementedException();

    public double C => throw new NotImplementedException();

    public override double Area() => throw new NotImplementedException();

    public override double Perimeter() => throw new NotImplementedException();
}

/// <summary>Операции над любыми фигурами — работают через базовый тип Shape (полиморфизм).</summary>
public static class ShapeStats
{
    /// <summary>Сумма площадей. shapes == null → ArgumentNullException. Пустая коллекция → 0.</summary>
    public static double TotalArea(IEnumerable<Shape> shapes)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Фигура с наибольшей площадью (при равенстве — первая из них). Пустая коллекция → null.
    /// shapes == null → ArgumentNullException.
    /// </summary>
    public static Shape? Largest(IEnumerable<Shape> shapes)
    {
        throw new NotImplementedException();
    }
}
