using Module15.Api.Configuration;
using Module15.Api.Diagnostics;
using Module15.Api.Endpoints;
using Module15.Api.ErrorHandling;
using Module15.Api.Middleware;
using Module15.Api.Repositories;

// ===== 1. Builder: конфигурация + регистрация сервисов в DI-контейнере =====
var builder = WebApplication.CreateBuilder(args);

// Options pattern: секция "TaskTracker" из appsettings.json → IOptions<TaskTrackerOptions>.
builder.Services.Configure<TaskTrackerOptions>(builder.Configuration.GetSection(TaskTrackerOptions.SectionName));

// Singleton: in-memory «база» должна жить между запросами.
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

// Задача 4: регистрация счётчиков с разными lifetimes.
builder.Services.AddLifetimeDemo();

// Задача 5: ProblemDetails + глобальный обработчик исключений.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// ===== 2. Middleware pipeline: ПОРЯДОК ВАЖЕН — запрос проходит сверху вниз, ответ — снизу вверх =====

// Задача 3: первым — чтобы id был у всех запросов, включая те, что упадут дальше.
app.UseMiddleware<RequestIdMiddleware>();

// Ловит исключения из всего, что подключено НИЖЕ, и отдаёт ProblemDetails.
app.UseExceptionHandler();

// ===== 3. Endpoints (маршруты) =====
app.MapGet("/api/ping", () => "pong");
app.MapTaskEndpoints();
app.MapDiagnosticsEndpoints();

app.Run();

// Нужен для WebApplicationFactory<Program> в интеграционных тестах:
// класс Program, который компилятор генерирует для top-level statements, делаем явно public.
// (В .NET 10 его делает public генератор исходного кода, но явное объявление понятнее и ничему не мешает.)
public partial class Program
{
}
