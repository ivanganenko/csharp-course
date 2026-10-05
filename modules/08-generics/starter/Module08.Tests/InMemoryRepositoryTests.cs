using Module08.Tasks;

namespace Module08.Tests;

public class InMemoryRepositoryTests
{
    private static readonly Book Dune = new(1, "Dune", "F. Herbert");
    private static readonly Book Solaris = new(2, "Solaris", "S. Lem");
    private static readonly Book Hobbit = new(3, "The Hobbit", "J. R. R. Tolkien");

    [Fact]
    public void Add_ThenGetById_ReturnsSameEntity()
    {
        var repo = new InMemoryRepository<Book>();

        repo.Add(Dune);

        Assert.Same(Dune, repo.GetById(1));
        Assert.Equal(1, repo.Count);
    }

    [Fact]
    public void GetById_Missing_ReturnsNull()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Dune);

        Assert.Null(repo.GetById(99));
    }

    [Fact]
    public void Add_DuplicateId_ThrowsInvalidOperationException()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Dune);

        var duplicate = new Book(1, "Another", "Somebody");

        Assert.Throws<InvalidOperationException>(() => repo.Add(duplicate));
        Assert.Same(Dune, repo.GetById(1));
    }

    [Fact]
    public void Add_Null_ThrowsArgumentNullException()
    {
        var repo = new InMemoryRepository<Book>();

        Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
    }

    [Fact]
    public void GetAll_ReturnsEntitiesSortedById()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Hobbit);
        repo.Add(Dune);
        repo.Add(Solaris);

        var all = repo.GetAll();

        Assert.Equal(new[] { 1, 2, 3 }, all.Select(b => b.Id));
    }

    [Fact]
    public void GetAll_ReturnsCopy_ModifyingItDoesNotAffectRepository()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Dune);

        var all = repo.GetAll();
        if (all is List<Book> list)
        {
            list.Clear();
        }

        Assert.Equal(1, repo.Count);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Update_Existing_ReplacesEntityAndReturnsTrue()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Dune);
        var updated = Dune with { Title = "Dune (2nd edition)" };

        bool result = repo.Update(updated);

        Assert.True(result);
        Assert.Equal("Dune (2nd edition)", repo.GetById(1)!.Title);
        Assert.Equal(1, repo.Count);
    }

    [Fact]
    public void Update_Missing_ReturnsFalseAndDoesNotAdd()
    {
        var repo = new InMemoryRepository<Book>();

        Assert.False(repo.Update(Solaris));
        Assert.Equal(0, repo.Count);
    }

    [Fact]
    public void Remove_ExistingAndMissing_ReturnsCorrectFlag()
    {
        var repo = new InMemoryRepository<Book>();
        repo.Add(Dune);

        Assert.True(repo.Remove(1));
        Assert.False(repo.Remove(1));
        Assert.Null(repo.GetById(1));
        Assert.Equal(0, repo.Count);
    }

    [Fact]
    public void Repository_WorksForAnotherEntityType()
    {
        // Тот же generic-класс, другой тип — в этом и смысл generics.
        var readers = new InMemoryRepository<Reader>();
        readers.Add(new Reader(10, "Reader A"));
        readers.Add(new Reader(5, "Reader B"));

        Assert.Equal("Reader B", readers.GetAll()[0].Name);
        Assert.Equal("Reader A", readers.GetById(10)!.Name);
    }
}
