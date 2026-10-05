namespace Module16.Tasks.Entities;

// Сущности (entities) библиотеки. EF Core по соглашениям (conventions) понимает:
// свойство Id — первичный ключ; AuthorId + навигация Author — внешний ключ (связь 1-N);
// List<Genre> в Book и List<Book> в Genre — связь N-N (EF сам создаст таблицу-связку BookGenre).

/// <summary>Автор. Один автор — много книг (1-N).</summary>
public class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Book> Books { get; set; } = new();
}

/// <summary>Книга (точнее, позиция каталога: у неё может быть несколько экземпляров CopiesTotal).</summary>
public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>ISBN-13 без дефисов, уникален.</summary>
    public string Isbn { get; set; } = string.Empty;

    public int PublishedYear { get; set; }

    /// <summary>Сколько физических экземпляров есть в библиотеке.</summary>
    public int CopiesTotal { get; set; } = 1;

    public int AuthorId { get; set; }

    // null! — «обещаем компилятору», что EF заполнит навигацию (при Include) или мы присвоим её сами.
    public Author Author { get; set; } = null!;

    public List<Genre> Genres { get; set; } = new();

    public List<Loan> Loans { get; set; } = new();
}

/// <summary>Жанр. Связь с книгами — многие-ко-многим (N-N).</summary>
public class Genre
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<Book> Books { get; set; } = new();
}

/// <summary>Читатель библиотеки.</summary>
public class Member
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    /// <summary>Уникален.</summary>
    public string Email { get; set; } = string.Empty;

    public List<Loan> Loans { get; set; } = new();
}

/// <summary>Выдача книги читателю. ReturnedOn == null — книга ещё на руках.</summary>
public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public Book Book { get; set; } = null!;

    public int MemberId { get; set; }

    public Member Member { get; set; } = null!;

    public DateOnly LoanedOn { get; set; }

    public DateOnly DueOn { get; set; }

    public DateOnly? ReturnedOn { get; set; }
}
