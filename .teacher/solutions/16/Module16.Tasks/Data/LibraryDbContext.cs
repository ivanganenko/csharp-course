using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Data;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Author> Authors => Set<Author>();

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Member> Members => Set<Member>();

    public DbSet<Loan> Loans => Set<Loan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fluent API, а не атрибуты [MaxLength] на сущностях: сущности остаются «чистыми»,
        // а вся схема БД описана в одном месте. В большом проекте каждую конфигурацию выносят
        // в класс IEntityTypeConfiguration<T> и подключают modelBuilder.ApplyConfigurationsFromAssembly(...).

        modelBuilder.Entity<Author>(author =>
        {
            author.Property(a => a.Name).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Title).IsRequired().HasMaxLength(200);
            book.Property(b => b.Isbn).IsRequired().HasMaxLength(13);

            // Уникальность — задача БАЗЫ, а не только кода: проверка «есть ли такой ISBN» в C#
            // не спасёт от двух параллельных запросов. Уникальный индекс гарантирует это атомарно
            // и заодно ускоряет поиск по ISBN.
            book.HasIndex(b => b.Isbn).IsUnique();

            // Check constraint — инвариант на уровне БД: даже SQL «мимо» приложения не запишет мусор.
            book.ToTable(t => t.HasCheckConstraint("CK_Books_CopiesTotal", "\"CopiesTotal\" >= 0"));

            // По соглашению для обязательного FK было бы Cascade: удаление автора тихо удалило бы
            // все его книги (и, каскадом, историю выдач). Restrict заставляет решать это явно.
            book.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // N-N: EF Core создаст таблицу-связку сам (skip navigations). Явно — для читаемости.
            book.HasMany(b => b.Genres).WithMany(g => g.Books);
        });

        modelBuilder.Entity<Genre>(genre =>
        {
            genre.Property(g => g.Name).IsRequired().HasMaxLength(50);
            genre.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<Member>(member =>
        {
            member.Property(m => m.FullName).IsRequired().HasMaxLength(150);
            // 254 — практический максимум длины email-адреса.
            member.Property(m => m.Email).IsRequired().HasMaxLength(254);
            member.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<Loan>(loan =>
        {
            // История выдач — важные данные (отчёты, споры с читателями): запрещаем каскадное удаление.
            loan.HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            loan.HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Частый запрос «активные выдачи читателя» — индекс по (MemberId, ReturnedOn).
            loan.HasIndex(l => new { l.MemberId, l.ReturnedOn });
        });
    }
}
