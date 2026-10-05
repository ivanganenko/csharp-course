using Module04.Tasks;

namespace Module04.Tests;

public class BookTests
{
    [Fact]
    public void ObjectInitializer_ValidValues_SetsTrimmedProperties()
    {
        var book = new Book { Title = "  The Silent Compiler ", Author = " A. Petrova ", Year = 2021 };

        Assert.Equal("The Silent Compiler", book.Title);
        Assert.Equal("A. Petrova", book.Author);
        Assert.Equal(2021, book.Year);
    }

    [Fact]
    public void OptionalProperties_NotSet_AreNull()
    {
        var book = new Book { Title = "Garbage Collected Poems", Author = "I. Sidorov" };

        Assert.Null(book.Year);
        Assert.Null(book.Isbn);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Title_Blank_ThrowsArgumentException(string? title)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Book { Title = title!, Author = "A. Petrova" });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Author_Blank_ThrowsArgumentException(string? author)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Book { Title = "Title", Author = author! });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(2101)]
    public void Year_OutOfRange_ThrowsArgumentOutOfRange(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Book { Title = "T", Author = "A", Year = year });
    }

    [Theory]
    [InlineData(Book.MinYear)]
    [InlineData(Book.MaxYear)]
    public void Year_Boundaries_AreAllowed(int year)
    {
        var book = new Book { Title = "T", Author = "A", Year = year };

        Assert.Equal(year, book.Year);
    }

    [Theory]
    [InlineData("978-5-00000-000-1", "9785000000001")]
    [InlineData("0-00-000000-0", "0000000000")]
    [InlineData("978 5 00000 000 1", "9785000000001")]
    public void Isbn_ValidValue_IsNormalized(string raw, string expected)
    {
        var book = new Book { Title = "T", Author = "A", Isbn = raw };

        Assert.Equal(expected, book.Isbn);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901")]
    [InlineData("978-5-ABCDE-000-1")]
    public void Isbn_InvalidValue_ThrowsArgumentException(string raw)
    {
        Assert.Throws<ArgumentException>(() => new Book { Title = "T", Author = "A", Isbn = raw });
    }

    [Fact]
    public void Description_WithAndWithoutYear()
    {
        var withYear = new Book { Title = "The Silent Compiler", Author = "A. Petrova", Year = 2021 };
        var withoutYear = new Book { Title = "The Silent Compiler", Author = "A. Petrova" };

        Assert.Equal("The Silent Compiler by A. Petrova (2021)", withYear.Description);
        Assert.Equal("The Silent Compiler by A. Petrova", withoutYear.Description);
    }
}
