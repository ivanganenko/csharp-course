using Microsoft.AspNetCore.Mvc;
using Module15.Api.Diagnostics;
using Module15.Api.Models;

namespace Module15.Api.Endpoints;

/// <summary>
/// Служебные эндпоинты для задач 4 и 5. Готовы, менять не нужно.
/// </summary>
public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diagnostics");

        group.MapGet("/counters", GetCounters);
        group.MapGet("/crash", Crash);

        return app;
    }

    /// <summary>
    /// GET /api/diagnostics/counters. Каждый счётчик запрашивается из DI ДВАЖДЫ в рамках одного запроса,
    /// и каждый полученный экземпляр увеличивается на 1. Ответ — значения «второго» экземпляра.
    /// Подумай: какие числа вернутся в первом и во втором запросе для каждого lifetime?
    /// [FromServices] явно говорит «бери из DI». Без него minimal API угадывает источник сам:
    /// незарегистрированный класс он посчитал бы JSON-телом запроса, а у GET тела нет —
    /// ошибка построения эндпоинта сломала бы ВСЕ маршруты, пока задача 4 не решена.
    /// </summary>
    public static CountersResponse GetCounters(
        [FromServices] SingletonCounter singleton1,
        [FromServices] SingletonCounter singleton2,
        [FromServices] ScopedCounter scoped1,
        [FromServices] ScopedCounter scoped2,
        [FromServices] TransientCounter transient1,
        [FromServices] TransientCounter transient2)
    {
        singleton1.Increment();
        singleton2.Increment();
        scoped1.Increment();
        scoped2.Increment();
        transient1.Increment();
        transient2.Increment();

        return new CountersResponse(singleton2.Value, scoped2.Value, transient2.Value);
    }

    /// <summary>
    /// GET /api/diagnostics/crash — имитация «неожиданной» ошибки с секретом в сообщении.
    /// Клиент НЕ должен увидеть текст этого исключения (задача 5).
    /// </summary>
    public static string Crash() =>
        throw new InvalidOperationException("Не удалось подключиться: Server=db.internal;Password=SuperSecret123");
}
