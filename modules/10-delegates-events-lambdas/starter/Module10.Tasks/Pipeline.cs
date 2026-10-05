namespace Module10.Tasks;

// Задача 10.7 ⭐ — конвейер middleware из делегатов, как в ASP.NET Core (модуль 15).
// Тесты: PipelineTests.

/// <summary>Контекст «запроса». Класс готов — менять не нужно.</summary>
public sealed class RequestContext
{
    public RequestContext(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public int StatusCode { get; set; } = 200;

    public string? Response { get; set; }

    /// <summary>Журнал прохождения запроса через middleware (для наглядности и тестов).</summary>
    public List<string> Log { get; } = new();
}

/// <summary>Обработчик запроса. Свой тип делегата — чтобы сигнатура имела понятное имя.</summary>
public delegate void RequestHandler(RequestContext context);

/// <summary>
/// Собирает конвейер. Middleware — это функция <c>Func&lt;RequestHandler, RequestHandler&gt;</c>:
/// она получает «следующий» обработчик (next) и возвращает новый обработчик, который может
/// сделать что-то ДО next, вызвать next (или не вызвать — short-circuit) и сделать что-то ПОСЛЕ.
/// </summary>
public class PipelineBuilder
{
    /// <summary>
    /// Добавляет middleware. Первый добавленный — самый внешний (выполняется первым).
    /// Возвращает этот же builder (для цепочки вызовов). null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public PipelineBuilder Use(Func<RequestHandler, RequestHandler> middleware)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Задаёт конечный обработчик (terminal handler) — он вызывается последним, если все middleware вызвали next.
    /// Повторный вызов заменяет обработчик. Возвращает этот же builder. null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public PipelineBuilder Run(RequestHandler terminal)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Собирает конвейер в один <see cref="RequestHandler"/>.
    /// Если Run не вызывался, конечный обработчик ставит StatusCode = 404 и Response = "Not Found".
    /// Middleware, добавленные ПОСЛЕ Build, не влияют на уже собранный обработчик.
    /// Собранный обработчик можно вызывать многократно для разных запросов.
    /// </summary>
    public RequestHandler Build()
    {
        throw new NotImplementedException();
    }
}
