namespace Module15.Api.Middleware;

/// <summary>
/// Задача 3. Свой middleware: у каждого ответа должен быть заголовок X-Request-Id.
/// Он уже подключён в Program.cs через app.UseMiddleware&lt;RequestIdMiddleware&gt;().
/// <para>
/// Поведение InvokeAsync:
/// <list type="number">
/// <item>Если в запросе есть заголовок X-Request-Id и <see cref="IsValidIncomingId"/> возвращает true — используем его
/// (так id «протягивается» через цепочку микросервисов).</item>
/// <item>Иначе генерируем новый: Guid.NewGuid().ToString("N") (32 hex-символа без дефисов).</item>
/// <item>Записываем id в заголовок ответа <see cref="HeaderName"/> ДО вызова следующего middleware
/// (после того как ответ начал отправляться, заголовки менять уже нельзя).</item>
/// <item>Кладём id в context.TraceIdentifier — его используют логи и ProblemDetails.</item>
/// <item>Вызываем следующий middleware: await _next(context).</item>
/// </list>
/// </para>
/// </summary>
public sealed class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-Id";
    public const int MaxIncomingIdLength = 64;

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // TODO (задача 3): реализуй поведение из описания класса.
        // Почему здесь не NotImplementedException: этот middleware стоит в pipeline у ВСЕХ запросов —
        // исключение «уронило» бы все остальные задачи модуля. Пока он просто передаёт запрос дальше.
        await _next(context);
    }

    /// <summary>
    /// Можно ли доверять id, пришедшему от клиента:
    /// не null и не пустой, длина не больше <see cref="MaxIncomingIdLength"/>,
    /// все символы — латинские буквы, цифры, '-' или '_'.
    /// Почему проверяем: значение уйдёт в заголовок ответа и в логи — нельзя отражать произвольный ввод
    /// (мусор, огромные строки, спецсимволы).
    /// </summary>
    public static bool IsValidIncomingId(string? value)
    {
        throw new NotImplementedException();
    }
}
