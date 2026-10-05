# Модуль 07. Исключения и IDisposable

> **Зачем:** обработка ошибок и освобождение ресурсов — то, что отличает «работает у меня» от production-кода. Незакрытое соединение с БД, проглоченное исключение, потерянный stack trace — типичные причины ночных инцидентов. На собеседовании: «`throw` vs `throw ex`», «выполнится ли `finally`», «что такое `using`», «когда создавать свои исключения», «исключения или коды ошибок». **Время:** ~6–7 ч. **Нужно:** модули 04–06.

## Что ты будешь уметь

- Объяснять иерархию исключений и выбирать правильный тип.
- Писать `try`/`catch`/`finally`, фильтры `when`, правильно перебрасывать исключения.
- Создавать свои исключения и понимать, когда это нужно.
- Выбирать между исключением, Try-pattern и `Result<T>`.
- Писать guard clauses с `ThrowIfNull` и компанией.
- Реализовывать `IDisposable` и пользоваться `using` (statement и declaration).

---

## 1. Что такое исключение и иерархия

**Исключение** (exception) — объект, описывающий ошибку, которая прерывает нормальное выполнение. Оно «летит» вверх по стеку вызовов (stack unwinding — раскрутка стека), пока не найдётся подходящий `catch`. Если не найдётся — процесс падает.

```
System.Object
└── System.Exception                    ← база всех исключений
    ├── SystemException                 ← исторически «от runtime» (разделение устарело)
    │   ├── ArgumentException
    │   │   ├── ArgumentNullException
    │   │   └── ArgumentOutOfRangeException
    │   ├── InvalidOperationException
    │   │   └── ObjectDisposedException
    │   ├── NullReferenceException     ← никогда не бросай сам
    │   ├── IndexOutOfRangeException
    │   ├── InvalidCastException
    │   ├── FormatException
    │   ├── NotSupportedException / NotImplementedException
    │   ├── KeyNotFoundException
    │   ├── ArithmeticException → DivideByZeroException, OverflowException
    │   └── IOException → FileNotFoundException …
    ├── OperationCanceledException      ← отмена (модуль 13)
    └── (твои исключения)
```

Что брать, когда бросаешь сам:

| Ситуация | Тип |
|---|---|
| аргумент null | `ArgumentNullException` |
| аргумент вне диапазона | `ArgumentOutOfRangeException` |
| аргумент невалиден по иной причине | `ArgumentException` |
| объект в неподходящем состоянии для операции | `InvalidOperationException` |
| объект уже освобождён | `ObjectDisposedException` |
| операция не поддерживается этим типом | `NotSupportedException` |
| ещё не написано | `NotImplementedException` (только временно!) |
| доменная ошибка, которую надо отличать | своё исключение |

У исключения есть: `Message`, `StackTrace` (где произошло), `InnerException` (исходная причина), `Data`, `HResult`, и `ToString()`, который печатает всё это вместе с цепочкой inner exceptions.

> **Почему так?** Почему исключения, а не коды возврата, как в C? Код возврата легко проигнорировать — и программа продолжит работать с мусором. Исключение невозможно «не заметить»: либо его обработали, либо программа остановилась. Кроме того, ошибка из глубины стека сама доходит до места, где её **можно** осмысленно обработать, — не нужно пробрасывать код ошибки через 10 уровней вызовов. Цена — производительность: создание исключения со stack trace стоит микросекунды, в тысячи раз дороже обычного `return`. Поэтому исключения — для **исключительных** ситуаций.

---

## 2. `try` / `catch` / `finally`

```csharp
try
{
    var n = int.Parse(input);              // может бросить FormatException / OverflowException
    Console.WriteLine(100 / n);            // может бросить DivideByZeroException
}
catch (FormatException ex)                 // от частного...
{
    Console.WriteLine($"Не число: {ex.Message}");
}
catch (ArithmeticException)                // ...к общему (DivideByZero и Overflow — наследники)
{
    Console.WriteLine("Арифметическая ошибка");
}
finally
{
    Console.WriteLine("finally выполняется всегда");
}
```

