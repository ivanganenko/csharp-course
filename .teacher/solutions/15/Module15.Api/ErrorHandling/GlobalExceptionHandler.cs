using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Module15.Api.ErrorHandling;

// IExceptionHandler (.NET 8+) регистрируется как singleton — внедряем только singleton-зависимости (ILogger).
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public static ProblemDetails CreateProblem(Exception exception) => exception switch
    {
        // Порядок веток важен так же, как порядок catch: сначала конкретные типы, в конце — «всё остальное».
        TaskNotFoundException notFound => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Ресурс не найден",
            Detail = notFound.Message,
        },
        BusinessRuleException rule => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Нарушено бизнес-правило",
            Detail = rule.Message,
        },
        // Detail не заполняем: текст неожиданного исключения может содержать секреты
        // (строки подключения, SQL, пути). Подробности — только в логах сервера.
        _ => new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Внутренняя ошибка сервера",
        },
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = CreateProblem(exception);

        // 5xx — наша ошибка, нужен stack trace и алерт → Error.
        // 4xx — ошибка клиента, штатная ситуация → Warning (иначе логи утонут в «ошибках»).
        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Необработанное исключение, TraceId={TraceId}", httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning("Запрос завершился ошибкой {Status}: {Message}", problem.Status, exception.Message);
        }

        // traceId позволяет клиенту/поддержке найти запись в логах по конкретному запросу.
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        // TypedResults.Problem сам выставит status code и Content-Type: application/problem+json.
        await TypedResults.Problem(problem).ExecuteAsync(httpContext);

        // true = «обработано»: middleware не будет пробрасывать исключение дальше.
        return true;
    }
}
