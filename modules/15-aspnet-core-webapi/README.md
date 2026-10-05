# Модуль 15. ASP.NET Core Web API

> **Зачем:** почти каждая вакансия Junior .NET — это бэкенд на ASP.NET Core: HTTP API для фронтенда, мобильного приложения или других сервисов. На собеседовании почти гарантированно спросят про DI lifetimes, middleware pipeline и REST. **Время:** ~10–12 ч. **Нужно:** модули 04–14 (особенно 07 — исключения, 13 — async, 14 — тесты).

## Что ты будешь уметь

- Объяснять, как HTTP-запрос проходит через ASP.NET Core: host → middleware pipeline → routing → endpoint → ответ.
- Писать REST API на Minimal APIs: CRUD, правильные status codes, model binding, валидация.
- Регистрировать сервисы в DI-контейнере и выбирать lifetime (Singleton / Scoped / Transient), объяснять captive dependency.
- Писать свой middleware и понимать, почему порядок `app.UseXxx()` важен.
- Читать настройки через Options pattern, логировать через `ILogger<T>`.
- Обрабатывать ошибки централизованно и отдавать ProblemDetails.
- Писать интеграционные тесты API через `WebApplicationFactory<Program>`.

---

## 1. HTTP и REST за 10 минут

**HTTP** — текстовый протокол «запрос → ответ». Запрос: **метод** (GET, POST…), **путь** (`/api/tasks/5`), **заголовки** (headers — метаданные: `Content-Type`, `Authorization`), необязательное **тело** (body, обычно JSON). Ответ: **status code**, заголовки, тело.

```http
POST /api/tasks HTTP/1.1
Content-Type: application/json

{ "title": "Купить молоко", "priority": 2 }
```
```http
HTTP/1.1 201 Created
Location: /api/tasks/1
Content-Type: application/json

{ "id": 1, "title": "Купить молоко", "description": null, "priority": 2, "isCompleted": false }
```

**REST** (Representational State Transfer) — стиль проектирования API: есть **ресурсы** (задачи, заказы), у каждого — адрес (URL), а действие выражается **HTTP-методом**, а не словом в URL.

| Действие | Метод и путь | Успех | Типичные ошибки |
|---|---|---|---|
| Список | `GET /api/tasks` | 200 OK | 400 (плохие параметры) |
| Один | `GET /api/tasks/5` | 200 OK | 404 Not Found |
| Создать | `POST /api/tasks` | 201 Created + `Location` | 400 Bad Request |
| Заменить | `PUT /api/tasks/5` | 204 No Content (или 200) | 400, 404 |
| Частично изменить | `PATCH /api/tasks/5` | 200/204 | 400, 404 |
| Удалить | `DELETE /api/tasks/5` | 204 No Content | 404 |

Группы status codes: **2xx** — успех, **3xx** — редирект, **4xx** — ошибка клиента (он прислал что-то не то), **5xx** — ошибка сервера (виноваты мы). Самые важные: 200, 201, 204, 400, 401 (не аутентифицирован — «кто ты?»), 403 (нет прав — «знаю, кто ты, но нельзя»), 404, 409 (конфликт с текущим состоянием), 500.

**Идемпотентность** (idempotency — повтор запроса не меняет результат): GET, PUT, DELETE — идемпотентны, POST — нет (два POST = две задачи). **Безопасные** (safe) методы ничего не меняют: GET, HEAD.

> **Почему так?** Единые правила (метод + статус) позволяют клиентам, прокси, кэшам и браузерам понимать API без документации: GET можно кэшировать и повторять, 5xx — повод для retry, 4xx — нет (повтор не поможет). Альтернатива — «RPC-стиль» (`POST /api/createTask`, `POST /api/deleteTask`) — проще на старте, но теряет эти гарантии. Для внутренних высоконагруженных сервисов используют gRPC — это нормально, REST не единственный вариант.

> 💼 **На собеседовании:** «Чем PUT отличается от POST?» — POST создаёт новый ресурс (не идемпотентен, сервер сам назначает id), PUT заменяет ресурс по известному адресу целиком (идемпотентен). PATCH — частичное изменение. «401 vs 403?» — 401: не знаем, кто ты; 403: знаем, но прав нет.

---

## 2. Хост и Program.cs

ASP.NET Core-приложение — это обычное консольное приложение, которое запускает **host** (хост — объект, который владеет DI-контейнером, конфигурацией, логированием и веб-сервером **Kestrel**). Всё описано в `Program.cs` тремя шагами:

```csharp
var builder = WebApplication.CreateBuilder(args);   // 1. конфигурация + сервисы
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

var app = builder.Build();                           // 2. pipeline: middleware
app.UseExceptionHandler();

app.MapGet("/api/ping", () => "pong");               // 3. endpoints
app.Run();                                           // запуск Kestrel, ждём запросы
```

