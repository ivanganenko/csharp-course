using Module05.Tasks;

namespace Module05.Tests;

public class ShapeTests
{
    [Fact]
    public void Circle_AreaAndPerimeter()
    {
        var circle = new Circle(2);

        Assert.Equal(2.0, circle.Radius, 10);
        Assert.Equal(Math.PI * 4, circle.Area(), 10);
        Assert.Equal(Math.PI * 4, circle.Perimeter(), 10);
    }

    [Fact]
    public void Rectangle_AreaAndPerimeter()
    {
        var rectangle = new Rectangle(2, 3);

        Assert.Equal(6.0, rectangle.Area(), 10);
        Assert.Equal(10.0, rectangle.Perimeter(), 10);
    }

    [Fact]
    public void Square_AreaAndPerimeter()
    {
        var square = new Square(4);

        Assert.Equal(16.0, square.Area(), 10);
        Assert.Equal(16.0, square.Perimeter(), 10);
    }

    [Fact]
    public void Triangle_AreaByHeronAndPerimeter()
    {
        var triangle = new Triangle(3, 4, 5);

        Assert.Equal(6.0, triangle.Area(), 10);
        Assert.Equal(12.0, triangle.Perimeter(), 10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructors_NonPositiveDimensions_ThrowArgumentOutOfRange(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Circle(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(value, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(1, value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Square(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Triangle(value, 1, 1));
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(1, 1, 5)]
    [InlineData(10, 1, 1)]
    public void Triangle_InequalityViolated_ThrowsArgumentException(double a, double b, double c)
    {
        Assert.Throws<ArgumentException>(() => new Triangle(a, b, c));
    }

    [Fact]
    public void Name_DefaultsToClassName()
    {
        Shape[] shapes = [new Circle(1), new Rectangle(1, 2), new Square(1), new Triangle(3, 4, 5)];

        Assert.Equal("Circle Rectangle Square Triangle", string.Join(" ", shapes.Select(s => s.Name)));
    }

    [Fact]
    public void Describe_UsesPolymorphicMembersAndInvariantFormat()
    {
        Shape shape = new Rectangle(2, 3);

        Assert.Equal("Rectangle: area 6.00, perimeter 10.00", shape.Describe());
        Assert.Equal(shape.Describe(), shape.ToString());
    }

    [Fact]
    public void TotalArea_SumsAreasThroughBaseType()
    {
        var shapes = new List<Shape> { new Rectangle(2, 3), new Square(2), new Triangle(3, 4, 5) };

        Assert.Equal(16.0, ShapeStats.TotalArea(shapes), 10);
        Assert.Equal(0.0, ShapeStats.TotalArea(new List<Shape>()), 10);
    }

    [Fact]
    public void Largest_ReturnsShapeWithMaxAreaOrNull()
    {
        var big = new Square(10);
        var shapes = new List<Shape> { new Circle(1), big, new Rectangle(2, 3) };

        Assert.Same(big, ShapeStats.Largest(shapes));
        Assert.Null(ShapeStats.Largest(new List<Shape>()));
    }

    [Fact]
    public void Stats_NullCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ShapeStats.TotalArea(null!));
        Assert.Throws<ArgumentNullException>(() => ShapeStats.Largest(null!));
    }
}
