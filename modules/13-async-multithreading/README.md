# Модуль 13. Асинхронность и многопоточность

> **Зачем:** любой backend на .NET — это async-код: запросы к БД, HTTP-вызовы, очереди. Ошибки здесь самые дорогие: deadlock, который «иногда» вешает сервис, race condition, который портит данные раз в неделю, исчерпание thread pool под нагрузкой. На собеседовании Junior это обязательный блок: «Task vs Thread», «что делает await», «почему нельзя `.Result`», «зачем `ConfigureAwait(false)`», «что такое `async void`», «как защитить общие данные». **Время:** ~8–10 ч. **Нужно:** модули 07 (исключения), 09 (коллекции), 10 (делегаты, лямбды), 12 (память).

## Что ты будешь уметь

- Объяснять разницу между процессом, потоком, `Thread`, `ThreadPool` и `Task`.
- Писать корректный async/await-код и объяснять, что происходит при `await` (без «await создаёт поток»).
- Запускать операции параллельно (`Task.WhenAll`), отменять их (`CancellationToken`), ограничивать по времени и по числу одновременных.
- Находить и исправлять deadlock от `.Result`/`.Wait()`, объяснять `ConfigureAwait(false)` и вред `async void`.
- Защищать общие данные: `lock`, `Interlocked`, `SemaphoreSlim`, concurrent-коллекции; строить producer/consumer на `Channel<T>`.

---

## 1. Процесс, поток, ThreadPool

**Процесс** (process) — запущенная программа: своё адресное пространство памяти, свои ресурсы. Процессы изолированы друг от друга.

**Поток** (thread) — последовательность выполнения внутри процесса. Все потоки процесса делят общую память (managed heap). У каждого потока — свой стек. Операционная система переключает потоки на ядрах процессора (context switch).

```csharp
var thread = new Thread(() => Console.WriteLine($"Поток {Environment.CurrentManagedThreadId}"));
thread.Start();
thread.Join(); // ждём завершения
// вывод: Поток 12 (номер зависит от запуска)
```

Создавать `Thread` вручную дорого: ~1 МБ под стек, системные вызовы на создание и уничтожение. Поэтому в .NET есть **ThreadPool** (пул потоков) — набор заранее созданных рабочих потоков, которые берут задачи из очереди. Пул сам подбирает число потоков под нагрузку.

```csharp
ThreadPool.QueueUserWorkItem(_ => Console.WriteLine("Выполнено потоком пула"));
```

> **Почему так?** Создание потока стоит сотни микросекунд и мегабайт памяти, а типичная операция в сервере — миллисекунды. Переиспользование потоков — тот же приём, что и пул объектов из модуля 12. Минус пула: если занять его потоки долгими блокирующими операциями, новые задачи будут ждать — это **thread pool starvation** (голодание пула), одна из частых причин «сервис внезапно стал отвечать за 30 секунд».

`new Thread` сегодня нужен редко: долгоживущий фоновый поток со своим приоритетом, STA-поток для COM/UI. Всё остальное — `Task`.

---

## 2. `Task` и `Task<T>`

**Task** — объект, представляющий **операцию, которая завершится в будущем** (promise/future). `Task<T>` — операция с результатом типа `T`.

Важно: **Task — это не поток.** Task может:
- выполняться на потоке пула (`Task.Run`);
- вообще не занимать поток — например, ожидание ответа от сети: ОС сообщит о завершении через I/O completion, и только тогда понадобится поток, чтобы продолжить;
- быть уже завершённым (`Task.FromResult(42)`, `Task.CompletedTask`).

```csharp
Task<int> cpuTask = Task.Run(() => Enumerable.Range(1, 1000).Sum()); // работа на потоке пула
Task<string> ioTask = File.ReadAllTextAsync("data.txt");            // ожидание диска, поток не занят
Task<int> ready = Task.FromResult(42);                               // уже готово

Console.WriteLine(ready.IsCompleted); // вывод: True
```

Состояния задачи: `Running`/`WaitingForActivation` → `RanToCompletion` | `Faulted` (исключение) | `Canceled`.