- `catch` проверяются **сверху вниз**, выполняется первый подходящий. Если поставить общий тип выше частного — компилятор выдаст ошибку CS0160 (частный никогда не сработает).
- `finally` выполняется **всегда**: при нормальном завершении, при исключении (пойманном или нет), при `return`, `break`, `continue` внутри `try`.
- Исключения, когда `finally` **не** выполнится: `Environment.FailFast`, `StackOverflowException` (процесс убивается), убийство процесса, бесконечный цикл в `try`.

```csharp
static int Demo()
{
    try { return 1; }
    finally { Console.WriteLine("finally"); }   // выполнится ДО фактического выхода из метода
}
Console.WriteLine(Demo());
// вывод:
// finally
// 1
```

> **Почему так?** `finally` — гарантия освобождения ресурсов и восстановления состояния (снять блокировку, закрыть файл), независимо от того, как закончился блок. Писать очистку в каждом `catch` и после `try` — дублирование и гарантированные пропуски. Кстати, `using` и `lock` — это синтаксический сахар над `try/finally`.

### Фильтры исключений `when`

```csharp
try { await client.GetAsync(url); }
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    return null;                       // 404 — ожидаемо
}
// остальные HttpRequestException пролетят дальше НЕтронутыми
```

> **Почему так?** Можно было бы поймать всё и сделать `if (...) ... else throw;`. Но фильтр выполняется **до** раскрутки стека: если условие false, исключение как будто и не ловили — stack trace и состояние отладчика сохраняются в месте ошибки, а не в твоём `catch`. Фильтр ещё и читается лучше. Трюк для логирования: `catch (Exception ex) when (Log(ex))`, где `Log` возвращает `false`, — залогировали, но не поймали.

> 💼 **На собеседовании:** «catch проверяются по порядку, сначала частные типы, потом общие. finally выполняется всегда, кроме падения процесса — FailFast, StackOverflow. Фильтр `when` позволяет ловить исключение по условию без раскрутки стека».

---

## 3. `throw;` vs `throw ex;` — вопрос №1 по исключениям

```csharp
try
{
    ProcessOrder();                    // исключение возникло глубоко внутри
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed");
    throw;        // ✅ перебрасывает ТО ЖЕ исключение, stack trace сохранён (видно ProcessOrder → ... → место ошибки)
    // throw ex;  // ❌ stack trace перезаписывается: будто ошибка возникла в ЭТОЙ строке
}
```

Три варианта:
1. `throw;` — переброс с сохранением stack trace. Используй, когда нужно что-то сделать (лог, откат) и отпустить исключение дальше.
2. `throw ex;` — stack trace начинается с текущей строки. Информация о реальном месте ошибки **потеряна**. Практически всегда ошибка.
3. `throw new MyException("контекст", ex);` — **обёртывание**: новое исключение с доменным смыслом, исходное в `InnerException` (stack trace сохранён там).

Если исключение нужно перебросить **позже** (из другого места, потока), используют `ExceptionDispatchInfo.Capture(ex).Throw()` — так делает `await`.

> **Почему так?** Stack trace — главная информация при разборе инцидента: «где именно упало». `throw ex` трактует существующий объект исключения как новое бросание и перезаписывает трассу от текущей точки. Анализатор CA2200 подсвечивает это как ошибку. На собеседовании этот вопрос задают почти всегда.

→ **Задача 3** (исследование) в [exercises.md](exercises.md).

---

## 4. Свои исключения: когда бросать, когда ловить

```csharp
public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(decimal requested, decimal available)
        : base($"Requested {requested}, available {available}.")
    {
        Requested = requested;
        Available = available;
    }

    public decimal Requested { get; }
    public decimal Available { get; }
}
```

