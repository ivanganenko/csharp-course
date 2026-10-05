namespace Module10.Tasks;

public sealed class RequestContext
{
    public RequestContext(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public int StatusCode { get; set; } = 200;

    public string? Response { get; set; }

    public List<string> Log { get; } = new();
}

public delegate void RequestHandler(RequestContext context);

public class PipelineBuilder
{
    private readonly List<Func<RequestHandler, RequestHandler>> _middlewares = new();
    private RequestHandler? _terminal;

    public PipelineBuilder Use(Func<RequestHandler, RequestHandler> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _middlewares.Add(middleware);
        return this; // fluent API — как app.Use(...).Use(...) в ASP.NET Core
    }

    public PipelineBuilder Run(RequestHandler terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        _terminal = terminal;
        return this;
    }

    public RequestHandler Build()
    {
        RequestHandler app = _terminal ?? new RequestHandler(NotFound);

        // Собираем «матрёшку» с конца: последний middleware оборачивает terminal,
        // предпоследний — результат и т.д. Поэтому первый добавленный оказывается самым внешним
        // и выполняется первым. Именно так ASP.NET Core строит конвейер в ApplicationBuilder.Build.
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            app = _middlewares[i](app);
        }

        // Конвейер собран сейчас, а не при каждом запросе: добавленные позже middleware
        // в него не попадут, а сам обработчик можно вызывать сколько угодно раз.
        return app;
    }

    private static void NotFound(RequestContext context)
    {
        context.StatusCode = 404;
        context.Response = "Not Found";
    }
}