`CreateBuilder` уже подключает: конфигурацию из `appsettings.json`, `appsettings.{Environment}.json`, переменных окружения и аргументов командной строки; логирование в консоль; DI-контейнер. **Environment** (окружение) задаётся переменной `ASPNETCORE_ENVIRONMENT`: `Development`, `Staging`, `Production` (по умолчанию). В `Development`, например, включена страница с подробностями исключений.

> **Почему так?** Раньше (ASP.NET Core 2–5) были `Program` + `Startup` с методами `ConfigureServices` и `Configure`. С .NET 6 это «minimal hosting»: то же самое, но в одном файле и без церемоний. Разделение на 2 фазы осталось: сначала *регистрируем* сервисы (`builder.Services`), потом `Build()` фиксирует контейнер, и дальше *используем* (`app.Use…`, `app.Map…`). Добавить сервис после `Build()` нельзя — контейнер уже построен.

> 💼 **На собеседовании:** «Что такое Kestrel?» — кроссплатформенный встроенный веб-сервер ASP.NET Core. В продакшене часто стоит за reverse proxy (nginx, IIS, балансировщик облака), который делает TLS, балансировку и т.п.

---

## 3. Dependency Injection и lifetimes

**Dependency Injection** (DI, внедрение зависимостей) — класс не создаёт свои зависимости через `new`, а получает их снаружи (обычно через конструктор). **DI-контейнер** (IoC container) — объект, который знает, «какой класс создать для какого интерфейса», и сам собирает граф объектов.

```csharp
// Регистрация: "когда просят ITaskRepository — дай InMemoryTaskRepository"
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IPriceCalculator, PriceCalculator>();

// Использование: контейнер сам передаст зависимости в конструктор
public sealed class OrderService(ITaskRepository repository, ILogger<OrderService> logger) : IOrderService { }
```

Три **lifetime** (время жизни экземпляра):

| Lifetime | Сколько экземпляров | Когда использовать |
|---|---|---|
| **Singleton** | один на всё приложение | stateless-сервисы, кэши, конфигурация, `HttpClient`-фабрики. Должен быть **потокобезопасным**! |
| **Scoped** | один на **scope**; в ASP.NET Core scope = один HTTP-запрос | `DbContext` (EF Core), «единица работы» запроса, данные текущего пользователя |
| **Transient** | новый при каждом запросе к контейнеру | лёгкие stateless-сервисы без состояния |

```csharp
// Внутри ОДНОГО HTTP-запроса:
var a = services.GetRequiredService<ScopedCounter>();
var b = services.GetRequiredService<ScopedCounter>();
Console.WriteLine(ReferenceEquals(a, b)); // вывод: True  (Scoped — тот же в рамках запроса)
// Для Transient было бы False, для Singleton — True даже между разными запросами.
```

**Captive dependency** (захваченная зависимость) — классическая ловушка: Singleton зависит от Scoped. Singleton создаётся один раз и **навсегда захватывает** тот scoped-объект, который получил при создании. Например, `DbContext` первого запроса будет жить вечно и использоваться всеми запросами параллельно → баги и исключения (`DbContext` не потокобезопасен).

```csharp
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddSingleton<ReportCache>(); // ReportCache(AppDbContext db) — ❌ captive dependency
```

В окружении Development ASP.NET Core включает **scope validation** и бросит исключение при старте: *Cannot consume scoped service 'AppDbContext' from singleton 'ReportCache'*. Правило: зависимость должна жить **не меньше**, чем тот, кто её использует. Singleton → только Singleton; Scoped → Scoped или Singleton; Transient → любые. Если Singleton действительно нужен scoped-сервис — внедряют `IServiceScopeFactory` и создают scope вручную на время операции.

> **Почему так?** DI нужен ради слабой связанности (loose coupling): класс зависит от абстракции, реализацию можно подменить (in-memory → EF Core, реальная оплата → фейк в тестах) без правки самого класса. Это буква **D** из SOLID (модуль 17). Lifetimes — компромисс между памятью/скоростью (Singleton дешевле — создаётся один раз) и безопасностью (Scoped/Transient не делят состояние между запросами и потоками).

> 💼 **На собеседовании:** «Какие lifetimes есть в DI и чем отличаются?» — это **самый частый** вопрос по ASP.NET Core. Ответь таблицей выше + пример (`DbContext` — Scoped) + captive dependency. Бонус: «Transient, реализующий `IDisposable`, резолвнутый из корневого контейнера, живёт до конца приложения — утечка памяти».

→ **Задача 4** (см. exercises.md)

---

## 4. Middleware pipeline

