namespace Module13.Tasks;

/// <summary>
/// Задача 13.2 — кооперативная отмена через CancellationToken.
/// Импорт заказов из файла: пользователь может нажать «Отмена» в любой момент.
/// </summary>
public static class OrderImporter
{
    /// <summary>
    /// Импортирует строки заказов ПО ОЧЕРЕДИ (в исходном порядке), вызывая importOne для каждой.
    /// - ПЕРЕД обработкой каждой строки проверяет токен: если отмена запрошена —
    ///   бросает OperationCanceledException (уже импортированное остаётся импортированным).
    /// - Передаёт cancellationToken в importOne.
    /// - Возвращает количество импортированных строк.
    /// - orderLines или importOne == null → ArgumentNullException.
    /// </summary>
    public static Task<int> ImportAsync(
        IEnumerable<string> orderLines,
        Func<string, CancellationToken, Task> importOne,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