> **Почему так?** Абстракция «операция в будущем» отделяет **что** мы ждём от **как** это выполняется. Код одинаково работает с вычислением на пуле и с сетевым запросом, а комбинаторы (`WhenAll`, `WhenAny`, `ContinueWith`) работают с любыми задачами.

> 💼 **На собеседовании (Task vs Thread):** «Thread — это ресурс ОС, поток выполнения. Task — объект-обещание результата, более высокоуровневая абстракция. Task может выполняться на потоке пула, а может не занимать поток вовсе — при асинхронном IO. Task поддерживает результат, исключения, отмену, композицию через WhenAll/WhenAny и async/await. Thread вручную создают редко».

---

## 3. async/await: что происходит на самом деле

```csharp
public async Task<int> GetOrderCountAsync(int customerId)
{
    Console.WriteLine("1. До await");
    var orders = await _repository.GetOrdersAsync(customerId); // ← здесь метод «приостанавливается»
    Console.WriteLine("3. После await");
    return orders.Count;
}
```

Что делает компилятор: превращает метод в **state machine** (конечный автомат) — структуру с полем «на каком шаге мы» и полями для локальных переменных. Код между `await` становится отдельными «кусками».

Что происходит в runtime:
1. Метод выполняется **синхронно** до первого `await`.
2. `await` спрашивает у задачи: «ты уже готова?» Если да — продолжаем сразу, без переключений.
3. Если нет — метод **регистрирует продолжение** (continuation: «когда закончишь — выполни остаток метода»), **возвращает вызывающему незавершённый Task** и освобождает поток.
4. Когда операция завершится, продолжение запустится — на потоке пула или в исходном **SynchronizationContext** (UI-поток), если он был.
5. `return orders.Count` завершает тот Task, который получил вызывающий.

```csharp
Console.WriteLine("0. Вызов");
var task = GetOrderCountAsync(7);
Console.WriteLine("2. Метод вернул Task, а мы идём дальше");
var count = await task;
// вывод: 0. Вызов → 1. До await → 2. Метод вернул Task... → 3. После await
// (если GetOrdersAsync действительно асинхронный и не завершился мгновенно)
```

Главное:
- **`async` не создаёт поток** и не делает код «фоновым». Он лишь разрешает `await` внутри метода.
- **`await` не блокирует поток** — он отпускает его, пока операция не завершится. В этом вся польза: веб-сервер с 50 потоками может обслуживать тысячи одновременных запросов, ожидающих БД.
- Исключение внутри async-метода сохраняется в Task и **выбрасывается при `await`**.

> **Почему так?** Синхронный код, который ждёт сеть, держит поток впустую. Раньше асинхронность писали через callbacks (APM, EAP) — «лапша» из колбэков. async/await даёт писать асинхронный код так же линейно, как синхронный, а всю работу с продолжениями берёт на себя компилятор.

> 💼 **На собеседовании:** «await не создаёт поток и не блокирует текущий. Компилятор превращает метод в state machine. Если задача ещё не завершена, await регистрирует продолжение, возвращает управление вызывающему, и поток свободен. Когда операция завершится, остаток метода выполнится — на пуле или в захваченном SynchronizationContext».

### `ValueTask<T>` — кратко

`ValueTask<T>` — struct-альтернатива `Task<T>` для методов, которые **часто завершаются синхронно** (например, значение уже в кэше): не нужна аллокация объекта Task. Ограничения: ожидать только **один раз**, не вызывать `.Result` до завершения. По умолчанию возвращай `Task`; `ValueTask` — оптимизация для горячих путей.

---

## 4. Параллельный запуск: `Task.WhenAll` и `Task.WhenAny`

Последовательно — время складывается:

```csharp
var user = await LoadUserAsync(id);       // 100 мс
var orders = await LoadOrdersAsync(id);   // 100 мс  → итого ~200 мс
```

Параллельно — сначала **запускаем**, потом **ждём**:

```csharp
var userTask = LoadUserAsync(id);         // запущена
var ordersTask = LoadOrdersAsync(id);     // запущена
await Task.WhenAll(userTask, ordersTask); // итого ~100 мс
var user = userTask.Result;               // здесь .Result безопасен: задача уже завершена
```

