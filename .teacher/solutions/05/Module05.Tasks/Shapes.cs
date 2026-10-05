using System.Globalization;

namespace Module05.Tasks;

// abstract class, а не interface: у фигур есть общая РЕАЛИЗАЦИЯ (Name по умолчанию, Describe, ToString),
// и мы хотим, чтобы наследники её получали, а не копировали.
public abstract class Shape
{
    public virtual string Name => GetType().Name;

    public abstract double Area();

    public abstract double Perimeter();

    // Не virtual: формат описания одинаков для всех. Полиморфизм работает «внутри» —
    // Area()/Perimeter()/Name вызываются у реального типа объекта (late binding).
    public string Describe() =>
        string.Create(CultureInfo.InvariantCulture, $"{Name}: area {Area():0.00}, perimeter {Perimeter():0.00}");

    public override string ToString() => Describe();
}

public sealed class Circle : Shape
{
    public Circle(double radius)
    {
        // Валидация в конструкторе: невалидную фигуру нельзя даже создать.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        Radius = radius;
    }

    public double Radius { get; }

    public override double Area() => Math.PI * Radius * Radius;

    public override double Perimeter() => 2 * Math.PI * Radius;
}

public class Rectangle : Shape
{
    public Rectangle(double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
    }

    // Get-only: фигура неизменяемая. Это, кстати, и снимает классическую LSP-проблему
    // Rectangle/Square — она возникает, только если у Rectangle есть сеттеры Width/Height.
    public double Width { get; }

    public double Height { get; }

    public override double Area() => Width * Height;

    public override double Perimeter() => 2 * (Width + Height);
}

// Не наследуем Square от Rectangle: «квадрат — это прямоугольник» верно в геометрии,
// но если бы у Rectangle были сеттеры, Square пришлось бы менять обе стороны сразу,
// и код, рассчитывающий на независимость Width/Height, сломался бы (нарушение LSP).
public sealed class Square : Shape
{
    public Square(double side)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(side);
        Side = side;
    }

    public double Side { get; }

    public override double Area() => Side * Side;

    public override double Perimeter() => 4 * Side;
}

public sealed class Triangle : Shape
{
    public Triangle(double a, double b, double c)
    {
        // Сначала проверки каждой стороны (ArgumentOutOfRange), потом — отношения между ними (Argument).
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(a);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(b);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(c);

        if (a + b <= c || a + c <= b || b + c <= a)
        {
            throw new ArgumentException("Sides do not satisfy the triangle inequality.");
        }

        A = a;
        B = b;
        C = c;
    }

    public double A { get; }

    public double B { get; }

    public double C { get; }

    public override double Area()
    {
        // Формула Герона.
        var p = Perimeter() / 2;
        return Math.Sqrt(p * (p - A) * (p - B) * (p - C));
    }

    public override double Perimeter() => A + B + C;
}

public static class ShapeStats
{
    // Принимаем IEnumerable<Shape> — самый общий тип: подойдут List, массив, результат LINQ.
    public static double TotalArea(IEnumerable<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        // Метод не знает, какие фигуры в коллекции, — и ему не нужно знать. Это и есть полиморфизм.
        return shapes.Sum(s => s.Area());
    }

    public static Shape? Largest(IEnumerable<Shape> shapes)
    {
        ArgumentNullException.ThrowIfNull(shapes);

        Shape? largest = null;
        double maxArea = double.MinValue;
        foreach (var shape in shapes)
        {
            // Area() считаем один раз на фигуру; строгое > оставляет первую из равных.
            var area = shape.Area();
            if (area > maxArea)
            {
                maxArea = area;
                largest = shape;
            }
        }

        return largest;
    }
}
