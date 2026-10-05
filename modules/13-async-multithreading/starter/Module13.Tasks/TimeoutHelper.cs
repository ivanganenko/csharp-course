namespace Module13.Tasks;

/// <summary>
/// Задача 13.5 — обёртка «выполни операцию, но не дольше timeout».
/// </summary>
public static class TimeoutHelper
{
    /// <summary>
    /// Запускает operation, передавая ей токен, и ждёт результат не дольше <paramref name="timeout"/>.
    /// - Успели → вернуть результат.
    /// - Не успели → бросить <see cref="TimeoutException"/> И отменить токен, переданный в operation
    ///   (к моменту, когда вызывающий получил TimeoutException, этот токен уже отменён).
    ///   Работает даже если operation игнорирует токен и не завершается никогда.
    /// - Вызывающий отменил cancellationToken → OperationCanceledException (НЕ TimeoutException).
    /// - operation бросила своё исключение → оно пробрасывается как есть.
    /// - timeout &lt;= TimeSpan.Zero → ArgumentOutOfRangeException; operation == null → ArgumentNullException.
    /// Подсказка: CancellationTokenSource.CreateLinkedTokenSource и Task.WaitAsync(TimeSpan, CancellationToken).
    /// </summary>
    public static Task<T> WithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