Для коллекции: `var results = await Task.WhenAll(ids.Select(id => LoadAsync(id)));` — массив результатов в **том же порядке**, что и задачи.

Исключения: если упало несколько задач, `await Task.WhenAll(...)` бросит **первое** исключение; все — в `whenAllTask.Exception` (`AggregateException`).

`Task.WhenAny` — дождаться **первой** завершившейся задачи (например, «кто быстрее из двух зеркал» или ручной таймаут).

> **Почему так?** IO-операции не нагружают процессор, поэтому их выгодно перекрывать во времени. WhenAll не создаёт потоков — он просто ждёт уже запущенные операции. Но параллельность по IO не бесплатна: 1000 одновременных запросов к одному API могут его «положить» — это решаем ограничением (раздел 8).

→ **Задача 1** (см. exercises.md)

---

## 5. CPU-bound vs IO-bound и `Task.Run`

| | CPU-bound | IO-bound |
|---|-----------|----------|
| Что | вычисления: сжатие, хэши, обработка изображений | ожидание: сеть, диск, БД |
| Нужен поток? | да, всё время | нет, только до/после ожидания |
| Инструмент | `Task.Run`, `Parallel.For/ForEach` | async/await с настоящими async-API (`ReadAsync`, `SendAsync`, `ToListAsync`) |

```csharp
// CPU-bound в UI-приложении: увести тяжёлое вычисление с UI-потока
var hash = await Task.Run(() => ComputeHash(bigFile));

// IO-bound: Task.Run НЕ нужен — это лишний поток, который просто будет ждать
var json = await httpClient.GetStringAsync(url);
```

Анти-паттерны:
- `await Task.Run(() => httpClient.GetStringAsync(url))` — бессмысленная обёртка.
- В ASP.NET Core `Task.Run` для «ускорения» запроса — только перекладывает работу с одного потока пула на другой.
- **Sync over async**: `GetStringAsync(url).Result` — блокирует поток (см. deadlock ниже).

`Parallel.For` / `Parallel.ForEach` — для CPU-bound обработки коллекций на всех ядрах. Для асинхронной обработки коллекции с ограничением параллелизма — `Parallel.ForEachAsync` (.NET 6+).

> **Почему так?** Async экономит потоки, когда есть чего ждать. Для вычислений ждать нечего — процессор занят, и нужен поток. Смешивание (Task.Run для IO или блокировка async-кода) тратит потоки пула и ведёт к starvation.

---

## 6. Отмена: `CancellationToken`

