# Модуль 15 — задачи

> Код пишется в `starter/Module15.Api/`. Тесты: `cd modules/15-aspnet-core-webapi/starter && dotnet test`.
> Запустить API вручную: `dotnet run --project Module15.Api` → http://localhost:5015/api/ping. Готовые запросы — в `Module15.Api/Module15.Api.http`.

**Проект «Task tracker».** Готово: модели и DTO (`Models/`), in-memory репозиторий (`Repositories/`), `Program.cs` со всеми регистрациями и middleware, эндпоинты `GET /api/tasks/{id}`, `POST /api/tasks/{id}/complete`, `GET /api/ping`, диагностические эндпоинты. Тесты `GettingStartedTests` зелёные с самого начала — посмотри их первыми: так выглядит интеграционный тест через `WebApplicationFactory<Program>`.

Почему тесты остальных задач падают с кодом 500: обработчики бросают `NotImplementedException`, а обработчик ошибок (задача 5) тоже ещё не написан.

---

## Задача 1. CRUD-эндпоинты  ·  ⏱ ~40 мин  ·  🟢 базовая

**Что сделать:** в `Endpoints/TaskEndpoints.cs` реализуй `Create` (POST → 201 + Location), `Update` (PUT → 204 / 404) и `Delete` (DELETE → 204 / 404). Точные правила — в XML-комментариях. Валидацию из задачи 2 можно пока не вызывать (тесты задачи 1 шлют корректные данные), но в сигнатуре уже есть `ValidationProblem` — вернёшься к ней.
**Зачем:** REST-семантика (метод + статус) — база любой вакансии бэкенда. На собеседовании спрашивают «чем PUT отличается от POST», «какой код вернуть после создания».
**Файл:** `Module15.Api/Endpoints/TaskEndpoints.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~TaskCrudTests"`
**Подумай / ответь письменно:**
1. Почему POST возвращает 201 и заголовок `Location`, а не просто 200?
2. Почему `Delete` лучше сразу вызвать `repository.Delete(id)` и посмотреть на результат, а не сначала `GetById`, а потом `Delete`?

## Задача 2. Валидация → 400 ProblemDetails  ·  ⏱ ~30 мин  ·  🟢 базовая

**Что сделать:** реализуй `TaskValidator.Validate` (ключи ошибок `Title`, `Description`, `Priority`) и используй его в `Create` и `Update`: при ошибках — `TypedResults.ValidationProblem(errors)`.
**Зачем:** валидация на сервере обязательна (клиенту доверять нельзя), а формат ошибок `ValidationProblemDetails` — стандарт, который понимают фронтенд-библиотеки.
**Файл:** `Module15.Api/Validation/TaskValidator.cs` (+ `TaskEndpoints.cs`)
**Тесты:** `dotnet test --filter "FullyQualifiedName~TaskValidationTests"`
**Подумай / ответь письменно:**
1. Почему валидатор собирает ВСЕ ошибки, а не возвращается на первой?
2. Почему валидатор — отдельный статический класс, а не код внутри эндпоинта? Какие тесты это упростило?

## Задача 3. Свой middleware: X-Request-Id  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуй `RequestIdMiddleware.IsValidIncomingId` и `InvokeAsync`: взять корректный id из заголовка запроса или сгенерировать `Guid.NewGuid().ToString("N")`, записать в заголовок ответа и в `context.TraceIdentifier`, вызвать следующий middleware. Middleware уже подключён в `Program.cs` — посмотри, на каком месте и почему.
**Зачем:** correlation id — реальная практика: по нему ищут все логи одного запроса в распределённой системе. А вопрос «что такое middleware и почему важен порядок» — один из самых частых по ASP.NET Core.
**Файл:** `Module15.Api/Middleware/RequestIdMiddleware.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~RequestIdMiddlewareTests"`
**Подумай / ответь письменно:**
1. Что случится, если записать заголовок ПОСЛЕ `await _next(context)`? Как сделать «после», если очень нужно (например, время обработки)?
2. Почему нельзя просто копировать любой входящий заголовок в ответ и в логи?