Своё исключение нужно, если вызывающему коду надо **отличить** эту ошибку от других (поймать именно её) или передать **дополнительные данные**. Соглашения: имя заканчивается на `Exception`, наследуйся от `Exception` (не от `ApplicationException`), предоставь конструктор с `innerException`. Если подходит стандартный тип — используй его.

**Когда бросать:**
- метод не может выполнить своё обещание (контракт): аргументы невалидны, объект в неверном состоянии, ресурс недоступен.

**Когда НЕ бросать:**
- для управления обычным потоком программы («не нашли пользователя» в поиске — это не исключение, верни `null` или `bool`);
- в getter'ах свойств (кроме проверки состояния), в `Equals`, `GetHashCode`, `ToString`, `Dispose`, финализаторах, static-конструкторах.

**Когда ловить:**
- если можешь **осмысленно обработать**: повторить, вернуть значение по умолчанию, показать пользователю сообщение;
- на **границе** системы: глобальный обработчик в Web API (модуль 15), `Main`, обработчик фонового задания — залогировать и не уронить процесс.

**Анти-паттерны:**

```csharp
try { DoWork(); } catch { }                         // «проглотить» всё — ошибка исчезла бесследно
try { DoWork(); } catch (Exception ex) { Console.WriteLine(ex.Message); }  // залогировал без stack trace и продолжил с битым состоянием
catch (Exception ex) { throw new Exception("Error"); }   // потеряли исходное исключение (нет inner)
catch (NullReferenceException) { ... }              // NRE — это баг, его исправляют, а не ловят
```

> **Почему так?** Исключение несёт информацию; поймать его, не зная, что с ним делать, значит потерять эту информацию и оставить программу в неизвестном состоянии. Правило: **ловишь — обрабатываешь, не можешь обработать — не лови** (или лови, добавь контекст и перебрось). Широкий `catch (Exception)` уместен только на верхнем уровне.

> 💼 **На собеседовании:** «Исключения — для ситуаций, когда метод не может выполнить контракт. Ловлю только то, что могу обработать, либо на границе приложения для логирования. Не глотаю исключения, перебрасываю через `throw;`, при обёртывании сохраняю inner. Свои исключения создаю, когда вызывающему нужно отличить ошибку или получить данные».

→ **Задача 1** (валидация) в [exercises.md](exercises.md).

---

## 5. Исключения vs Try-pattern vs Result

| Подход | Пример | Когда |
|---|---|---|
| Исключение | `int.Parse("x")` → `FormatException` | ошибка — действительно исключительная/баг; вызывающий не может продолжить |
| Try-pattern | `int.TryParse("x", out var n)` → `false` | неудача — ожидаемая и частая (ввод пользователя, кэш), нужно быстро |
| Result / null | `Result<User>.Failure("not found")` | неудача — часть бизнес-логики, нужен текст/тип ошибки, цепочки операций |

```csharp
// Try-pattern: соглашение — bool + out, никаких исключений при «обычной» неудаче
if (dictionary.TryGetValue(key, out var value)) { ... }

// Result: неудача видна в сигнатуре
Result<int> ParseAge(string? input) => ...;
var message = ParseAge(text).Match(age => $"Age {age}", error => error);
```

> **Почему так?** Исключение дорогое (сбор stack trace) и «невидимо» в сигнатуре метода: глядя на `int Parse(string)`, не скажешь, что он может упасть и как. Для **ожидаемых** ошибок (пользователь ввёл буквы, товара нет на складе) это плохо: и медленно в цикле, и заставляет писать try/catch для обычной логики. Try-pattern и `Result` делают неудачу явной и дешёвой. Но для **неожиданных** ошибок (баг, упала сеть, нарушен контракт) исключения лучше: их нельзя случайно проигнорировать. Многие команды комбинируют: Result для бизнес-ошибок, исключения — для всего остального.

→ **Задача 2** (парсер конфигурации) и **Задача 6 ⭐** (`Result<T>`) в [exercises.md](exercises.md).

---

## 6. Guard clauses

