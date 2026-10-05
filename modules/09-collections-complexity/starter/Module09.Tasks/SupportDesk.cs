namespace Module09.Tasks;

/// <summary>
/// Задача 09.5 — «выбери коллекцию»: очередь обращений в службу поддержки.
/// Для каждой операции подберите коллекцию с подходящей сложностью и обоснуйте выбор в комментарии.
/// Тесты: SupportDeskTests.
/// </summary>
public class SupportDesk
{
    /// <summary>Количество обращений, ожидающих обработки.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>
    /// Идентификаторы обращений в том порядке, в котором их забрали через <see cref="TakeNext"/>.
    /// </summary>
    public IReadOnlyList<string> History => throw new NotImplementedException();

    /// <summary>
    /// Регистрирует обращение. Меньшее число priority = более срочное (1 — авария, 5 — вопрос).
    /// ticketId: null/пустой/из пробелов → <see cref="ArgumentException"/> (или наследник).
    /// Обращение с таким же id уже ожидает обработки → <see cref="InvalidOperationException"/>
    /// (сравнение id — с учётом регистра, ordinal). После TakeNext id можно использовать снова.
    /// Сложность: O(log n).
    /// </summary>
    public void Submit(string ticketId, int priority)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Забирает самое срочное обращение (минимальный priority) и возвращает его id.
    /// При равном priority — то, что зарегистрировано раньше (FIFO).
    /// Нет обращений → null. Забранный id добавляется в <see cref="History"/>.
    /// Сложность: O(log n).
    /// </summary>
    public string? TakeNext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// true, если обращение с таким id ожидает обработки. Сложность: O(1).
    /// </summary>
    public bool Contains(string ticketId)
    {
        throw new NotImplementedException();
    }
}
