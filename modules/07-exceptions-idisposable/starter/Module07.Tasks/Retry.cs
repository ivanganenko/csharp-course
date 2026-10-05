namespace Module07.Tasks;

/// <summary>
/// Задача 07.5 — повтор операции при временных (transient) сбоях: сеть моргнула, БД заблокирована и т.п.
/// Без задержек между попытками (в реальном коде — экспоненциальная задержка, см. библиотеку Polly).
/// </summary>
public static class Retry
{
    /// <summary>
    /// Выполняет operation до maxAttempts раз.
    /// - Успех → вернуть результат сразу.
    /// - Исключение ex:
    ///     если isTransient(ex) == false → пробросить ex сразу, без повторов;
    ///     если это была последняя попытка → пробросить ex (ТО ЖЕ САМОЕ исключение, с исходным stack trace);
    ///     иначе → вызвать onRetry(номерНеудачнойПопытки, ex) и попробовать снова.
    /// - isTransient == null → все исключения считаются временными.
    /// - operation == null → ArgumentNullException; maxAttempts &lt; 1 → ArgumentOutOfRangeException.
    /// Попытки нумеруются с 1.
    /// Подсказка: catch (Exception ex) when (...) — фильтр исключений; тогда на последней попытке
    /// исключение просто не будет поймано и уйдёт наружу нетронутым.
    /// </summary>
    public static T Execute<T>(
        Func<T> operation,
        int maxAttempts,
        Func<Exception, bool>? isTransient = null,
        Action<int, Exception>? onRetry = null)
    {
        throw new NotImplementedException();
    }
}
