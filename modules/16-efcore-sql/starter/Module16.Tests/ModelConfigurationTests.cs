using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tests;

/// <summary>Задача 1. Конфигурация модели через Fluent API.</summary>
public sealed class ModelConfigurationTests : IDisposable
{
    private readonly TestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Theory]
    [InlineData(typeof(Author), nameof(Author.Name), 150)]
    [InlineData(typeof(Book), nameof(Book.Title), 200)]
    [InlineData(typeof(Book), nameof(Book.Isbn), 13)]
    [InlineData(typeof(Genre), nameof(Genre.Name), 50)]
    [InlineData(typeof(Member), nameof(Member.FullName), 150)]
    [InlineData(typeof(Member), nameof(Member.Email), 254)]
    public void Property_HasConfiguredMaxLength(Type entityType, string propertyName, int expectedMaxLength)
    {
        using var db = _database.CreateContext();

        var property = db.Model.FindEntityType(entityType)!.FindProperty(propertyName)!;

        Assert.Equal(expectedMaxLength, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    [Theory]
    [InlineData(typeof(Book), nameof(Book.Isbn))]
    [InlineData(typeof(Genre), nameof(Genre.Name))]
    [InlineData(typeof(Member), nameof(Member.Email))]
    public void Property_HasUniqueIndex(Type entityType, string propertyName)
    {
        using var db = _database.CreateContext();

        var indexes = db.Model.FindEntityType(entityType)!.GetIndexes();

        Assert.Contains(indexes, index =>
            index.IsUnique && index.Properties.Count == 1 && index.Properties[0].Name == propertyName);
    }

    [Theory]
    [InlineData(typeof(Book), typeof(Author))]
    [InlineData(typeof(Loan), typeof(Book))]
    [InlineData(typeof(Loan), typeof(Member))]
    public void ForeignKey_DeleteBehavior_IsRestrict(Type dependentType, Type principalType)
    {
        using var db = _database.CreateContext();

        var foreignKey = db.Model.FindEntityType(dependentType)!
            .GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == principalType);

        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void DuplicateIsbn_SaveChanges_ThrowsDbUpdateException()
    {
        using var db = _database.CreateContext();
        var author = new Author { Name = "Test Author" };
        db.Books.Add(new Book { Title = "First", Isbn = "9781111111111", PublishedYear = 2000, Author = author });
        db.Books.Add(new Book { Title = "Second", Isbn = "9781111111111", PublishedYear = 2001, Author = author });

        Assert.ThrowsAny<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void DuplicateEmail_SaveChanges_ThrowsDbUpdateException()
    {
        using var db = _database.CreateContext();
        db.Members.Add(new Member { FullName = "A", Email = "same@example.test" });
        db.Members.Add(new Member { FullName = "B", Email = "same@example.test" });

        Assert.ThrowsAny<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void DuplicateGenreName_SaveChanges_ThrowsDbUpdateException()
    {
        using var db = _database.CreateContext();
        db.Genres.Add(new Genre { Name = "Poetry" });
        db.Genres.Add(new Genre { Name = "Poetry" });

        Assert.ThrowsAny<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void NegativeCopiesTotal_SaveChanges_ThrowsDbUpdateException()
    {
        using var db = _database.CreateContext();
        db.Books.Add(new Book
        {
            Title = "Broken",
            Isbn = "9782222222222",
            PublishedYear = 2000,
            CopiesTotal = -1,
            Author = new Author { Name = "Someone" },
        });

        Assert.ThrowsAny<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void DifferentIsbns_SaveChanges_Succeeds()
    {
        using var db = _database.CreateContext();
        var author = new Author { Name = "Test Author" };
        db.Books.Add(new Book { Title = "First", Isbn = "9781111111111", PublishedYear = 2000, Author = author });
        db.Books.Add(new Book { Title = "Second", Isbn = "9781111111112", PublishedYear = 2001, Author = author });

        db.SaveChanges();

        Assert.Equal(2, db.Books.Count());
    }
}
