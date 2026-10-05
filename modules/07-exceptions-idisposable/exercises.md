# Модуль 07 — задачи

> Код пишется в `starter/Module07.Tasks/`. Тесты: `cd modules/07-exceptions-idisposable/starter && dotnet test`.
>
> Сигнатуры `public`-членов не меняй. Можно добавлять private-поля, private-конструкторы и методы, заменять `=> throw ...` на auto-properties.

## Задача 1. Валидация с собственным исключением  ·  ⏱ ~35 мин  ·  🟢 базовая

**Что сделать:** реализуй `ValidationException` (наследник `Exception` со свойством `PropertyName` и двумя конструкторами, второй — с `innerException`) и `RegistrationValidator`:
- `Validate` — бросает `ValidationException` на **первой** ошибке (порядок: Username → Email → Age); `null`-запрос → `ArgumentNullException`;
- `GetErrors` — то же без исключений: список **всех** невалидных полей.
Правила — в XML-комментариях.

**Зачем:** проектирование своего исключения по конвенциям .NET, разница между «ошибкой программиста» (`ArgumentNullException`) и «ошибкой данных» (`ValidationException`), сравнение подхода «исключение» и «сбор ошибок».
**Файл:** `Module07.Tasks/Validation.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ValidationTests"`
**Подумай / ответь письменно:**
1. Почему `null`-запрос — `ArgumentNullException`, а не `ValidationException`?
2. Когда пользователю удобнее `Validate`, а когда — `GetErrors`? Как сделать так, чтобы правила не дублировались?

## Задача 2. Парсер конфигурации  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** реализуй `AppConfig.Parse` для формата `key=value` (комментарии `#`, пустые строки, `\r\n`, ключи без учёта регистра, значение может содержать `=`), ошибки формата — `ConfigParseException` с номером строки. Методы чтения: `GetString`, `GetInt` (ошибка разбора числа **оборачивается** в `ConfigParseException` с `InnerException`), `TryGetInt` (без исключений).

**Зачем:** реалистичная обработка ошибок: доменное исключение с контекстом (номер строки), обёртывание низкоуровневых исключений, фильтр `when`, Try-pattern рядом с методом, бросающим исключение (как `int.Parse`/`int.TryParse`).
**Файл:** `Module07.Tasks/AppConfig.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~AppConfigTests"`
**Подумай / ответь письменно:**
1. Зачем оборачивать `FormatException` в `ConfigParseException`, а не пропустить его наружу? Почему важно передать `ex` как `innerException`?
2. Почему `catch (Exception ex) when (ex is FormatException or OverflowException)` лучше, чем `catch (Exception ex)`?

## Задача 3. Исследование: `throw;` vs `throw ex;`, порядок `finally`  ·  ⏱ ~25 мин  ·  🧪 исследование

**Что сделать:** без тестов, во временном консольном проекте (`dotnet new console -n ThrowDemo` вне папки курса):
1. Напиши цепочку `Level1()` → `Level2()` → `Level3()`, где `Level3` бросает `InvalidOperationException`. В `Level1` поймай исключение и перебрось тремя способами по очереди: `throw;`, `throw ex;`, `throw new ApplicationException("wrapped", ex);`. В `Main` выведи `ex.ToString()`. Сравни stack trace — в каком случае `Level3` исчез из трассы?
2. Предскажи, затем проверь вывод:
   ```csharp
   static int F()
   {
       try { Console.WriteLine("try"); throw new Exception("x"); }
       catch { Console.WriteLine("catch"); return 1; }
       finally { Console.WriteLine("finally"); }
   }
   Console.WriteLine(F());
   ```
3. Что будет, если в `finally` тоже бросить исключение? Какое исключение увидит вызывающий код?
4. Запусти в Release (`dotnet run -c Release`) — изменилась ли трасса? (Подсказка: inlining. Отметь, почему stack trace в Release может быть «короче».)

**Зачем:** «`throw` vs `throw ex`» и «что выведет код с finally» — обязательные вопросы на собеседовании. Лучше один раз увидеть самому.
**Подумай / ответь письменно:** объясни своими словами, почему `throw ex;` портит stack trace и почему анализатор CA2200 считает это ошибкой.

## Задача 4. Ресурс с `Dispose`  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуй `DatabaseConnection : IDisposable` (фейковое соединение, сообщающее в `ConnectionTracker` об открытии/выполнении/закрытии): валидация до «открытия», идемпотентный `Dispose`, `ObjectDisposedException` после закрытия. Затем `ReportService.RunCommands` и `ReportService.Transfer` — соединения должны закрываться **даже при исключении**, а в `Transfer` — в обратном порядке открытия.

**Зачем:** `IDisposable`, `using` statement и declaration, гарантия освобождения через `try/finally`. В реальной работе это `SqlConnection`, `DbContext`, `HttpResponseMessage`, `Stream`.
**Файл:** `Module07.Tasks/DatabaseConnection.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~DatabaseConnectionTests"`
**Подумай / ответь письменно:**
1. Перепиши мысленно `using var connection = ...` в `RunCommands` через `try/finally`. Почему это одно и то же?
2. Почему в конструкторе валидация должна идти до `tracker.OnOpened(name)`?
3. Почему `Dispose` должен быть идемпотентным? Кто может вызвать его дважды?

## Задача 5. Retry-helper  ·  ⏱ ~35 мин  ·  🟡 средняя

**Что сделать:** реализуй `Retry.Execute<T>`: повторяет операцию до `maxAttempts` раз, но только при «временных» ошибках (предикат `isTransient`), вызывает `onRetry` перед каждым повтором и после последней неудачи пробрасывает **тот же самый** объект исключения с исходным stack trace.

**Зачем:** реальная задача (внешние API, БД); проверяет фильтры исключений и понимание `throw;`. Встречается на live coding. Тесты проверяют `Assert.Same` — подменить исключение нельзя.
**Файл:** `Module07.Tasks/Retry.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~RetryTests"`
**Подумай / ответь письменно:**
1. Почему повторять `ArgumentException` бессмысленно, а `TimeoutException` — имеет смысл?
2. Чего не хватает этому helper'у для продакшена? (задержка, экспоненциальный backoff, jitter, отмена через `CancellationToken`, async, логирование — см. Polly.)

## Задача 6. `Result<T>` вместо исключений  ·  ⏱ ~50 мин  ·  🔴 сложная  ·  ⭐ со звёздочкой

**Что сделать:** реализуй неизменяемый generic-тип `Result<T>` (успех со значением или неудача с ошибкой) с методами `Map`, `Bind`, `Match`, `GetValueOrDefault`, и `AgeParser.Parse`, который разбирает пользовательский ввод без единого исключения.

**Зачем:** популярный в .NET-командах подход для бизнес-ошибок (библиотеки FluentResults, ErrorOr, CSharpFunctionalExtensions). На собеседовании: «исключения или Result — когда что?». Плюс первая практика generics (подробно — модуль 08) и делегатов `Func` (модуль 10).
**Файл:** `Module07.Tasks/Result.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ResultTests"`
**Подумай / ответь письменно:**
1. Почему чтение `Value` у неудачного результата бросает исключение — разве мы не уходили от исключений?
2. Чем `Bind` отличается от `Map`? Что получится, если в тесте `Bind_ChainsOperationsThatCanFail` использовать `Map(Classify)`?
3. Для каких ошибок ты бы оставил исключения даже в проекте, где принят `Result<T>`?
