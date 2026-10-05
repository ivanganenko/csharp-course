namespace Module15.Api.Middleware;

public sealed class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-Id";
    public const int MaxIncomingIdLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestIdMiddleware> _logger;

    // Middleware создаётся ОДИН раз на всё приложение (фактически singleton),
    // поэтому в конструктор можно внедрять только singleton-зависимости (ILogger — singleton).
    // Scoped-сервисы, если понадобятся, берут параметрами InvokeAsync — они резолвятся на каждый запрос.
    public RequestIdMiddleware(RequestDelegate next, ILogger<RequestIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var requestId = IsValidIncomingId(incoming) ? incoming : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = requestId;

        // Заголовок ставим ДО _next: как только следующий middleware начнёт писать тело,
        // заголовки уйдут клиенту и менять их будет поздно (InvalidOperationException).
        context.Response.Headers[HeaderName] = requestId;

        // Logging scope: все логи, написанные «ниже» по pipeline в этом запросе, получат RequestId.
        using (_logger.BeginScope("RequestId: {RequestId}", requestId))
        {
            await _next(context);
        }
    }

    public static bool IsValidIncomingId(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxIncomingIdLength)
        {
            return false;
        }

        foreach (var c in value)
        {
            // IsAsciiLetterOrDigit, а не IsLetterOrDigit: последний пропустит кириллицу и другие Unicode-буквы.
            if (!char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_')
            {
                return false;
            }
        }

        return true;
    }
}
