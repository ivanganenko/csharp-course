using Module06.Tasks;

namespace Module06.Tests;

public class RecordTests
{
    private static readonly ProductDto Pen = new("PEN-01", "Blue pen", 19.99m);

    [Theory]
    [InlineData(10, "17.99")]
    [InlineData(0, "19.99")]
    [InlineData(100, "0")]
    [InlineData(50, "10.00")]
    public void ApplyDiscount_ReturnsNewRecordWithRoundedPrice(int percent, string expected)
    {
        var discounted = ProductOperations.ApplyDiscount(Pen, percent);

        Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), discounted.Price);
        Assert.Equal(Pen.Sku, discounted.Sku);
        Assert.Equal(Pen.Name, discounted.Name);
        Assert.Equal(19.99m, Pen.Price); // исходный record не изменился
    }

    [Fact]
    public void ApplyDiscount_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ProductOperations.ApplyDiscount(Pen, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ProductOperations.ApplyDiscount(Pen, 101));
        Assert.Throws<ArgumentNullException>(() => ProductOperations.ApplyDiscount(null!, 10));
    }

    [Fact]
    public void Rename_ReturnsNewRecordEqualByValue()
    {
        var renamed = ProductOperations.Rename(Pen, "  Red pen ");

        Assert.Equal(new ProductDto("PEN-01", "Red pen", 19.99m), renamed);
        Assert.NotSame(Pen, renamed);
        Assert.Equal("Blue pen", Pen.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Rename_BlankName_Throws(string? name)
    {
        Assert.ThrowsAny<ArgumentException>(() => ProductOperations.Rename(Pen, name!));
    }

    [Fact]
    public void ShoppingList_SameItemsInDifferentListInstances_AreEqual()
    {
        var a = new ShoppingList("Anna", new List<string> { "milk", "bread" });
        var b = new ShoppingList("Anna", new[] { "milk", "bread" });

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ShoppingList_DifferentOrderOrOwner_AreNotEqual()
    {
        var a = new ShoppingList("Anna", new List<string> { "milk", "bread" });

        Assert.False(a.Equals(new ShoppingList("Anna", new List<string> { "bread", "milk" })));
        Assert.False(a.Equals(new ShoppingList("anna", new List<string> { "milk", "bread" })));
        Assert.False(a.Equals(new ShoppingList("Anna", new List<string> { "milk" })));
        Assert.False(a.Equals(null));
    }

    [Fact]
    public void ShoppingList_WithExpression_CreatesChangedCopy()
    {
        var a = new ShoppingList("Anna", new List<string> { "milk" });

        var b = a with { Owner = "Boris" };

        Assert.Equal("Anna", a.Owner);
        Assert.Equal("Boris", b.Owner);
        Assert.False(a == b);
        Assert.Same(a.Items, b.Items); // with делает ПОВЕРХНОСТНУЮ копию: список общий
    }

    [Fact]
    public void ShoppingList_WorksInHashSet()
    {
        var set = new HashSet<ShoppingList>
        {
            new("Anna", new List<string> { "milk" }),
            new("Anna", new List<string> { "milk" }),
        };

        Assert.Equal(1, set.Count);
    }
}
