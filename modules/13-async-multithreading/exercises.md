# Модуль 13 — задачи

> Код пишется в `starter/Module13.Tasks/`. Тесты: `cd modules/13-async-multithreading/starter && dotnet test`.
>
> Тесты здесь детерминированные: они не меряют время, а проверяют **факты** — сколько операций стартовало до завершения первой, какой максимум одновременных вызовов наблюдался, отменён ли токен. `AsyncGuard` в тестах — только предохранитель от зависания (10 секунд), а не замер скорости.

## Задача 1. Дашборд: параллельная загрузка через WhenAll  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `DashboardLoader.LoadAllAsync(source, widgetIds, token)` — загрузить данные всех виджетов **одновременно** и вернуть результаты в исходном порядке. Тест с «воротами» проверяет, что все три `LoadAsync` стартовали до того, как хоть одна завершилась: последовательный `foreach` + `await` не пройдёт.

**Зачем:** «как ускорить метод, который делает три независимых запроса?» — частый вопрос и частая задача на code review. Плюс разница между «запустить» и «дождаться».
**Файл:** `Module13.Tasks/DashboardLoader.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~DashboardLoaderTests"`
**Подумай / ответь письменно:**
1. Почему без `.ToArray()` после `Select` задачи всё равно запустятся, но код станет хрупким? (Подсказка: deferred execution из модуля 11 — что будет, если перечислить `Select` дважды?)
2. Почему здесь не нужен `Task.Run`?

## Задача 2. Импорт заказов с отменой  ·  ⏱ ~20 мин  ·  🟢 базовая

**Что сделать:** реализуй `OrderImporter.ImportAsync(lines, importOne, token)`: строки обрабатываются по очереди, **перед каждой** проверяется токен, токен передаётся в `importOne`, возвращается количество импортированных.

**Зачем:** `CancellationToken` есть в сигнатуре почти каждого async-метода в ASP.NET Core и EF Core. Нужно понимать, что отмена кооперативная и где её проверять.
**Файл:** `Module13.Tasks/OrderImporter.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~OrderImporterTests"`
**Подумай / ответь письменно:**
1. Почему отмена в .NET кооперативная, а не «убить поток»?
2. В чём разница между `ThrowIfCancellationRequested()` и `if (token.IsCancellationRequested) return count;`? Когда что уместно?

## Задача 3. Потокобезопасный счётчик: lock vs Interlocked  ·  ⏱ ~30 мин  ·  🟢 базовая

**Что сделать:**
1. 🧪 Сначала эксперимент: в любом консольном приложении (или временном тесте) запусти `Parallel.For(0, 100_000, _ => counter.Increment())` для готового `UnsafeCounter` 5 раз и запиши результаты.
2. Реализуй `LockCounter` (включая `TryIncrementUpTo(limit)` — атомарное «проверить и увеличить») и `InterlockedCounter`.

**Зачем:** race condition, `lock`, `Interlocked`, check-then-act — классика вопросов по многопоточности.
**Файл:** `Module13.Tasks/Counters.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~CounterTests"`
**Подумай / ответь письменно:**
1. Почему `UnsafeCounter` даёт каждый раз разный результат? Опиши по шагам, как теряется инкремент.
2. Как реализовать `TryIncrementUpTo` без lock через `Interlocked.CompareExchange`? (Код не обязателен — опиши идею цикла.)

## Задача 4. Загрузчик файлов с ограничением параллелизма  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуй `ThrottledDownloader.DownloadAllAsync(urls, download, maxConcurrency, token)` — скачать всё, но не больше `maxConcurrency` вызовов одновременно. Используй `SemaphoreSlim` и `WaitAsync`. Результаты — в исходном порядке. При ошибке одной загрузки остальные не должны зависнуть.

**Зачем:** «не положить внешний API» — реальная задача в любом интеграционном сервисе; `SemaphoreSlim` — ответ на вопрос «как сделать lock для async-кода».
**Файл:** `Module13.Tasks/ThrottledDownloader.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ThrottledDownloaderTests"`
**Подумай / ответь письменно:**
1. Почему `lock` здесь не подойдёт?
2. Что случится, если `Release()` вызывать не в `finally`, и первая загрузка упадёт при `maxConcurrency = 1`?

## Задача 5. Timeout-обёртка  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуй `TimeoutHelper.WithTimeoutAsync(operation, timeout, token)`: результат, если операция успела; `TimeoutException` и **отменённый токен операции**, если нет (даже если операция игнорирует токен); `OperationCanceledException`, если отменил вызывающий.

**Зачем:** таймауты на вызовы внешних сервисов — обязательная практика; на собеседовании любят спрашивать разницу «перестать ждать» vs «остановить операцию».
**Файл:** `Module13.Tasks/TimeoutHelper.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~TimeoutHelperTests"`
**Подумай / ответь письменно:**
1. Почему одного `CancelAfter` недостаточно, если операция игнорирует токен?
2. Почему вызывающему важно различать `TimeoutException` и `OperationCanceledException`?

## Задача 6. Постраничное чтение через IAsyncEnumerable  ·  ⏱ ~25 мин  ·  🟡 средняя

**Что сделать:** реализуй `PagedReader.ReadAllAsync(loadPage, token)` как async-итератор (`async IAsyncEnumerable<string>` + `yield return`): страницы 0, 1, 2… до первой пустой, следующая страница грузится только по требованию, токен работает и параметром, и через `.WithCancellation()` (`[EnumeratorCancellation]`).

**Зачем:** стриминг данных из API/БД без загрузки всего в память; `await foreach` встречается в EF Core и ASP.NET Core.
**Файл:** `Module13.Tasks/PagedReader.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~PagedReaderTests"`
**Подумай / ответь письменно:**
1. Чем этот метод лучше `Task<List<string>> ReadAllAsync()`? А чем хуже?
2. Почему проверка `loadPage == null` в итераторе срабатывает не при вызове метода?

## Задача 7. ⭐ Конвейер обработки заказов на Channel&lt;T&gt;  ·  ⏱ ~60 мин  ·  🔴 сложная

**Что сделать:** реализуй `OrderPipeline.RunAsync(orderIds, process, consumerCount, capacity, token)`: один producer пишет id в **bounded** `Channel<int>`, `consumerCount` consumers читают (`ReadAllAsync`) и обрабатывают, результаты собираются в потокобезопасную коллекцию. Ошибка в `process` должна остановить весь конвейер (тест с `capacity = 1` и 100 заказами зависнет, если producer не остановить) и выйти наружу.

**Зачем:** producer/consumer — популярная live coding задача на middle-собеседованиях и хороший «плюс» для junior; проверяет `Channel`, отмену, обработку ошибок в параллельном коде.
**Файл:** `Module13.Tasks/OrderPipeline.cs` (нужен `using System.Threading.Channels;`)
**Тесты:** `dotnet test --filter "FullyQualifiedName~OrderPipelineTests"`
**Подумай / ответь письменно:**
1. Зачем bounded-канал, если unbounded проще? Что такое backpressure?
2. Почему без `Writer.Complete()` consumers никогда не завершатся?