Отмена в .NET **кооперативная**: никто не прерывает поток насильно. Вызывающий создаёт `CancellationTokenSource`, передаёт `Token` вниз по цепочке, а код **сам** проверяет токен.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // отмена через 5 с
try
{
    await ImportAsync(lines, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Импорт отменён");
}

async Task ImportAsync(IEnumerable<string> lines, CancellationToken ct)
{
    foreach (var line in lines)
    {
        ct.ThrowIfCancellationRequested();        // проверка в безопасной точке
        await SaveAsync(line, ct);                // передаём дальше — в БД/HTTP
    }
}
```

Правила:
- Принимай `CancellationToken cancellationToken = default` последним параметром async-методов.
- Передавай токен во все вложенные async-вызовы.
- Отмена = `OperationCanceledException` (или наследник `TaskCanceledException`) — ловить `catch (OperationCanceledException)`.
- `CancellationTokenSource` — `IDisposable`.
- `CancellationTokenSource.CreateLinkedTokenSource(a, b)` — токен, который отменится при отмене любого из источников (например, «отмена пользователем ИЛИ таймаут»).

> **Почему так?** Насильственное прерывание потока (`Thread.Abort`, которого больше нет в .NET Core) могло оборвать код посреди изменения данных — оставить заблокированный lock, полузаписанный файл. Кооперативная отмена позволяет коду остановиться там, где это безопасно, и корректно освободить ресурсы.

### Таймаут

```csharp
var result = await LoadAsync(ct).WaitAsync(TimeSpan.FromSeconds(2)); // .NET 6+: TimeoutException
```

`WaitAsync` перестаёт **ждать**, но не останавливает саму операцию — поэтому её токен тоже нужно отменить.

→ **Задача 2**, **Задача 5** (см. exercises.md)

---

## 7. Deadlock, `ConfigureAwait(false)`, `async void`

### Deadlock от `.Result` / `.Wait()`

**SynchronizationContext** — механизм «вернуться в нужный поток после await». Он есть в WinForms/WPF/MAUI (UI-поток) и в классическом ASP.NET (.NET Framework). По умолчанию `await` захватывает контекст и выполняет продолжение в нём.

```csharp
// WinForms, обработчик кнопки (UI-поток)
private void Button_Click(object sender, EventArgs e)
{
    var data = LoadAsync().Result;   // ❌ DEADLOCK
}

private async Task<string> LoadAsync()
{
    await Task.Delay(100);          // продолжение хочет вернуться в UI-поток...
    return "data";                  // ...но UI-поток заблокирован в .Result и ждёт этот же Task
}
```

UI-поток ждёт задачу, задача ждёт UI-поток — взаимная блокировка навсегда.

Решения:
1. **Async all the way** — сделать вызывающий код async: `private async void Button_Click(...) { var data = await LoadAsync(); }` (обработчик события — единственное законное место для `async void`).
2. В библиотечном коде — `await ... .ConfigureAwait(false)`: продолжение не возвращается в контекст.

**ASP.NET Core не имеет SynchronizationContext** — классического deadlock там нет, но `.Result` всё равно блокирует поток пула → под нагрузкой starvation.

### `ConfigureAwait(false)`

```csharp
public async Task<Report> BuildReportAsync()
{
    var data = await _client.GetDataAsync().ConfigureAwait(false); // продолжим на любом потоке пула
    return Build(data);
}
```

Где важно: в **библиотеках** (NuGet-пакеты), которые могут вызвать из UI-приложений. В коде приложения ASP.NET Core практического эффекта нет (контекста нет). В UI-коде — **нельзя**, если после `await` трогаешь элементы интерфейса.

> **Почему так?** Возврат в контекст стоит переключения и делает код уязвимым к deadlock, если кто-то выше по стеку заблокирован. Библиотека не знает, где её вызовут, поэтому безопаснее «не возвращаться». Приложение знает свою среду и решает само.

### `async void` — почему плохо

```csharp
async void SaveAsync() { await Task.Delay(10); throw new InvalidOperationException(); }

try { SaveAsync(); }        // исключение НЕ поймается здесь
catch (Exception) { }       // процесс может упасть
```

- Вызывающий **не может дождаться** завершения (нет Task).
- Исключение не попадает в Task — оно выбрасывается в SynchronizationContext/пул → обычно **падение процесса**.
- Нельзя протестировать нормально.

Допустимо только для **обработчиков событий** (сигнатура `void` навязана). Везде остальное — `async Task`.

> 💼 **На собеседовании:** «.Result/.Wait() в коде с SynchronizationContext (UI, старый ASP.NET) вызывают deadlock: поток заблокирован ожиданием, а продолжение хочет выполниться в этом же потоке. Решение — async all the way или ConfigureAwait(false) в библиотеках. async void — только для обработчиков событий: исключения из него нельзя поймать, и его нельзя дождаться».

---

## 8. Общие данные: race condition, `lock`, `Interlocked`, `SemaphoreSlim`

**Race condition** (состояние гонки) — результат зависит от того, в каком порядке потоки выполнили операции.

```csharp
var counter = 0;
Parallel.For(0, 100_000, _ => counter++);
Console.WriteLine(counter); // вывод: например 63817 — каждый раз разное и меньше 100000
```

`counter++` — три операции: прочитать, прибавить, записать. Два потока читают одно значение → одно увеличение теряется.

### `lock`

```csharp
private readonly Lock _sync = new(); // .NET 9+; раньше — private readonly object _sync = new();
private decimal _balance;

public bool TryWithdraw(decimal amount)
{
    lock (_sync)                         // в каждый момент внутри — только один поток
    {
        if (_balance < amount) return false; // проверка и изменение — атомарно вместе
        _balance -= amount;
        return true;
    }
}
```

`lock` — синтаксис над `Monitor.Enter/Exit` в `try/finally`. Правила:
- замок — **приватный** `readonly` объект (не `this`, не `typeof(...)`, не строка);
- внутри — минимум работы, **никаких `await`** (компилятор запретит) и вызовов чужого кода;
- если несколько замков — всегда брать их в одном порядке, иначе deadlock.

### `Interlocked`

Атомарные операции над одной переменной без блокировки: `Interlocked.Increment(ref x)`, `Add`, `Exchange`, `CompareExchange`. Быстрее lock, но только для простых случаев.

### `SemaphoreSlim`

Семафор — «турникет на N мест». В отличие от lock, **ждать можно асинхронно**, а значит — использовать вокруг `await`:

```csharp
private readonly SemaphoreSlim _gate = new(3); // не больше 3 одновременно

public async Task<string> CallApiAsync(string url, CancellationToken ct)
{
    await _gate.WaitAsync(ct);
    try { return await _http.GetStringAsync(url, ct); }
    finally { _gate.Release(); }
}
```

`new SemaphoreSlim(1, 1)` — «async lock»: взаимное исключение для кода с `await` внутри.

> **Почему так?** `lock` привязан к потоку (Monitor запоминает владельца), а после `await` код может продолжиться на другом потоке — поэтому `await` внутри `lock` запрещён. `SemaphoreSlim` не знает о потоках, он просто считает места — поэтому подходит для async.

→ **Задача 3**, **Задача 4** (см. exercises.md)

---

## 9. Concurrent-коллекции и `Parallel`

Обычные `List<T>` и `Dictionary<K,V>` **не потокобезопасны** для записи из нескольких потоков — можно получить исключение, потерю данных или повреждённую структуру.

| Коллекция | Когда |
|-----------|-------|
| `ConcurrentDictionary<K,V>` | общий кэш/словарь: `GetOrAdd`, `AddOrUpdate`, `TryRemove` |
| `ConcurrentQueue<T>` / `ConcurrentStack<T>` | FIFO/LIFO из нескольких потоков |
| `ConcurrentBag<T>` | неупорядоченный набор, когда порядок не важен |
| `Channel<T>` | асинхронная очередь producer/consumer |
| `ImmutableList<T>` и др. | «копия при записи», безопасное чтение |

```csharp
var cache = new ConcurrentDictionary<int, string>();
var name = cache.GetOrAdd(42, id => LoadName(id));
// ⚠ фабрика может выполниться НЕСКОЛЬКО раз при гонке (в словарь попадёт одно значение)
```

> **Почему так?** Делать все коллекции потокобезопасными — значит заставить всех платить за блокировки, даже в однопоточном коде. Поэтому в .NET потокобезопасность — отдельные типы, и выбирать их нужно осознанно. Кроме того, «потокобезопасная коллекция» не делает потокобезопасной **последовательность** операций: `if (!dict.ContainsKey(k)) dict[k] = v;` — гонка даже с `ConcurrentDictionary`; используй атомарные `GetOrAdd`/`TryAdd`.

---

## 10. `IAsyncEnumerable<T>` и `Channel<T>`

### Асинхронные потоки данных

`IAsyncEnumerable<T>` (C# 8) — как `IEnumerable<T>`, но каждый следующий элемент можно получать через `await`. Пишется через `async` + `yield return`, читается через `await foreach`.

```csharp
public async IAsyncEnumerable<Order> ReadOrdersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    for (var page = 0; ; page++)
    {
        var batch = await _api.GetPageAsync(page, ct);
        if (batch.Count == 0) yield break;
        foreach (var order in batch) yield return order;
    }
}