**Guard clause** (сторожевая проверка) — проверка аргументов в начале метода с немедленным выходом/исключением. Современный .NET предоставляет готовые throw-helpers:

```csharp
public void Register(string name, int age, Order order)
{
    ArgumentNullException.ThrowIfNull(order);                     // .NET 6
    ArgumentException.ThrowIfNullOrWhiteSpace(name);              // .NET 8 (null → ArgumentNullException)
    ArgumentOutOfRangeException.ThrowIfNegative(age);             // .NET 8
    ArgumentOutOfRangeException.ThrowIfGreaterThan(age, 150);     // .NET 8
    ObjectDisposedException.ThrowIf(_disposed, this);             // .NET 7
    // основная логика без вложенных if
}
```

Имя параметра подставляется автоматически благодаря атрибуту `[CallerArgumentExpression]` — в сообщении будет `"Value cannot be null. (Parameter 'order')"`.

> **Почему так?** Fail fast («падай быстро»): ошибка обнаруживается там, где произошла, с понятным сообщением, а не через 5 вызовов как `NullReferenceException` без контекста. Throw-helpers короче ручных `if (x is null) throw new ArgumentNullException(nameof(x));`, не ошибаются в `nameof` и лучше для JIT (бросание вынесено из «горячего» метода).

---

## 7. `IDisposable` и `using`

GC (garbage collector, сборщик мусора) освобождает **память**, но не знает про **неуправляемые и внешние ресурсы**: файловые дескрипторы, сокеты, соединения с БД, блокировки, таймеры. Их нужно освобождать явно и **как можно раньше** — для этого интерфейс `IDisposable` с единственным методом `Dispose()`.

```csharp
public sealed class TempFile : IDisposable
{
    private bool _disposed;
    public TempFile() => Path = System.IO.Path.GetTempFileName();
    public string Path { get; }

    public void Dispose()
    {
        if (_disposed) return;          // идемпотентность: второй вызов — без эффекта
        _disposed = true;
        File.Delete(Path);
    }
}
```

`using` гарантирует вызов `Dispose` — даже при исключении:

```csharp
using (var reader = new StreamReader("data.txt"))   // using statement: Dispose в конце блока
{
    Console.WriteLine(reader.ReadLine());
}

using var writer = new StreamWriter("log.txt");     // using declaration (C# 8): Dispose в конце области видимости
writer.WriteLine("hello");
```

Во что компилятор разворачивает `using`:

```csharp
var reader = new StreamReader("data.txt");
try
{
    Console.WriteLine(reader.ReadLine());
}
finally
{
    reader?.Dispose();
}
```

Несколько `using` освобождаются в **обратном** порядке объявления (последний созданный — первым закрыт), как вложенные `try/finally`.

Правила:
- Если класс **владеет** полем-`IDisposable` — сам реализует `IDisposable` и освобождает его.
- `Dispose` должен быть идемпотентным и не бросать исключений.
- После `Dispose` методы объекта бросают `ObjectDisposedException`.
- Не вызывай `Dispose` для объекта, которым не владеешь (например, полученного через DI — им управляет контейнер).
- `IAsyncDisposable` + `await using` — для асинхронного освобождения (`DbContext`, `Stream` с асинхронным сбросом буфера) — подробнее в модуле 13.
- Полный Dispose pattern с `Dispose(bool)` и финализатором (`~ClassName()`) нужен только при прямом владении **неуправляемым** ресурсом (handle) — это модуль 12; в современном коде вместо этого используют `SafeHandle`.

> **Почему так?** GC недетерминирован: объект может быть собран через секунды или минуты, а соединение с БД из пула нужно вернуть сразу, иначе пул закончится под нагрузкой. `IDisposable` — явный контракт «у меня есть ресурс, освободи меня», а `using` — способ сделать освобождение гарантированным и незаметным. Почему не деструктор, как в C++? В .NET нет детерминированного уничтожения объектов; финализатор вызывается GC когда-нибудь (или никогда до конца процесса) — это лишь страховка, не основной механизм.