## Задача 4. DI lifetimes  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** в `AddLifetimeDemo` зарегистрируй `SingletonCounter` как Singleton, `ScopedCounter` как Scoped, `TransientCounter` как Transient. **До запуска тестов** предскажи письменно, что вернёт `GET /api/diagnostics/counters` в первом и во втором запросе (см. `DiagnosticsEndpoints.GetCounters`), потом проверь.
**Зачем:** «Какие бывают lifetimes?» — вопрос №1 по ASP.NET Core. Нужно не только назвать, но и объяснить поведение на примере.
**Файл:** `Module15.Api/Diagnostics/LifetimeDemoRegistration.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~LifetimeTests"`
**Подумай / ответь письменно:**
1. Что будет, если `SingletonCounter` начнёт принимать `ScopedCounter` в конструктор? Что сделает ASP.NET Core в Development и почему это ошибка?
2. Почему `InMemoryTaskRepository` зарегистрирован как Singleton, а `DbContext` (модуль 16) обычно Scoped?

## Задача 5. Глобальная обработка ошибок → ProblemDetails  ·  ⏱ ~35 мин  ·  🟡 средняя

**Что сделать:** реализуй `GlobalExceptionHandler.CreateProblem` (404 / 409 / 500) и `TryHandleAsync` (построить ProblemDetails, залогировать, записать ответ, вернуть `true`). Проверь вручную `POST /api/tasks/999/complete` и `GET /api/diagnostics/crash`.
**Зачем:** централизованная обработка ошибок — обязательная часть любого продакшен-API: единый формат, правильные коды, никаких утечек stack trace и строк подключения.
**Файл:** `Module15.Api/ErrorHandling/GlobalExceptionHandler.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ErrorHandlingTests"`
**Подумай / ответь письменно:**
1. Почему для 500 мы не кладём `exception.Message` в `Detail`, а для 404 — кладём?
2. В `GetById` «не найдено» — это `return TypedResults.NotFound()`, а в `Complete` — `throw`. Какие плюсы и минусы у каждого подхода? Что бы ты выбрал в своём проекте?

## Задача 6. Фильтрация и пагинация  ·  ⏱ ~50 мин  ·  ⭐ со звёздочкой

**Что сделать:** реализуй `TaskQuery.Validate`, `TaskQuery.Apply` и эндпоинт `GetTasks` (`GET /api/tasks?status=active&search=молоко&page=1&pageSize=10`). Значение по умолчанию и максимум для `pageSize` бери из `IOptions<TaskTrackerOptions>` (секция `TaskTracker` в `appsettings.json`). Ответ — `PagedResponse<TaskResponse>` с `TotalCount`.
**Зачем:** пагинация и фильтры есть в любом реальном API со списками; это частая live-coding задача («напиши метод, который возвращает N-ю страницу отсортированных данных»). Плюс Options pattern на практике.
**Файл:** `Module15.Api/Querying/TaskQuery.cs` (+ `GetTasks` в `TaskEndpoints.cs`)
**Тесты:** `dotnet test --filter "FullyQualifiedName~TaskQueryTests"`
**Подумай / ответь письменно:**
1. Зачем сортировке «тай-брейкер» `ThenBy(t => t.Id)`? Что будет с пагинацией без него?
2. Почему `TotalCount` считается после фильтрации, но до `Skip/Take`? Зачем вообще ограничивать максимальный `pageSize`?
3. Посмотри тест `GetTasks_PageSizeAboveConfiguredMax_Returns400`: как он подменяет настройку и почему `PostConfigure`, а не `Configure`?

---

## 🧪 Бонус (без тестов). Controllers-версия

Создай рядом `TasksController : ControllerBase` с `[ApiController]`, `[Route("api/v2/tasks")]` и одним действием `GET {id:int}` через `ActionResult<TaskResponse>` (не забудь `builder.Services.AddControllers()` и `app.MapControllers()`). Сравни с Minimal API: что стало проще, что сложнее? Это пригодится — в большинстве существующих проектов именно контроллеры.
