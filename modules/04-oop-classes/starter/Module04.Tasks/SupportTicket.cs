namespace Module04.Tasks;

/// <summary>
/// Задача 04.4 — заявка в службу поддержки со сквозной нумерацией через static-счётчик.
/// Каждая новая заявка получает Id = предыдущий + 1 (первая — 1).
/// Счётчик общий для всех экземпляров — значит, это static-член класса.
/// </summary>
public sealed class SupportTicket
{
    /// <summary>Максимальная длина заголовка. const — значение «вшивается» компилятором в место использования.</summary>
    public const int MaxTitleLength = 100;

    /// <summary>Префикс номера. static readonly — вычисляется один раз при инициализации типа.</summary>
    public static readonly string Prefix = "TCK-";

    /// <summary>
    /// Создаёт заявку.
    /// title: null/пусто/пробелы → ArgumentException (или наследник);
    /// после Trim длиннее MaxTitleLength → ArgumentException.
    /// ВАЖНО: если заголовок невалиден, счётчик НЕ должен увеличиться (проверка — до изменения состояния).
    /// Заголовок сохраняется без пробелов по краям.
    /// </summary>
    public SupportTicket(string title)
    {
        throw new NotImplementedException();
    }

    /// <summary>Сколько заявок успешно создано за всё время работы программы.</summary>
    public static int TotalCreated => throw new NotImplementedException();

    /// <summary>Уникальный номер заявки (1, 2, 3, ...).</summary>
    public int Id => throw new NotImplementedException();

    /// <summary>Заголовок заявки.</summary>
    public string Title => throw new NotImplementedException();

    /// <summary>Номер для людей: Prefix + Id, дополненный нулями до 4 цифр. Id = 7 → "TCK-0007".</summary>
    public string Number => throw new NotImplementedException();
}