**Middleware** (промежуточное ПО) — компонент, через который проходит каждый запрос. Middleware выстроены в **pipeline** (конвейер): каждый может что-то сделать **до** следующего, вызвать следующий (`next`) и что-то сделать **после** — на обратном пути ответа. Аналогия: матрёшка или слои лука.

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine("1: до");      // запрос идёт «внутрь»
    await next(context);
    Console.WriteLine("1: после");   // ответ идёт «наружу»
});
app.Use(async (context, next) =>
{
    Console.WriteLine("2: до");
    await next(context);
    Console.WriteLine("2: после");
});
app.Run(async context => await context.Response.WriteAsync("Hello"));
// вывод: 1: до / 2: до / 2: после / 1: после   (внутрь — сверху вниз, наружу — в обратном порядке)
```

- `app.Use(...)` — middleware, который может вызвать следующий.
- `app.Run(...)` — **терминальный** (terminal) middleware: следующий не вызывается, pipeline заканчивается.
- `app.Map("/path", branch => ...)` — ветвление pipeline по пути.
- `app.UseMiddleware<T>()` — подключить middleware-класс.

Middleware-класс — по соглашению: конструктор принимает `RequestDelegate next`, есть метод `InvokeAsync(HttpContext context)`:

```csharp
public sealed class TimingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        context.Response.OnStarting(() =>  // заголовки можно менять только до начала отправки ответа
        {
            var ms = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            context.Response.Headers["X-Elapsed-Ms"] = ms.ToString("0", CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });
        await next(context);
    }
}
```

**Порядок важен.** Типичный порядок в реальном приложении:

```csharp
app.UseExceptionHandler();   // 1. первым — ловит исключения из всего, что ниже
app.UseHttpsRedirection();
app.UseStaticFiles();        // статика отдаётся без лишней работы
app.UseRouting();            // определить endpoint (в minimal hosting вызывается неявно)
app.UseCors();
app.UseAuthentication();     // кто ты?
app.UseAuthorization();      // можно ли тебе? (после аутентификации!)
app.MapControllers();        // endpoints
```

> **Почему так?** Pipeline — это композиция функций: каждый middleware отвечает за одну сквозную задачу (cross-cutting concern: логирование, ошибки, аутентификация, CORS, сжатие), и их можно собирать как конструктор. Цена — порядок нужно понимать: если `UseAuthorization` стоит до `UseAuthentication`, авторизация не знает пользователя; если обработчик ошибок стоит последним — он не увидит исключений из middleware выше.

> 💼 **На собеседовании:** «Что такое middleware? Чем `Use` отличается от `Run`?» — компонент pipeline с доступом к `HttpContext` и `next`; `Run` — терминальный. «Почему порядок важен?» — запрос идёт сверху вниз, ответ — обратно; пример с Authentication/Authorization. Middleware vs filter: filters (в MVC) работают только внутри MVC/endpoint и знают про action/модель; middleware — для всех запросов.

→ **Задача 3**

---

## 5. Routing и Minimal APIs vs Controllers

**Routing** (маршрутизация) — сопоставление URL и метода с **endpoint** (обработчиком). Шаблоны маршрута: `/api/tasks/{id}`, с ограничениями (route constraints): `{id:int}`, `{slug:alpha}`, необязательные: `{page?}`.

**Minimal APIs** (.NET 6+) — эндпоинт = делегат или метод:

```csharp
var group = app.MapGroup("/api/tasks");                    // общий префикс
group.MapGet("/{id:int}", GetById);                        // GET /api/tasks/5
group.MapPost("/", Create);

