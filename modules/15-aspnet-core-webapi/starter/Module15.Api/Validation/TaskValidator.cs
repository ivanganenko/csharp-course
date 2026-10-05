namespace Module15.Api.Validation;

/// <summary>
/// Задача 2. Валидация входных данных задачи.
/// Результат — словарь «имя поля → сообщения об ошибках»: именно такой формат
/// ожидает TypedResults.ValidationProblem(...) и превращает его в ответ 400 + ProblemDetails.
/// </summary>
public static class TaskValidator
{
    public const int MaxTitleLength = 100;
    public const int MaxDescriptionLength = 1000;
    public const int MinPriority = 1;
    public const int MaxPriority = 5;

    /// <summary>
    /// Проверяет поля задачи и возвращает словарь ошибок (пустой, если всё корректно).
    /// Правила (ключи словаря — ровно такие, с большой буквы):
    /// <list type="bullet">
    /// <item>"Title": null, пустая строка или только пробелы → ошибка «Название обязательно»;
    /// длина после Trim() больше <see cref="MaxTitleLength"/> → ошибка о длине.</item>
    /// <item>"Description": может быть null; если длина больше <see cref="MaxDescriptionLength"/> → ошибка.</item>
    /// <item>"Priority": вне диапазона [<see cref="MinPriority"/>; <see cref="MaxPriority"/>] → ошибка.</item>
    /// </list>
    /// Если ошибок несколько — в словаре должны быть ВСЕ ключи (клиенту удобнее исправить всё за раз).
    /// Тексты сообщений — на твоё усмотрение, тесты проверяют только ключи.
    /// </summary>
    public static Dictionary<string, string[]> Validate(string? title, string? description, int priority)
    {
        throw new NotImplementedException();
    }
}