> 💼 **На собеседовании:** «`IDisposable` — для детерминированного освобождения ресурсов, которые GC не контролирует: соединения, файлы, сокеты. `using` разворачивается в try/finally и гарантирует вызов Dispose даже при исключении. Dispose должен быть идемпотентным. Финализатор — только страховка для неуправляемых ресурсов».

→ **Задача 4** (соединение с БД) в [exercises.md](exercises.md).

---

## 8. Повтор при временных ошибках (retry)

Сеть, БД, внешние API иногда отвечают ошибкой, которая через мгновение исчезает (**transient fault**). Такие операции повторяют — но **только** временные ошибки и ограниченное число раз.

```csharp
for (int attempt = 1; ; attempt++)
{
    try { return client.GetData(); }
    catch (HttpRequestException) when (attempt < 3)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(100 * attempt));   // в async-коде — await Task.Delay
    }
}
```

> **Почему так?** Повторять ошибку валидации (`ArgumentException`) бессмысленно — она повторится. Повторять бесконечно — опасно (лавина запросов к и так лежащему сервису). Отсюда: фильтр по типу ошибки, лимит попыток, растущая задержка (exponential backoff). В реальных проектах используют библиотеку **Polly** / `Microsoft.Extensions.Resilience`, но понимать механику нужно — её дают на live coding.

→ **Задача 5** (Retry) в [exercises.md](exercises.md).

---

## Типичные ошибки

1. `throw ex;` вместо `throw;` — потерян stack trace.
2. Пустой `catch { }` или `catch (Exception)` посреди бизнес-логики.
3. Обёртывание без `innerException` — потеряна причина.
4. Исключения для обычного потока управления (поиск, валидация в цикле).
5. Общий `catch (Exception)` перед частными — частные не сработают (ошибка компиляции, но иногда это делают через `when`).
6. Бросают `Exception` или `ApplicationException` вместо конкретного типа.
7. Бросают `NullReferenceException`/`IndexOutOfRangeException` вручную — это «системные» типы для багов.
8. Забыли `using` для `IDisposable` (`new SqlConnection()`, `new StreamWriter()`, `HttpResponseMessage`).
9. Неидемпотентный `Dispose` / `Dispose`, бросающий исключения.
10. Захват ресурса до валидации аргументов в конструкторе → утечка при исключении.
11. `Dispose` объекта, которым не владеешь (из DI, переданного извне).
12. Retry на неvременных ошибках и без лимита.

## Шпаргалка

```csharp
// Guard clauses
ArgumentNullException.ThrowIfNull(x);
ArgumentException.ThrowIfNullOrWhiteSpace(s);
ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);
ObjectDisposedException.ThrowIf(_disposed, this);

// Обработка
try { ... }
catch (FormatException ex) when (ex.Message.Length > 0) { ... }  // частный тип + фильтр
catch (Exception ex) { log(ex); throw; }                         // перебросить С сохранением трассы
finally { cleanup(); }                                           // всегда

throw new DomainException("context", ex);                         // обернуть с inner

// Свое исключение
public sealed class OrderNotFoundException(int id)
    : Exception($"Order {id} not found") { public int OrderId { get; } = id; }

// Ресурсы
using var conn = new DbConnectionStub();                          // Dispose в конце области
public void Dispose() { if (_disposed) return; _disposed = true; /* free */ }

// Альтернативы исключениям
if (int.TryParse(s, out var n)) { ... }
Result<int> r = Parse(s); r.Match(ok => ..., err => ...);
```

## Глоссарий