await foreach (var order in ReadOrdersAsync().WithCancellation(ct))
    Console.WriteLine(order.Id);
```

`[EnumeratorCancellation]` позволяет передать токен и параметром, и через `.WithCancellation(...)`. EF Core умеет `AsAsyncEnumerable()`, ASP.NET Core — стримить `IAsyncEnumerable` в ответ.

→ **Задача 6** (см. exercises.md)

### Producer/consumer: `Channel<T>`

`System.Threading.Channels` (входит в .NET) — асинхронная очередь: producers пишут, consumers читают, ожидание без блокировки потоков.

```csharp
var channel = Channel.CreateBounded<int>(capacity: 100); // при заполнении writer ждёт

var producer = Task.Run(async () =>
{
    for (var i = 0; i < 1000; i++) await channel.Writer.WriteAsync(i);
    channel.Writer.Complete();                   // «больше данных не будет»
});

var consumer = Task.Run(async () =>
{
    await foreach (var item in channel.Reader.ReadAllAsync())
        Console.WriteLine(item);
});

await Task.WhenAll(producer, consumer);
```

> **Почему так?** Bounded-канал даёт **backpressure** (обратное давление): быстрый producer не завалит память, а подождёт медленных consumers. Это ровно то, что делают очереди сообщений (RabbitMQ, Kafka), но внутри одного процесса. Раньше то же самое делали на `BlockingCollection<T>`, но она блокирует потоки.

→ **Задача 7 ⭐** (см. exercises.md)

---

## Типичные ошибки

1. **«async создаёт новый поток»** / «await запускает задачу в фоне». Нет: метод выполняется синхронно до первого незавершённого await.
2. **`.Result` / `.Wait()` / `.GetAwaiter().GetResult()` на незавершённой задаче** — deadlock в UI/старом ASP.NET, starvation в ASP.NET Core.
3. **`async void`** вне обработчиков событий.
4. **Последовательный `await` в цикле**, когда операции независимы, — вместо `WhenAll`.
5. **`Task.Run` для IO-bound** операций или «чтобы было async».
6. **Забытый `await`** — задача запущена, исключение потеряно, код идёт дальше (warning CS4014 — не игнорируй).
7. **Не передан `CancellationToken`** во вложенные вызовы — отмена «не работает».
8. **Check-then-act без lock**: `if (_count < max) _count++;` из нескольких потоков.
9. **`lock (this)`**, `lock` на строке или на `Type`; `await` внутри `lock` (не скомпилируется — и правильно).
10. **`List<T>` из нескольких потоков** вместо concurrent-коллекции или lock.
11. **`SemaphoreSlim.Release()` не в `finally`** — после исключения слоты «утекают».
12. **Unbounded очередь** между быстрым producer и медленным consumer — память растёт.

## Шпаргалка

```csharp
// async-метод
public async Task<T> DoAsync(CancellationToken ct = default) { var x = await OtherAsync(ct); return x; }