static Results<Ok<TaskResponse>, NotFound> GetById(int id, ITaskRepository repo)
{
    var task = repo.GetById(id);
    if (task is null) return TypedResults.NotFound();
    return TypedResults.Ok(task.ToResponse());
}
```

**Controllers** (MVC) — классический подход: класс-наследник `ControllerBase` с атрибутами:

```csharp
[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskRepository repo) : ControllerBase
{
    [HttpGet("{id:int}")]
    public ActionResult<TaskResponse> GetById(int id)
    {
        var task = repo.GetById(id);
        return task is null ? NotFound() : Ok(task.ToResponse());
    }
}
// Program.cs: builder.Services.AddControllers(); ... app.MapControllers();
```

| | Minimal APIs | Controllers |
|---|---|---|
| Код | меньше церемоний, функции | классы, атрибуты, наследование |
| Фичи | filters, groups, typed results | model validation `[ApiController]`, filters, conventions, огромная экосистема |
| Где встретишь | новые сервисы, микросервисы | большинство существующих проектов |

> **Почему так?** Controllers появились первыми (из ASP.NET MVC) и несут много «магии» по соглашениям. Minimal APIs сделали, чтобы HTTP API можно было написать в несколько строк и с меньшими накладными расходами. Microsoft поддерживает оба подхода; на работе ты встретишь контроллеры почти наверняка, поэтому их синтаксис нужно узнавать, даже если пишешь на Minimal APIs. В курсе — Minimal APIs: меньше кода между тобой и HTTP.

> 💼 **На собеседовании:** «Controllers или Minimal APIs?» — нет «правильного» ответа; назови компромисс из таблицы и скажи, что умеешь оба. Вопрос «`IActionResult` vs `ActionResult<T>`»: второй типизирован — и документирует тип тела для OpenAPI, и позволяет вернуть `T` напрямую.

---

## 6. Model binding и валидация

**Model binding** (привязка модели) — ASP.NET Core сам достаёт значения параметров обработчика из запроса:

| Источник | Как определяется (Minimal APIs) | Явный атрибут |
|---|---|---|
| Маршрут | имя совпадает с `{id}` в шаблоне | `[FromRoute]` |
| Query string | простой тип, не из маршрута (`?page=2`) | `[FromQuery]` |
| Тело (JSON) | сложный тип в POST/PUT | `[FromBody]` |
| Заголовок | — | `[FromHeader(Name = "X-Request-Id")]` |
| DI-контейнер | тип зарегистрирован в контейнере | `[FromServices]` |
| Специальные | `HttpContext`, `CancellationToken`, `ClaimsPrincipal` | — |

```csharp
// GET /api/tasks?status=active&page=2  →  status = "active", page = 2, pageSize = null
group.MapGet("/", (string? status, int? page, int? pageSize, ITaskRepository repo) => ...);
```

Если JSON не парсится или `?page=abc` — фреймворк сам вернёт **400**. А вот **бизнес-валидацию** (название не пустое, приоритет 1–5) делаешь ты. Варианты: вручную (как в задаче 2), DataAnnotations (`[Required]`, `[Range(1, 5)]` — в контроллерах с `[ApiController]` проверяются автоматически; в .NET 10 для Minimal APIs появился `builder.Services.AddValidation()`), библиотека **FluentValidation** (популярна в проектах).

Ответ на ошибку валидации — **400** с телом `ValidationProblemDetails`:

```csharp
var errors = new Dictionary<string, string[]> { ["Title"] = ["Название обязательно."] };
return TypedResults.ValidationProblem(errors);
// 400, Content-Type: application/problem+json
// { "title": "One or more validation errors occurred.", "status": 400, "errors": { "Title": ["Название обязательно."] } }
```

> **Почему так?** Привязка по соглашениям убирает рутину «достань из Request.Query, распарси, проверь null». Валидация на сервере обязательна, даже если фронтенд тоже проверяет: клиенту нельзя доверять — запрос можно отправить curl'ом в обход любой формы. Формат ошибок стандартизован (RFC 9457 ProblemDetails), чтобы клиенты разбирали их одинаково.

> 💼 **На собеседовании:** «Откуда берутся значения параметров action/handler?» — model binding: route, query, body, headers, services; можно явно указать `[FromXxx]`. «Почему валидировать на сервере, если валидирует фронтенд?» — клиенту доверять нельзя.

→ **Задача 1**, **Задача 2**

---

## 7. Результаты: IResult, TypedResults, ActionResult

В Minimal APIs обработчик возвращает:
- обычный объект → **200** + JSON (`return task;`);
- `string` → 200 + `text/plain`;
- `IResult` — явный ответ: `Results.Ok(x)`, `Results.NotFound()`, `Results.Created(uri, x)`, `Results.NoContent()`, `Results.BadRequest()`, `Results.Problem(...)`, `Results.ValidationProblem(...)`.

`TypedResults` — то же, но возвращает **конкретные типы** (`Ok<T>`, `NotFound`), а `Results<T1, T2, …>` объединяет варианты:

```csharp
static Results<Created<TaskResponse>, ValidationProblem> Create(CreateTaskRequest req, ITaskRepository repo) { ... }
```

> **Почему так?** `Results<…>` — это и документация (по сигнатуре видно все возможные ответы), и проверка компилятором (вернуть `Ok<string>` вместо `Ok<TaskResponse>` не выйдет), и метаданные для OpenAPI. Плюс typed results легко проверять в unit-тестах: `Assert.IsType<NotFound>(result.Result)`. В контроллерах аналог — `ActionResult<T>` и методы `Ok()`, `NotFound()`, `CreatedAtAction()`.

---

## 8. Конфигурация и Options pattern

Конфигурация — это «словарь» ключ-значение, собранный из нескольких **источников** (providers). Более поздний источник перекрывает ранний: `appsettings.json` → `appsettings.Development.json` → user secrets (в Development) → переменные окружения → аргументы командной строки.

```json
{ "TaskTracker": { "DefaultPageSize": 10, "MaxPageSize": 50 } }
```
```csharp
var max = builder.Configuration["TaskTracker:MaxPageSize"];   // "50" — строка, вложенность через ':'
// Переменная окружения для того же ключа: TaskTracker__MaxPageSize=100  (двойное подчёркивание)
```

**Options pattern** — привязать секцию к классу и получать его через DI:

```csharp
public sealed class TaskTrackerOptions
{
    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 50;
}

