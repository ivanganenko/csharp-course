namespace Module07.Tasks;

public static class Retry
{
    public static T Execute<T>(
        Func<T> operation,
        int maxAttempts,
        Func<Exception, bool>? isTransient = null,
        Action<int, Exception>? onRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return operation();
            }
            // Exception filter (when): если условие false, исключение НЕ ловится вовсе —
            // оно летит дальше нетронутым, с исходным stack trace. Так обрабатываются и
            // «последняя попытка», и «не временная ошибка» — без throw; и без риска написать throw ex;.
            // Плюс: фильтр выполняется до раскрутки стека, отладчик остановится в месте исходной ошибки.
            catch (Exception ex) when (attempt < maxAttempts && (isTransient?.Invoke(ex) ?? true))
            {
                onRetry?.Invoke(attempt, ex);
            }
        }
    }
}