// параллельно
var results = await Task.WhenAll(items.Select(i => LoadAsync(i, ct)));
var first = await Task.WhenAny(t1, t2);

// CPU-bound
var r = await Task.Run(() => Compute(), ct);
Parallel.For(0, n, i => Work(i));

// отмена и таймаут
using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
cts.CancelAfter(TimeSpan.FromSeconds(5));
ct.ThrowIfCancellationRequested();
await task.WaitAsync(TimeSpan.FromSeconds(2), ct);   // TimeoutException

// библиотека
await SomethingAsync().ConfigureAwait(false);

// синхронизация
lock (_sync) { /* коротко, без await */ }
Interlocked.Increment(ref _count);
await _semaphore.WaitAsync(ct); try { ... } finally { _semaphore.Release(); }

// коллекции
var dict = new ConcurrentDictionary<K, V>(); dict.GetOrAdd(key, k => Create(k));

// потоки данных
async IAsyncEnumerable<T> Stream([EnumeratorCancellation] CancellationToken ct = default) { yield return ...; }
await foreach (var x in Stream().WithCancellation(ct)) { }
var ch = Channel.CreateBounded<T>(100); await ch.Writer.WriteAsync(x, ct); ch.Writer.Complete();
await foreach (var x in ch.Reader.ReadAllAsync(ct)) { }
```

## Глоссарий

| Термин | Что значит |
|--------|-----------|
| Process | запущенная программа с изолированной памятью |
| Thread | поток выполнения внутри процесса; свой стек, общая куча |
| ThreadPool | пул переиспользуемых рабочих потоков |
| Thread pool starvation | все потоки пула заняты/заблокированы, новые задачи ждут |
| Task / Task&lt;T&gt; | объект-обещание результата операции в будущем |
| async / await | синтаксис асинхронного кода; компилятор строит state machine |
| State machine | сгенерированный автомат, хранящий шаг и локальные переменные async-метода |
| Continuation | продолжение — код, который выполнится после завершения задачи |
| SynchronizationContext | механизм возврата продолжения в нужный поток (UI) |
| ConfigureAwait(false) | не возвращаться в захваченный контекст после await |
| Deadlock | взаимная блокировка: каждый ждёт другого |
| CPU-bound / IO-bound | ограничено процессором / ожиданием внешнего устройства |
| CancellationToken | токен кооперативной отмены |
| Race condition | результат зависит от порядка выполнения потоков |
| Atomic operation | операция, которая выполняется целиком, без промежуточных состояний |
| lock / Monitor | взаимное исключение: в критической секции один поток |
| Interlocked | атомарные операции над переменной |
| SemaphoreSlim | ограничение числа одновременных входов; поддерживает WaitAsync |
| ValueTask | struct-вариант Task для часто синхронных результатов |
| IAsyncEnumerable | асинхронная последовательность, `await foreach` |
| Channel&lt;T&gt; | асинхронная очередь producer/consumer |
| Backpressure | замедление producer, когда consumer не успевает |

## Вопросы на собеседовании

<details><summary><b>Чем Task отличается от Thread?</b></summary>

Thread — поток ОС, дорогой ресурс со своим стеком. Task — объект, представляющий будущий результат операции. Task может выполняться на потоке пула (`Task.Run`), а может вообще не занимать поток — при асинхронном IO. Task поддерживает результат, исключения, отмену, композицию (`WhenAll`, `WhenAny`) и `await`. В современном коде почти всегда используют Task.
</details>

<details><summary><b>Что происходит, когда выполнение доходит до await?</b></summary>

Компилятор превратил метод в state machine. На `await` проверяется, завершена ли задача: если да — выполнение продолжается синхронно. Если нет — регистрируется продолжение, вызывающему возвращается незавершённый Task, и поток освобождается. Когда задача завершится, остаток метода выполнится — в захваченном SynchronizationContext, если он есть, иначе на потоке пула. Новый поток await не создаёт.
</details>

<details><summary><b>Почему .Result или .Wait() может привести к deadlock?</b></summary>

В среде с SynchronizationContext (UI, классический ASP.NET) await по умолчанию возвращает продолжение в исходный поток. Если этот поток заблокирован в `.Result`, продолжение не может выполниться, а `.Result` ждёт завершения задачи — взаимная блокировка. Решение — async all the way, либо `ConfigureAwait(false)` во внутреннем коде. В ASP.NET Core контекста нет, но блокировка всё равно тратит потоки пула.
</details>

<details><summary><b>Зачем нужен ConfigureAwait(false)?</b></summary>

Говорит await не возвращаться в захваченный SynchronizationContext, а продолжить на любом потоке пула. Это немного быстрее и защищает от deadlock, если кто-то выше блокирует поток. Рекомендуется в библиотеках. В UI-коде нельзя, если после await трогаешь интерфейс. В ASP.NET Core практического эффекта нет — там нет контекста.
</details>

<details><summary><b>Почему async void — плохо?</b></summary>

Вызывающий не получает Task — не может дождаться завершения и узнать об ошибке. Исключение из async void выбрасывается напрямую в SynchronizationContext или пул и обычно роняет процесс; его нельзя поймать try/catch вокруг вызова. Такой код сложно тестировать. Единственное оправданное применение — обработчики событий.
</details>

<details><summary><b>Как запустить несколько асинхронных операций параллельно?</b></summary>

Сначала запустить все (получить Task'и), потом ждать: `await Task.WhenAll(tasks)`. Результаты возвращаются в порядке задач. При ошибках await бросит первое исключение, все остальные — в `Task.Exception`. Если нужно ограничить число одновременных операций — `SemaphoreSlim` или `Parallel.ForEachAsync` с `MaxDegreeOfParallelism`.

```csharp
var results = await Task.WhenAll(ids.Select(id => LoadAsync(id, ct)));
```
</details>

<details><summary><b>Чем CPU-bound отличается от IO-bound? Когда нужен Task.Run?</b></summary>

CPU-bound — вычисления, нужен поток всё время. IO-bound — ожидание сети/диска/БД, поток нужен только до и после. Для IO используют async-API без Task.Run. Task.Run — для CPU-bound работы, которую нужно увести с текущего потока (например, с UI-потока). В ASP.NET Core Task.Run для ускорения запроса бесполезен — это тот же пул.
</details>

<details><summary><b>Как работает отмена в .NET?</b></summary>

Кооперативно: `CancellationTokenSource` создаёт токен, его передают в методы, а код сам проверяет `IsCancellationRequested` или вызывает `ThrowIfCancellationRequested()` и передаёт токен во вложенные вызовы. При отмене бросается `OperationCanceledException`. Связанные токены (`CreateLinkedTokenSource`) объединяют несколько причин отмены, `CancelAfter` — таймаут.
</details>

<details><summary><b>Что такое race condition и как от неё защититься?</b></summary>

Ситуация, когда результат зависит от порядка выполнения потоков, например `count++` из нескольких потоков теряет инкременты. Защита: `lock` для критических секций (особенно check-then-act), `Interlocked` для простых атомарных операций над одной переменной, concurrent-коллекции, иммутабельные данные или отсутствие общего состояния.
</details>

<details><summary><b>Почему нельзя использовать await внутри lock? Что использовать вместо?</b></summary>

lock (Monitor) привязан к потоку-владельцу, а после await продолжение может выполниться на другом потоке, который не владеет замком. Кроме того, держать замок на время IO — плохая идея. Компилятор запрещает await в lock. Вместо него — `SemaphoreSlim(1, 1)` с `await WaitAsync()` и `Release()` в finally.
</details>

<details><summary><b>Чем lock отличается от Interlocked?</b></summary>

lock даёт взаимное исключение для блока кода любой сложности, но стоит дороже и может приводить к ожиданию. Interlocked — атомарные инструкции процессора для одной переменной (Increment, Add, Exchange, CompareExchange), без блокировок. Для счётчика — Interlocked, для нескольких связанных изменений или проверки + изменения — lock.
</details>

<details><summary><b>Что выведет код?</b></summary>

```csharp
static async Task Main()
{
    Console.Write("A");
    var t = WorkAsync();
    Console.Write("C");
    await t;
    Console.Write("E");
}