builder.Services.Configure<TaskTrackerOptions>(builder.Configuration.GetSection("TaskTracker"));

// где угодно:
public sealed class SomeService(IOptions<TaskTrackerOptions> options)
{
    private readonly int _max = options.Value.MaxPageSize;
}
```

Три интерфейса: `IOptions<T>` (Singleton, значение читается один раз), `IOptionsSnapshot<T>` (Scoped, перечитывается на каждый запрос — видит изменения файла), `IOptionsMonitor<T>` (Singleton, всегда актуальное значение + событие `OnChange`).

> **Почему так?** Типизированный класс вместо строковых ключей: опечатка в имени свойства — ошибка компиляции, а не `null` в рантайме; настройки можно валидировать при старте (`.ValidateDataAnnotations().ValidateOnStart()`); в тестах легко подменить (`PostConfigure`). Секреты (пароли, ключи API) **не кладут** в appsettings.json в git — для разработки есть user secrets (`dotnet user-secrets set`), в продакшене — переменные окружения или хранилища секретов (Azure Key Vault и т.п.).

> 💼 **На собеседовании:** «Как в ASP.NET Core читать настройки?» — `IConfiguration` + Options pattern; назови порядок источников и `IOptions` vs `IOptionsSnapshot` vs `IOptionsMonitor`. «Где хранить пароль от БД?» — не в git: user secrets / env vars / secret store.

→ **Задача 6** (использует `IOptions<TaskTrackerOptions>`)

---

## 9. Logging

Логгер внедряется как `ILogger<T>` (категория = имя класса T):

```csharp
public sealed class OrderService(ILogger<OrderService> logger)
{
    public void Ship(int orderId)
    {
        logger.LogInformation("Заказ {OrderId} отправлен", orderId);  // ✅ structured logging
        // logger.LogInformation($"Заказ {orderId} отправлен");       // ❌ интерполяция
    }
}
```

Уровни: `Trace` < `Debug` < `Information` < `Warning` < `Error` < `Critical`. Минимальный уровень задаётся в `appsettings.json` (`Logging:LogLevel`) — отдельно для категорий (`"Microsoft.AspNetCore": "Warning"`).

> **Почему так?** **Structured logging** (структурированное логирование): шаблон `"Заказ {OrderId}"` + аргументы сохраняются как поля, а не склеенная строка. В системах сбора логов (Seq, Elastic, Application Insights) можно искать «все записи с OrderId = 42». С интерполяцией `$"..."` строка собирается всегда (даже если уровень отключён — лишняя аллокация) и поле OrderId теряется. Для горячих путей есть ещё более быстрый вариант — source-generated `[LoggerMessage]`.

---

## 10. Обработка ошибок и ProblemDetails

Без обработчика необработанное исключение даёт 500 с пустым телом (в Production) или страницу с подробностями (в Development — **никогда** не включай её в продакшене: там stack trace и внутренности). Правильно — **централизованный** обработчик, который превращает исключения в единый формат **ProblemDetails** (RFC 9457):

```json
{ "type": "...", "title": "Ресурс не найден", "status": 404, "detail": "Задача с id=42 не найдена", "traceId": "..." }
```

В .NET 8+ для этого есть `IExceptionHandler`:

```csharp
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var problem = ex is KeyNotFoundException
            ? new ProblemDetails { Status = 404, Title = "Не найдено" }
            : new ProblemDetails { Status = 500, Title = "Внутренняя ошибка сервера" };
        await TypedResults.Problem(problem).ExecuteAsync(ctx);
        return true; // обработано
    }
}

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();   // middleware, который вызывает наш обработчик
```

Два стиля сообщать об ошибке из бизнес-логики:
1. **Явные результаты** — `return TypedResults.NotFound()` (или Result-тип из модуля 07). Поток управления виден в сигнатуре.
2. **Исключения + глобальный обработчик** — `throw new TaskNotFoundException(id)`; меньше кода в эндпоинтах, но «невидимый» поток управления и цена исключений.

Обычно: ожидаемые ситуации (не найдено, не прошла валидация) — явными результатами; нарушенные инварианты и неожиданные сбои — исключениями. В нашем проекте есть оба варианта: сравни `GetById` и `Complete`.

> **Почему так?** Централизованный обработчик — это DRY (одно место вместо try/catch в каждом эндпоинте) и безопасность: гарантированно не утечёт stack trace или строка подключения. Единый формат ProblemDetails позволяет фронтенду обрабатывать все ошибки одним кодом. `traceId` связывает ответ клиента с записью в логах.

> 💼 **На собеседовании:** «Как ты обрабатываешь ошибки в Web API?» — глобально: `UseExceptionHandler` + `IExceptionHandler` (или свой middleware с try/catch вокруг `next`), маппинг типов исключений на status codes, ProblemDetails, логирование 5xx как Error, никаких деталей исключения наружу.

→ **Задача 5**

---

## 11. OpenAPI / Swagger (обзорно)

**OpenAPI** — стандартное машиночитаемое описание API (эндпоинты, параметры, модели). **Swagger UI** — веб-страница, где по этому описанию можно «потыкать» API. В .NET 9+ шаблон `dotnet new webapi` использует встроенный пакет `Microsoft.AspNetCore.OpenApi` (`builder.Services.AddOpenApi(); app.MapOpenApi();` → документ на `/openapi/v1.json`); раньше почти все проекты использовали Swashbuckle. UI подключают отдельно (Swagger UI, Scalar). В нашем starter OpenAPI нет — чтобы не тянуть лишние пакеты; для ручных запросов есть файл `Module15.Api.http`.

> **Почему так?** Описание API, сгенерированное из кода, не устаревает (в отличие от вики) и позволяет автоматически генерировать клиентов (TypeScript, C#). Именно поэтому полезны `TypedResults`: из них берутся метаданные ответов.

---

## 12. Интеграционные тесты: WebApplicationFactory

**Unit-тест** проверяет класс в изоляции (модуль 14). **Интеграционный** — несколько частей вместе: здесь — весь pipeline от HTTP-запроса до ответа. `WebApplicationFactory<Program>` (пакет `Microsoft.AspNetCore.Mvc.Testing`) запускает приложение **в памяти** (TestServer) и даёт `HttpClient`:

```csharp
public sealed class TaskApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();
    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetMissingTask_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

