using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Data;

/// <summary>
/// DbContext — «сессия работы с базой»: DbSet-ы (таблицы), change tracker, SaveChanges.
/// Опции (какая БД, строка подключения) приходят снаружи через конструктор — так один и тот же
/// контекст работает и с SQLite в тестах, и с SQL Server / PostgreSQL в продакшене.
/// </summary>
public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<Loan> Loans => Set<Loan>();

    /// <summary>
    /// Задача 1. Настрой модель через Fluent API (modelBuilder.Entity&lt;T&gt;(...)):
    /// <list type="bullet">
    /// <item>Author.Name — обязательное, максимум 150 символов.</item>
    /// <item>Book.Title — обязательное, максимум 200; Book.Isbn — обязательное, максимум 13, УНИКАЛЬНЫЙ индекс.</item>
    /// <item>Book: check constraint "CK_Books_CopiesTotal" — CopiesTotal &gt;= 0
    /// (подсказка: builder.ToTable(t =&gt; t.HasCheckConstraint(имя, sql))).</item>
    /// <item>Book → Author: удаление автора, у которого есть книги, запрещено — DeleteBehavior.Restrict.</item>
    /// <item>Genre.Name — обязательное, максимум 50, уникальный индекс.</item>
    /// <item>Member.FullName — обязательное, максимум 150; Member.Email — обязательное, максимум 254, уникальный индекс.</item>
    /// <item>Loan → Book и Loan → Member: DeleteBehavior.Restrict (историю выдач терять нельзя).</item>
    /// </list>
    /// Пока метод пуст, модель строится только по соглашениям — остальные задачи модуля уже работают.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TODO (задача 1). Не бросай здесь NotImplementedException: модель строится при первом
        // обращении к контексту, и упали бы тесты ВСЕХ задач модуля.
    }
}
