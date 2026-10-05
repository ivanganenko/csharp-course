namespace Module13.Tests;

/// <summary>
/// Защита от зависания тестов: если тестируемая операция не завершилась за разумное время,
/// тест падает с понятным сообщением, а не висит бесконечно.
/// Время здесь — только «аварийный предохранитель», а не проверка скорости кода.
/// </summary>
internal static class AsyncGuard
{
    public static readonly TimeSpan Limit = TimeSpan.FromSeconds(10);

    public static async Task<T> WithTimeout<T>(Task<T> task)
    {
        var winner = await Task.WhenAny(task, Task.Delay(Limit));
        if (winner != task)
        {
            throw new InvalidOperationException("Тест завис: операция не завершилась за 10 секунд.");
        }

        return await task;
    }

    public static async Task WithTimeout(Task task)
    {
        var winner = await Task.WhenAny(task, Task.Delay(Limit));
        if (winner != task)
        {
            throw new InvalidOperationException("Тест завис: операция не завершилась за 10 секунд.");
        }

        await task;
    }
}
