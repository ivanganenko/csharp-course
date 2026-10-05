using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Module16.Tasks.Data;
using Module16.Tasks.Entities;

namespace Module16.Tests;

/// <summary>
/// SQLite in-memory база на один тест.
/// Почему соединение держим открытым: in-memory база SQLite живёт, пока открыто соединение;
/// закрыли — база исчезла. Все контексты теста работают через ОДНО соединение → видят одни данные.
/// Почему SQLite, а не EF InMemory-провайдер: SQLite — настоящая реляционная БД (уникальные индексы,
/// внешние ключи, транзакции, SQL), InMemory-провайдер всего этого не проверяет.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Новый контекст на том же соединении. sqlLog (если задан) получает текст каждой выполненной SQL-команды.</summary>
    public LibraryDbContext CreateContext(Action<string>? sqlLog = null)
    {
        var builder = new DbContextOptionsBuilder<LibraryDbContext>().UseSqlite(_connection);
        if (sqlLog is not null)
        {
            // Логируем только событие «команда выполнена» — по одному сообщению на SQL-команду.
            builder.LogTo(sqlLog, new[] { RelationalEventId.CommandExecuted });
        }

        return new LibraryDbContext(builder.Options);
    }

    public void Dispose() => _connection.Dispose();
}

/// <summary>
/// Стандартный набор данных. «Сегодня» в тестах — <see cref="Today"/> (вторник, 10 марта 2026).
/// <code>
/// Авторы: Boris Stone (2 книги), Clara Vale (2), Mira Oakwood (1), Dan Empty (0)
/// Книги:  The Ring Keeper (Mira, 2001, 2 экз., Fantasy+Mystery)   Stone Garden (Boris, 1999, 1 экз., Mystery)
///         Quiet Orbit (Clara, 2015, 1 экз., Science)              Ring of Salt (Boris, 2010, 3 экз., Fantasy)
///         Atlas of Fog (Clara, 2020, 1 экз., Mystery+Science)
/// Читатели: Ivan Testov, Olga Primerova, Petr Novikov (без выдач)
/// Выдачи:  L1 RingKeeper→Ivan  01.02–15.02, возвращена 10.02
///          L2 StoneGarden→Ivan 01.03–15.03, на руках (не просрочена)
///          L3 QuietOrbit→Olga  01.02–15.02, на руках (ПРОСРОЧЕНА)
///          L4 RingKeeper→Olga  20.02–06.03, на руках (ПРОСРОЧЕНА)
///          L5 RingOfSalt→Ivan  05.01–19.01, возвращена 18.01
/// Свободно: RingKeeper 1, StoneGarden 0, QuietOrbit 0, RingOfSalt 3, AtlasOfFog 1
/// </code>
/// </summary>
public sealed class LibraryData
{
    public static readonly DateOnly Today = new(2026, 3, 10);

    public required Author Boris { get; init; }
    public required Author Clara { get; init; }
    public required Author Mira { get; init; }
    public required Author Dan { get; init; }
    public required Book RingKeeper { get; init; }
    public required Book StoneGarden { get; init; }
    public required Book QuietOrbit { get; init; }
    public required Book RingOfSalt { get; init; }
    public required Book AtlasOfFog { get; init; }
    public required Member Ivan { get; init; }
    public required Member Olga { get; init; }
    public required Member Petr { get; init; }
    public required Loan L1 { get; init; }
    public required Loan L2 { get; init; }
    public required Loan L3 { get; init; }
    public required Loan L4 { get; init; }
    public required Loan L5 { get; init; }

    public static LibraryData Seed(TestDatabase database)
    {
        using var db = database.CreateContext();

        var fantasy = new Genre { Name = "Fantasy" };
        var mystery = new Genre { Name = "Mystery" };
        var science = new Genre { Name = "Science" };

        var boris = new Author { Name = "Boris Stone" };
        var clara = new Author { Name = "Clara Vale" };
        var mira = new Author { Name = "Mira Oakwood" };
        var dan = new Author { Name = "Dan Empty" };

        var ringKeeper = new Book { Title = "The Ring Keeper", Isbn = "9780000000001", PublishedYear = 2001, CopiesTotal = 2, Author = mira, Genres = { fantasy, mystery } };
        var stoneGarden = new Book { Title = "Stone Garden", Isbn = "9780000000002", PublishedYear = 1999, CopiesTotal = 1, Author = boris, Genres = { mystery } };
        var quietOrbit = new Book { Title = "Quiet Orbit", Isbn = "9780000000003", PublishedYear = 2015, CopiesTotal = 1, Author = clara, Genres = { science } };
        var ringOfSalt = new Book { Title = "Ring of Salt", Isbn = "9780000000004", PublishedYear = 2010, CopiesTotal = 3, Author = boris, Genres = { fantasy } };
        var atlasOfFog = new Book { Title = "Atlas of Fog", Isbn = "9780000000005", PublishedYear = 2020, CopiesTotal = 1, Author = clara, Genres = { mystery, science } };

        var ivan = new Member { FullName = "Ivan Testov", Email = "ivan@example.test" };
        var olga = new Member { FullName = "Olga Primerova", Email = "olga@example.test" };
        var petr = new Member { FullName = "Petr Novikov", Email = "petr@example.test" };

        var l1 = new Loan { Book = ringKeeper, Member = ivan, LoanedOn = new(2026, 2, 1), DueOn = new(2026, 2, 15), ReturnedOn = new DateOnly(2026, 2, 10) };
        var l2 = new Loan { Book = stoneGarden, Member = ivan, LoanedOn = new(2026, 3, 1), DueOn = new(2026, 3, 15) };
        var l3 = new Loan { Book = quietOrbit, Member = olga, LoanedOn = new(2026, 2, 1), DueOn = new(2026, 2, 15) };
        var l4 = new Loan { Book = ringKeeper, Member = olga, LoanedOn = new(2026, 2, 20), DueOn = new(2026, 3, 6) };
        var l5 = new Loan { Book = ringOfSalt, Member = ivan, LoanedOn = new(2026, 1, 5), DueOn = new(2026, 1, 19), ReturnedOn = new DateOnly(2026, 1, 18) };

        db.AddRange(dan, ringKeeper, stoneGarden, quietOrbit, ringOfSalt, atlasOfFog, petr, l1, l2, l3, l4, l5);
        db.SaveChanges();

        return new LibraryData
        {
            Boris = boris, Clara = clara, Mira = mira, Dan = dan,
            RingKeeper = ringKeeper, StoneGarden = stoneGarden, QuietOrbit = quietOrbit, RingOfSalt = ringOfSalt, AtlasOfFog = atlasOfFog,
            Ivan = ivan, Olga = olga, Petr = petr,
            L1 = l1, L2 = l2, L3 = l3, L4 = l4, L5 = l5,
        };
    }
}