static async Task WorkAsync()
{
    Console.Write("B");
    await Task.Delay(100);
    Console.Write("D");
}
```

`ABCDE`. WorkAsync выполняется синхронно до первого await (печатает B), на `Task.Delay` возвращает незавершённый Task, Main печатает C, затем ждёт: через 100 мс печатается D, задача завершается, печатается E.
</details>

<details><summary><b>Что такое ValueTask и когда его использовать?</b></summary>

struct-альтернатива Task&lt;T&gt;, которая не требует аллокации, если результат готов синхронно (например, из кэша). Ограничения: можно await только один раз, нельзя ждать конкурентно, нельзя `.Result` до завершения. По умолчанию — Task; ValueTask — оптимизация горячих путей, подтверждённая замерами.
</details>

<details><summary><b>Какие потокобезопасные коллекции вы знаете?</b></summary>

`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentStack`, `ConcurrentBag`, `BlockingCollection`, а для async producer/consumer — `Channel<T>`. Важно: потокобезопасна каждая операция, но не их последовательность — для «проверить и добавить» нужны атомарные методы (`GetOrAdd`, `TryAdd`, `AddOrUpdate`). У `GetOrAdd` фабрика может вызваться несколько раз.
</details>

## Дополнительно

- [Asynchronous programming with async and await](https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/)
- [Task-based asynchronous pattern (TAP)](https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Cancellation in managed threads](https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads)
- [The lock statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock)
- [System.Threading.Channels](https://learn.microsoft.com/dotnet/core/extensions/channels)
- [Thread-safe collections](https://learn.microsoft.com/dotnet/standard/collections/thread-safe/)
- Stephen Cleary, «Concurrency in C# Cookbook» и статья «Don't Block on Async Code» (blog.stephencleary.com).
- Stephen Toub, «ConfigureAwait FAQ» (devblogs.microsoft.com/dotnet).
- Джон Скит, «C# in Depth» — главы об async/await.
- Джеффри Рихтер, «CLR via C#» — часть о потоках.