- `PostAsJsonAsync`, `GetFromJsonAsync`, `ReadFromJsonAsync<T>` — из `System.Net.Http.Json`.
- Подменить сервисы для теста: `_factory.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<IClock>(new FakeClock())))`.
- В `Program.cs` нужно `public partial class Program { }` — тестовый проект должен «видеть» класс `Program` (для top-level statements компилятор генерирует его неявно; в .NET 10 он уже public, но явная строка не мешает и работает везде).
- `factory.Services` — корневой `IServiceProvider` приложения: удобно подготовить данные (`GetRequiredService<ITaskRepository>()`).

> **Почему так?** Unit-тесты эндпоинтов не поймают ошибки «склейки»: неверный порядок middleware, забытую регистрацию в DI, неправильный маршрут или формат JSON. TestServer проходит весь pipeline, но без сети и портов — тесты быстрые и стабильные. В реальных проектах базу для таких тестов поднимают в Docker (Testcontainers) — это следующий шаг.

---

## Типичные ошибки

| Ошибка | Последствие | Как правильно |
|---|---|---|
| Singleton зависит от Scoped (captive dependency) | один `DbContext` на все запросы, гонки | lifetime зависимости ≥ lifetime потребителя; `IServiceScopeFactory` |
| Изменяемое состояние в Singleton без синхронизации | race conditions под нагрузкой | `lock`, `Interlocked`, `ConcurrentDictionary` или не хранить состояние |
| `UseAuthorization` до `UseAuthentication`; обработчик ошибок не первым | пользователь «аноним», исключения мимо обработчика | помнить порядок pipeline |
| Менять заголовки после `await next()` | `InvalidOperationException: Headers are read-only, response has already started` | до `next` или через `Response.OnStarting` |
| Возвращать 200 на всё, ошибки — в теле | клиенты, прокси, мониторинг не понимают, что была ошибка | правильные status codes |
| `POST` возвращает 200 без `Location` | клиент не знает адрес созданного ресурса | 201 Created + Location |
| Отдавать наружу доменные сущности / EF-модели | утечка лишних полей, API ломается при рефакторинге модели | DTO |
| `ex.Message` / stack trace в ответе 500 | утечка секретов и деталей реализации | ProblemDetails без деталей + лог |
| Пароли в `appsettings.json` в git | утечка доступа | user secrets / env vars / secret store |
| `.Result` / `.Wait()` в обработчиках | блокировка потоков ThreadPool, падение пропускной способности | `async`/`await` до конца |
| Интерполяция строк в логах | нет структурированных полей, лишние аллокации | шаблоны `{Name}` + аргументы |

