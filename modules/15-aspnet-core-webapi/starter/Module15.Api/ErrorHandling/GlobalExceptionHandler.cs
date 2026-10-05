using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Module15.Api.ErrorHandling;

/// <summary>
/// Задача 5. Глобальная обработка ошибок: любое необработанное исключение → ответ ProblemDetails (RFC 9457).
/// Уже подключено в Program.cs: AddProblemDetails() + AddExceptionHandler&lt;GlobalExceptionHandler&gt;() + app.UseExceptionHandler().
/// Если нужен ILogger — добавь конструктор с параметром ILogger&lt;GlobalExceptionHandler&gt; (DI сам его передаст).
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Чистая функция «исключение → ProblemDetails» (её легко тестировать без HTTP):
    /// <list type="bullet">
    /// <item><see cref="TaskNotFoundException"/> → Status = 404, Title = "Ресурс не найден", Detail = exception.Message;</item>
    /// <item><see cref="BusinessRuleException"/> → Status = 409, Title = "Нарушено бизнес-правило", Detail = exception.Message;</item>
    /// <item>любое другое → Status = 500, Title = "Внутренняя ошибка сервера", Detail = null
    /// (НЕ показываем клиенту текст исключения: там могут быть строки подключения, SQL, пути к файлам).</item>
    /// </list>
    /// </summary>
    public static ProblemDetails CreateProblem(Exception exception)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Вызывается middleware UseExceptionHandler. Должен:
    /// 1) построить ProblemDetails через <see cref="CreateProblem"/>;
    /// 2) (желательно) залогировать: 5xx — LogError с исключением, 4xx — LogWarning/LogInformation;
    /// 3) записать ответ: например, await TypedResults.Problem(problem).ExecuteAsync(httpContext);
    /// 4) вернуть true — «исключение обработано».
    /// </summary>
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