| Термин | Что значит |
|---|---|
| exception | объект-ошибка, прерывающий нормальное выполнение |
| stack unwinding | раскрутка стека в поисках подходящего `catch` |
| stack trace | цепочка вызовов до места ошибки |
| `finally` | блок, выполняемый всегда при выходе из `try` |
| exception filter | условие `when` у `catch`, проверяемое до раскрутки стека |
| rethrow (`throw;`) | переброс того же исключения с сохранением трассы |
| inner exception | исходное исключение, обёрнутое в новое |
| guard clause | проверка аргументов в начале метода |
| fail fast | обнаружение ошибки как можно раньше |
| Try-pattern | метод `bool TryX(..., out T result)` без исключений |
| Result type | тип «успех со значением или ошибка» |
| `IDisposable` | контракт детерминированного освобождения ресурсов |
| `using` statement / declaration | сахар над try/finally с вызовом `Dispose` |
| idempotent | повторный вызов не меняет результат |
| `IAsyncDisposable` | асинхронный `DisposeAsync` + `await using` |
| unmanaged resource | ресурс ОС вне контроля GC (handle, сокет) |
| finalizer | метод `~T()`, вызываемый GC перед сборкой (страховка) |
| transient fault | временная ошибка, исчезающая при повторе |

## Вопросы на собеседовании

<details><summary><b>Чем <code>throw;</code> отличается от <code>throw ex;</code>?</b></summary>

`throw;` перебрасывает текущее исключение, сохраняя исходный stack trace — видно реальное место ошибки. `throw ex;` бросает тот же объект заново, и stack trace перезаписывается с текущей строки — информация о том, где произошла ошибка, теряется. Поэтому для переброса — только `throw;`, а если нужно добавить контекст — `throw new XxxException("контекст", ex)`, исходное исключение сохранится в `InnerException`.
</details>

<details><summary><b>Всегда ли выполняется <code>finally</code>?</b></summary>

Практически всегда: при нормальном выходе, при исключении (пойманном или нет), при `return`/`break`/`continue` из `try`. Не выполнится, если процесс завершается аварийно: `Environment.FailFast`, `StackOverflowException`, убийство процесса, отключение питания; и если `try` никогда не завершается (бесконечный цикл). Если в `finally` самом возникнет исключение, оно заменит исходное.
</details>

<details><summary><b>Что выведет код?</b>

```csharp
static int F()
{
    int x = 1;
    try { return x; }
    finally { x = 2; Console.Write("finally "); }
}
Console.WriteLine(F());
```
</summary>

`finally 1`. Значение для `return` вычисляется (копия `x = 1`) до выполнения `finally`; изменение `x` в `finally` уже не влияет на возвращаемое значение. `finally` выполняется до фактического выхода из метода, поэтому «finally» печатается первым.
</details>

<details><summary><b>Как работает <code>using</code>? Во что он компилируется?</b></summary>