## Шпаргалка

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IRepo, Repo>();          // 1 на приложение
builder.Services.AddScoped<IUow, Uow>();               // 1 на запрос
builder.Services.AddTransient<ICalc, Calc>();          // каждый раз новый
builder.Services.Configure<MyOptions>(builder.Configuration.GetSection("My"));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseMiddleware<RequestIdMiddleware>();

var g = app.MapGroup("/api/items");
g.MapGet("/{id:int}", (int id, IRepo r) => r.Find(id) is { } x ? Results.Ok(x) : Results.NotFound());
g.MapPost("/", (CreateDto dto, IRepo r) => Results.Created($"/api/items/{r.Add(dto)}", null));
app.Run();

public partial class Program { }
```

| Ситуация | Код |
|---|---|
| создано | 201 + Location |
| удалено / обновлено без тела | 204 |
| невалидный ввод | 400 ValidationProblem |
| нет ресурса | 404 |
| конфликт состояния (уже существует, уже завершено) | 409 |
| неожиданное исключение | 500 ProblemDetails без деталей |

## Глоссарий

| Термин | Что значит |
|---|---|
| Host | объект, владеющий DI, конфигурацией, логированием и сервером |
| Kestrel | встроенный кроссплатформенный веб-сервер ASP.NET Core |
| Middleware | компонент pipeline, обрабатывающий запрос до/после следующего |
| Pipeline | цепочка middleware, через которую проходит каждый запрос |
| Endpoint | конечный обработчик запроса (action контроллера, делегат Minimal API) |
| Routing | сопоставление URL + метода с endpoint |
| Model binding | автоматическое заполнение параметров обработчика из запроса |
| DI container | объект, создающий сервисы и их зависимости по регистрациям |
| Lifetime | Singleton / Scoped / Transient — время жизни экземпляра сервиса |
| Scope | область жизни Scoped-сервисов; в ASP.NET Core — один HTTP-запрос |
| Captive dependency | долгоживущий сервис захватывает короткоживущую зависимость |
| Options pattern | привязка секции конфигурации к классу, доступ через `IOptions<T>` |
| ProblemDetails | стандартный JSON-формат ошибок HTTP API (RFC 9457) |
| DTO | Data Transfer Object — класс-контракт для входа/выхода API |
| Idempotent | повтор запроса не меняет результат (GET, PUT, DELETE) |
| WebApplicationFactory | запуск приложения в памяти для интеграционных тестов |

## Вопросы на собеседовании

<details><summary><b>Какие lifetimes есть в DI-контейнере ASP.NET Core и когда какой использовать?</b></summary>

Singleton — один экземпляр на всё приложение (stateless-сервисы, кэши; обязан быть потокобезопасным). Scoped — один на scope, в ASP.NET Core — на HTTP-запрос (`DbContext`, unit of work). Transient — новый при каждом resolve (лёгкие stateless-сервисы). Правило: зависимость должна жить не меньше, чем потребитель, иначе captive dependency.
</details>

<details><summary><b>Что такое captive dependency и как её обнаружить?</b></summary>

Это когда сервис с длинным lifetime (Singleton) получает в конструктор сервис с коротким (Scoped/Transient) и удерживает его навсегда. Пример: Singleton-кэш с `DbContext` внутри — один контекст на все запросы, гонки и устаревшие данные. В Development ASP.NET Core включает `ValidateScopes` и бросает исключение при старте/резолве. Решения: поменять lifetime или внедрить `IServiceScopeFactory` и создавать scope на операцию.
</details>

<details><summary><b>Что такое middleware и почему важен порядок?</b></summary>

Middleware — звено pipeline: получает `HttpContext` и `next`, может работать до и после следующего звена или «замкнуть» pipeline (`Run`). Запрос проходит middleware сверху вниз, ответ — снизу вверх. Поэтому обработчик исключений ставят первым (чтобы ловить всё ниже), аутентификацию — до авторизации, статику — до тяжёлых middleware.
</details>

<details><summary><b>Чем отличаются `app.Use`, `app.Run` и `app.Map`?</b></summary>

`Use` добавляет middleware, который может вызвать `next`. `Run` — терминальный: `next` у него нет, на нём pipeline заканчивается. `Map` создаёт ветку pipeline для запросов с определённым префиксом пути. Для эндпоинтов используют `MapGet/MapPost/...` — это уже routing, а не ветвление pipeline.
</details>

<details><summary><b>Controllers или Minimal APIs — в чём разница?</b></summary>

Это два способа описать endpoints поверх одного и того же pipeline и routing. Controllers — классы с атрибутами, богатые соглашения (`[ApiController]` — автоматический 400 при невалидной модели), фильтры MVC; их больше всего в существующих проектах. Minimal APIs — лямбды/методы, меньше кода и накладных расходов, `TypedResults`, endpoint filters, route groups. Выбор — вопрос стиля и команды.
</details>

<details><summary><b>Как в ASP.NET Core устроена конфигурация?</b></summary>

`IConfiguration` собирается из провайдеров по порядку: appsettings.json → appsettings.{Environment}.json → user secrets (Development) → переменные окружения → аргументы командной строки; более поздние перекрывают ранние. Секции привязывают к классам через Options pattern (`Configure<T>` + `IOptions<T>`). `IOptionsSnapshot` перечитывает значения на каждый запрос, `IOptionsMonitor` — сразу и умеет уведомлять об изменениях.
</details>

<details><summary><b>Какой status code вернуть, если: ресурс создан; входные данные невалидны; ресурса нет; задача уже завершена; упала база?</b></summary>

201 Created (+ заголовок Location), 400 Bad Request (ValidationProblemDetails), 404 Not Found, 409 Conflict, 500 Internal Server Error (или 503, если сервис временно недоступен). Важно не возвращать 200 с `{ "error": ... }` — тогда клиенты и мониторинг не видят ошибку.
</details>

<details><summary><b>Как организовать глобальную обработку исключений?</b></summary>

`AddProblemDetails()` + `AddExceptionHandler<T>()` + `app.UseExceptionHandler()` в начале pipeline. В `IExceptionHandler.TryHandleAsync` сопоставляем тип исключения со status code, формируем ProblemDetails, логируем (5xx как Error), возвращаем true. Детали исключения клиенту не отдаём — только в логи, а в ответ кладём `traceId`. Альтернатива для старых версий — свой middleware с try/catch вокруг `await next(context)`.
</details>

<details><summary><b>Что такое model binding? Откуда берутся параметры?</b></summary>

Механизм, который заполняет параметры обработчика из запроса: route values, query string, тело (JSON), заголовки, форма, DI-контейнер и специальные типы (`HttpContext`, `CancellationToken`). В Minimal APIs источник выводится по соглашениям (простые типы — route/query, сложные — body, зарегистрированные — services), а явно задаётся атрибутами `[FromRoute]`, `[FromQuery]`, `[FromBody]`, `[FromHeader]`, `[FromServices]`.
</details>

<details><summary><b>Зачем DTO, если можно вернуть сущность напрямую?</b></summary>

DTO отделяет контракт API от внутренней модели: можно менять сущность, не ломая клиентов; не утекают лишние поля (хэш пароля, внутренние флаги); нет проблем с сериализацией навигационных свойств EF (циклы, ленивая загрузка); входные DTO защищают от over-posting (клиент не может прислать `IsAdmin = true`). Цена — маппинг (вручную или Mapster/AutoMapper).
</details>

<details><summary><b>Что произойдёт, если изменить заголовок ответа после `await next(context)`?</b></summary>

Если следующий middleware уже начал писать тело, заголовки отправлены клиенту, и попытка их изменить даст `InvalidOperationException` (headers are read-only). Поэтому заголовки ставят до вызова `next` или регистрируют callback `context.Response.OnStarting(...)`, который выполнится прямо перед отправкой заголовков.
</details>

<details><summary><b>Как тестировать Web API?</b></summary>

Unit-тестами — логику (валидаторы, сервисы) без HTTP. Интеграционными — через `WebApplicationFactory<Program>`: приложение поднимается в памяти, `HttpClient` проходит весь pipeline (routing, model binding, middleware, DI, сериализация). Зависимости подменяются через `WithWebHostBuilder` + `ConfigureTestServices`. Для настоящей БД — Testcontainers.
</details>

<details><summary><b>Почему в логах нужно писать `LogInformation("Order {Id}", id)`, а не `$"Order {id}"`?</b></summary>

Structured logging: шаблон и значения сохраняются раздельно, по полю `Id` можно искать и строить графики в системе логов. Интерполированная строка — просто текст, к тому же она собирается даже если уровень Information отключён (лишняя аллокация). Для горячих путей — `[LoggerMessage]` source generator.
</details>

## Дополнительно

- [ASP.NET Core fundamentals](https://learn.microsoft.com/aspnet/core/fundamentals/) — host, DI, middleware, configuration.
- [Dependency injection in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection) — lifetimes, scope validation.
- [ASP.NET Core Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/) — порядок middleware, написание своего.
- [Minimal APIs overview](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/overview) и [Parameter binding](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/parameter-binding).
- [Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api) — `IExceptionHandler`, ProblemDetails.
- [Options pattern](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options).
- [Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests).
- Книга: Andrew Lock, «ASP.NET Core in Action» — лучшее подробное введение.
