namespace Module04.Tasks;

/// <summary>
/// Задача 04.3 — карточка книги: required, init, валидация в init-аксессорах, вычисляемое свойство.
/// Объект создаётся через object initializer:
///   var book = new Book { Title = "The Silent Compiler", Author = "A. Petrova", Year = 2021 };
/// Title и Author помечены required — без них код не скомпилируется (ошибка CS9035).
/// После создания свойства изменить нельзя (init, а не set).
/// Подсказка: для валидации в init понадобятся private readonly поля.
/// </summary>
public sealed class Book
{
    /// <summary>Наименьший допустимый год издания.</summary>
    public const int MinYear = 1;

    /// <summary>Наибольший допустимый год издания.</summary>
    public const int MaxYear = 2100;

    /// <summary>
    /// Название. null/пусто/пробелы → ArgumentException (или наследник). Пробелы по краям убираются.
    /// </summary>
    public required string Title
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }

    /// <summary>
    /// Автор. null/пусто/пробелы → ArgumentException (или наследник). Пробелы по краям убираются.
    /// </summary>
    public required string Author
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }

    /// <summary>
    /// Год издания — необязательный (null = неизвестен).
    /// Если задан и не в диапазоне [MinYear; MaxYear] → ArgumentOutOfRangeException.
    /// </summary>
    public int? Year
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }

    /// <summary>
    /// ISBN — необязательный (null = не задан).
    /// При присваивании дефисы и пробелы удаляются: "978-5-00000-000-1" → "9785000000001".
    /// После очистки должно остаться ровно 10 или 13 цифр, иначе → ArgumentException.
    /// (Упрощение: букву X в ISBN-10 не поддерживаем.)
    /// </summary>
    public string? Isbn
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }

    /// <summary>
    /// Вычисляемое описание:
    /// без года: "The Silent Compiler by A. Petrova";
    /// с годом:  "The Silent Compiler by A. Petrova (2021)".
    /// </summary>
    public string Description => throw new NotImplementedException();
}