`using` для объекта `IDisposable` разворачивается в `try { ... } finally { obj?.Dispose(); }`, поэтому `Dispose` вызывается гарантированно — и при нормальном выходе, и при исключении. `using var x = ...;` (C# 8) вызывает `Dispose` в конце области видимости. Несколько `using` освобождаются в обратном порядке. Для `IAsyncDisposable` — `await using`.
</details>

<details><summary><b>Зачем нужен <code>IDisposable</code>, если есть GC?</b></summary>

GC управляет только управляемой памятью и делает это недетерминированно — когда сочтёт нужным. Внешние ресурсы (соединения с БД, файлы, сокеты, блокировки) ограничены и должны освобождаться сразу после использования, иначе пул соединений исчерпается, файл останется заблокированным. `IDisposable.Dispose()` — явный и детерминированный способ освободить ресурс, `using` — гарантирует вызов. Финализатор — лишь страховка для неуправляемых ресурсов, если Dispose забыли.
</details>

<details><summary><b>Когда создавать собственные исключения?</b></summary>

Когда вызывающему коду нужно отличить эту ошибку от других (поймать именно её) или получить дополнительные данные (Id заказа, имя поля). Если подходит стандартный тип (`ArgumentException`, `InvalidOperationException`) — используют его. Соглашения: суффикс `Exception`, наследование от `Exception`, конструктор с `innerException`, неизменяемые свойства с данными.
</details>

<details><summary><b>Исключения или коды ошибок / Result? Когда что?</b></summary>

Исключения — для ситуаций, когда метод не может выполнить контракт и вызывающий не ожидает этого: баги, недоступность ресурсов, нарушение инвариантов. Их нельзя проигнорировать, но они дорогие и невидимы в сигнатуре. Try-pattern (`TryParse`, `TryGetValue`) — для ожидаемых частых неудач. Result — для бизнес-ошибок, которые являются частью логики (валидация, «недостаточно средств»), особенно в цепочках операций. Исключения не стоит использовать для управления обычным потоком.
</details>

<details><summary><b>В каком порядке проверяются блоки <code>catch</code>? Что такое фильтр <code>when</code>?</b></summary>

Сверху вниз, выполняется первый, тип которого совместим с исключением. Поэтому частные типы ставят выше общих — иначе компилятор выдаст ошибку о недостижимом catch. Фильтр `when (условие)` — дополнительное условие: если false, catch пропускается, как будто его нет. Фильтр выполняется до раскрутки стека, что сохраняет контекст для отладки; удобно для `catch (HttpRequestException ex) when (ex.StatusCode == 404)`.
</details>

<details><summary><b>Можно ли ловить <code>Exception</code>? А <code>NullReferenceException</code>?</b></summary>

`catch (Exception)` — только на верхнем уровне (глобальный обработчик, `Main`, фоновая задача) для логирования и предотвращения падения процесса, или с последующим `throw;`. В середине бизнес-логики — антипаттерн: ловит и баги, и то, что ты не умеешь обработать. `NullReferenceException`, `IndexOutOfRangeException` ловить нельзя — это признак бага, его исправляют проверками (guard clauses, nullable reference types).
</details>

<details><summary><b>Что должно быть в правильной реализации <code>Dispose</code>?</b></summary>

Идемпотентность (флаг `_disposed`, повторный вызов ничего не делает), отсутствие исключений, освобождение всех владеемых ресурсов (в том числе вызов `Dispose` у владеемых полей), после освобождения — `ObjectDisposedException` при использовании объекта. Для sealed-класса с управляемыми ресурсами достаточно простого `Dispose()`. Если класс может наследоваться или владеет неуправляемым ресурсом — полный Dispose pattern с `protected virtual Dispose(bool)` и `GC.SuppressFinalize(this)`.
</details>

<details><summary><b>Что произойдёт с исключением внутри <code>finally</code> или <code>Dispose</code>?</b></summary>

Новое исключение заменит исходное, если оно уже летело, — исходное будет потеряно. Поэтому `Dispose` и код в `finally` не должны бросать исключения: в худшем случае ты потеряешь настоящую причину проблемы и получишь в логах вторичную ошибку.
</details>

<details><summary><b>Что такое guard clause? Какие throw-helpers есть в .NET?</b></summary>

Проверка аргументов в начале метода с немедленным исключением — fail fast, понятная ошибка в месте вызова и плоский код без вложенных if. Throw-helpers: `ArgumentNullException.ThrowIfNull`, `ArgumentException.ThrowIfNullOrEmpty/OrWhiteSpace`, `ArgumentOutOfRangeException.ThrowIfNegative/Zero/GreaterThan/LessThan...`, `ObjectDisposedException.ThrowIf`. Имя параметра подставляется через `CallerArgumentExpression`.
</details>

## Дополнительно

- [Исключения: обзор (learn.microsoft.com)](https://learn.microsoft.com/dotnet/csharp/fundamentals/exceptions/)
- [Best practices for exceptions](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Exception filters (`when`)](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/exception-handling-statements)
- [Implement a Dispose method](https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose), [using statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using)
- [Transient fault handling / resilience](https://learn.microsoft.com/dotnet/core/resilience/)
- Книги: Рихтер, «CLR via C#» — гл. 20 (исключения и управление состоянием), гл. 21 (Dispose, финализация); Скит, «C# in Depth» — exception filters.
