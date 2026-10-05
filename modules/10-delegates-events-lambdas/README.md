# Модуль 10. Делегаты, события и лямбды

> **Зачем:** делегаты — это способ передать **поведение** как значение: «вот тебе функция, вызови её, когда нужно». На них построены LINQ (`Where(x => x > 0)`), события UI и доменной логики, callbacks, middleware в ASP.NET Core, DI-фабрики, `Task.Run(() => ...)`. На собеседовании спрашивают: «что такое делегат», «`Func` vs `Action`», «чем event отличается от делегата», «что такое замыкание и какая ловушка с циклом», «как события приводят к утечкам памяти». **Время:** ~5–6 ч. **Нужно:** модули 04–09 (классы, интерфейсы, generics, коллекции, `yield`).

## Что ты будешь уметь

- Объявлять и использовать делегаты, в том числе multicast.
- Выбирать между `Func`, `Action`, `Predicate` и своим типом делегата.
- Писать лямбды и использовать method groups.
- Объяснять closures (замыкания) и избегать ловушки с переменной цикла.
- Объявлять события по стандартному паттерну `EventHandler<TEventArgs>`, правильно вызывать (`?.Invoke`) и отписываться.
- Реализовывать observer pattern, event bus и конвейер middleware на делегатах.

---

## 1. Делегат — тип «ссылка на метод»

**Делегат** (delegate) — это тип, экземпляр которого хранит ссылку на метод (и на объект, если метод экземплярный). Объявление делегата описывает **сигнатуру**:

```csharp
public delegate int BinaryOperation(int a, int b);   // «любой метод (int, int) → int»

static int Add(int a, int b) => a + b;
static int Multiply(int a, int b) => a * b;

BinaryOperation op = Add;          // method group conversion
Console.WriteLine(op(2, 3));       // вывод: 5
op = Multiply;
Console.WriteLine(op.Invoke(2, 3)); // вывод: 6 — op(…) и op.Invoke(…) одно и то же
```

Под капотом `delegate` объявляет **класс**, наследник `System.MulticastDelegate`, с методом `Invoke`. Экземпляр делегата хранит `Target` (объект или `null` для static-метода) и `Method`.

Зачем это нужно? Чтобы метод мог принимать **поведение** параметром:

```csharp
static int Apply(int[] numbers, BinaryOperation op, int seed)
{
    int result = seed;
    foreach (int n in numbers) result = op(result, n);
    return result;
}

Console.WriteLine(Apply(new[] { 1, 2, 3, 4 }, Add, 0));       // вывод: 10
Console.WriteLine(Apply(new[] { 1, 2, 3, 4 }, Multiply, 1));  // вывод: 24
```

> **Почему так?** Алгоритм «пройти и свернуть» один, а операции разные. Делегат отделяет «как перебирать» от «что делать с элементом» — тот же принцип, что у интерфейсов, но легче: не нужно объявлять класс ради одного метода. В отличие от указателей на функции в C, делегат типобезопасен: компилятор проверяет сигнатуру.

> 💼 **На собеседовании:** «Делегат — типобезопасная ссылка на метод. Это класс, наследник `MulticastDelegate`, хранящий target и метод. Позволяет передавать поведение как параметр: callbacks, LINQ, события».

---

## 2. `Func`, `Action`, `Predicate`

Объявлять свой делегат на каждую сигнатуру неудобно, поэтому в BCL есть готовые generic-делегаты:

| Делегат | Сигнатура | Пример |
|---|---|---|
| `Action` | `() → void` | `Action save = () => Console.WriteLine("saved");` |
| `Action<T1, …, T16>` | `(T1…) → void` | `Action<string> log = Console.WriteLine;` |
| `Func<TResult>` | `() → TResult` | `Func<DateTime> now = () => DateTime.UtcNow;` |
| `Func<T1, …, T16, TResult>` | `(T1…) → TResult` (результат — **последний** параметр типа) | `Func<int, int, int> add = (a, b) => a + b;` |
| `Predicate<T>` | `T → bool` | `Predicate<int> isEven = n => n % 2 == 0;` |

```csharp
Func<string, int> length = s => s.Length;
Action<string> print = s => Console.WriteLine(s);
Predicate<int> isPositive = n => n > 0;

print(length("hello").ToString());          // вывод: 5
Console.WriteLine(isPositive(-1));           // вывод: False
```

**Ловушка:** `Predicate<T>` и `Func<T, bool>` — **разные** типы с одинаковой сигнатурой; присвоить один другому напрямую нельзя:

```csharp
Func<int, bool> f = n => n > 0;
// Predicate<int> p = f;          // CS0029 — делегаты с одинаковой сигнатурой несовместимы
Predicate<int> p = new Predicate<int>(f);   // можно обернуть
Predicate<int> p2 = n => f(n);              // или через лямбду
```

Когда объявлять **свой** делегат: когда имя добавляет смысла (`RequestHandler`, `EventHandler`), когда нужны `ref`/`out`-параметры (их `Func` не поддерживает) или когда делегат — часть публичного API библиотеки.

> **Почему так?** `Predicate` появился в .NET 2.0 (для `List<T>.FindAll`), а `Func`/`Action` — в .NET 3.5 вместе с LINQ, когда стало ясно, что набор универсальных делегатов покрывает 99% случаев. Делегаты в .NET номинальные (совместимость по имени типа, а не по сигнатуре) — так устроена система типов CLR, поэтому старый `Predicate` и новый `Func` не совместимы.

→ **Задача 1** (см. exercises.md) — свои `Filter`/`Map`/`Reduce`/`CountWhere`.

---

## 3. Лямбды и method groups

**Лямбда-выражение** (lambda) — анонимная функция, записанная прямо на месте:

```csharp
Func<int, int> square = x => x * x;                 // expression lambda
Func<int, int, int> max = (a, b) => a > b ? a : b;
Action<string> greet = name =>                       // statement lambda — с телом в {}
{
    var text = $"Hello, {name}!";
    Console.WriteLine(text);
};
Func<int> answer = () => 42;                         // без параметров
Func<int, int> ignoreArg = _ => 0;                   // _ — «параметр не нужен»
var parse = (string s) => int.Parse(s);              // C# 10: natural type — Func<string, int>
```

**Method group** — имя метода без вызова; компилятор сам создаёт делегат:

```csharp
var words = new List<string> { "b", "a" };
words.ForEach(Console.WriteLine);                  // вместо x => Console.WriteLine(x)
var numbers = new[] { "1", "2" }.Select(int.Parse); // Func<string, int> из method group
```

**Static lambda** (C# 9) — `static x => x * 2` — запрещает захват внешних переменных; полезно, чтобы случайно не создать замыкание (и лишнюю аллокацию).

> **Почему так?** До лямбд (C# 3) писали `delegate (int x) { return x * x; }` (анонимные методы C# 2) или отдельные именованные методы — шумно. Лямбды сделали LINQ удобным. Компилятор превращает лямбду в обычный метод (часто — в скрытом классе) и создаёт делегат на него.

---

## 4. Multicast-делегаты

Делегат может ссылаться на **несколько** методов — это invocation list (список вызова). Операторы `+` / `+=` добавляют, `-` / `-=` удаляют:

```csharp
Action<string> log = s => Console.WriteLine($"console: {s}");
log += s => Console.WriteLine($"file: {s}");
log("started");
// вывод:
// console: started
// file: started

Console.WriteLine(log.GetInvocationList().Length);  // вывод: 2
```

Важные свойства:
- Методы вызываются **по порядку** добавления, синхронно.
- Для делегата с возвращаемым значением результат — от **последнего** метода; остальные теряются.
- Если один из методов бросил исключение, **последующие не вызываются**.
- Делегаты **immutable**: `+=` создаёт новый объект-делегат. Поэтому если во время вызова кто-то отписался, текущий вызов идёт по старому списку.
- `-=` удаляет последнее вхождение **равного** делегата. Делегаты равны, если совпадают `Target` и `Method`. Две одинаковые по тексту лямбды — **разные** методы, поэтому `-= x => ...` ничего не удалит.

```csharp
Func<int> f = () => 1;
f += () => 2;
Console.WriteLine(f());   // вывод: 2 — только последний результат
```

> **Почему так?** Multicast создавался для событий: «оповестить всех подписчиков». Возвращаемое значение для событий не имеет смысла, поэтому события в .NET возвращают `void`. Immutable-делегаты делают подписку/отписку потокобезопасной на уровне ссылки: вызывающий работает со «снимком».

→ **Задача 6** (🧪 исследование) — multicast, исключения в цепочке, замыкание в for/foreach.

---

## 5. Closures — замыкания

Лямбда может использовать переменные из окружающего метода. Она **захватывает переменную, а не значение** — это и есть **closure** (замыкание):

```csharp
int counter = 0;
Action increment = () => counter++;
increment();
increment();
Console.WriteLine(counter);   // вывод: 2 — лямбда меняла ту же переменную
```

Как это устроено: компилятор переносит `counter` в поле скрытого класса (display class), а лямбду делает методом этого класса. Метод и лямбда работают с одним и тем же полем. Переменная «живёт» столько, сколько живёт делегат, даже после выхода из метода:

```csharp
static Func<int> CreateCounter()
{
    int count = 0;              // станет полем display class
    return () => ++count;
}

var next = CreateCounter();
Console.WriteLine(next());     // вывод: 1
Console.WriteLine(next());     // вывод: 2
```

### Классическая ловушка: переменная цикла `for`

```csharp
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.Write(i + " "));
foreach (var a in actions) a();
// вывод: 3 3 3
```

У `for` переменная `i` **одна** на весь цикл. Все три лямбды захватили её, а к моменту вызова `i == 3`. Исправление — копия внутри тела:

```csharp
for (int i = 0; i < 3; i++)
{
    int copy = i;                       // новая переменная на каждой итерации
    actions.Add(() => Console.Write(copy + " "));
}
// вывод: 0 1 2
```

У `foreach` с C# 5 переменная итерации **своя на каждой итерации**, поэтому `foreach (var x in items) actions.Add(() => Use(x));` работает правильно. До C# 5 и `foreach` имел эту ловушку — её «починили» breaking change-ем, потому что на ней ошибались все.

Цена замыканий: аллокация display class и делегата. В горячих местах это заметно — отсюда `static`-лямбды.

> **Почему так?** Захват переменной (а не значения) позволяет лямбде видеть актуальное состояние и менять его — без этого не работали бы счётчики, кэши, мемоизация. Плата — неочевидность: переменная может измениться до вызова. Правило: если лямбда вызывается позже (отложенно), задумайся, какое значение переменная будет иметь **в момент вызова**.

> 💼 **На собеседовании:** «Замыкание — лямбда вместе с захваченными внешними переменными. Захватывается переменная, а не значение: компилятор переносит её в поле сгенерированного класса. Ловушка — переменная цикла `for`: все лямбды видят последнее значение. Решение — локальная копия внутри цикла. У `foreach` с C# 5 этой проблемы нет».

→ **Задача 2** — калькулятор на `Dictionary<string, Func<double, double, double>>`.
→ **Задача 4** — `ClosureLab`: счётчик, мемоизация, ловушка цикла.

---

## 6. События (events)

**Событие** (event) — член класса, через который объект **оповещает** подписчиков. Это обёртка над полем-делегатом, ограничивающая доступ:

```csharp
public class Thermostat
{
    public event EventHandler<TemperatureChangedEventArgs>? TemperatureChanged;

    private double _current;

    public void SetTemperature(double value)
    {
        if (value == _current) return;
        double old = _current;
        _current = value;
        OnTemperatureChanged(new TemperatureChangedEventArgs(old, value));
    }

    protected virtual void OnTemperatureChanged(TemperatureChangedEventArgs e)
        => TemperatureChanged?.Invoke(this, e);   // ?. — подписчиков может не быть
}

public sealed class TemperatureChangedEventArgs(double oldValue, double newValue) : EventArgs
{
    public double OldValue { get; } = oldValue;
    public double NewValue { get; } = newValue;
}

var thermostat = new Thermostat();
thermostat.TemperatureChanged += (sender, e) => Console.WriteLine($"{e.OldValue} → {e.NewValue}");
thermostat.SetTemperature(21.5);   // вывод: 0 → 21,5 (или 21.5 — зависит от культуры)
```

### Стандартный паттерн .NET

- Тип события — `EventHandler` или `EventHandler<TEventArgs>`: `void (object? sender, TEventArgs e)`.
- `sender` — кто вызвал событие (обычно `this`), `e` — данные, наследник `EventArgs` (если данных нет — `EventArgs.Empty`).
- Вызов — в методе `protected virtual void OnXxx(...)`, чтобы наследники могли переопределить.
- Имена: событие — глагол/причастие (`PriceChanged`, `Closing`), аргументы — `XxxEventArgs`.

### `event` vs публичное поле-делегат

```csharp
public class Bad  { public Action? Changed; }            // поле
public class Good { public event Action? Changed; }      // событие

var bad = new Bad();
bad.Changed += Handler;
bad.Changed = null;       // компилируется: кто угодно стёр ВСЕ подписки
bad.Changed?.Invoke();    // компилируется: кто угодно «вызвал событие» от имени объекта

var good = new Good();
good.Changed += Handler;  // можно
// good.Changed = null;   // CS0070: снаружи доступны только += и -=
// good.Changed();        // CS0070
```

Ключевое слово `event` оставляет снаружи только `+=` и `-=` (методы `add`/`remove`), а вызов и присваивание — только внутри класса-владельца.

> **Почему так?** Это инкапсуляция для делегатов. Публикатор контролирует, когда событие происходит; подписчики не могут помешать друг другу (стереть чужие подписки) или подделать событие. Поле-делегат подходит для одного callback-а «владелец задаёт поведение», событие — для «многие наблюдают».

> 💼 **На собеседовании:** «Event — обёртка над делегатом, которая снаружи разрешает только подписку и отписку. Вызвать событие или заменить список подписчиков может только класс-владелец. Это защищает от `= null`, стирающего чужие подписки, и от вызова события извне».

### `?.Invoke` и потокобезопасность

`Changed?.Invoke(...)` читает поле один раз. Старый вариант `if (Changed != null) Changed(...)` читал его дважды — между проверкой и вызовом другой поток мог отписаться последним → `NullReferenceException`. Раньше писали `var handler = Changed; if (handler != null) handler(...)` — `?.` делает то же самое короче.

→ **Задача 3** — `StockTicker` с событием и подписчиками `PriceAlert`.

---

## 7. Отписка и утечки памяти

Подписка `publisher.Event += subscriber.Handle` кладёт в publisher делегат, который хранит ссылку на **subscriber** (`Target`). Пока publisher жив, subscriber не может быть собран GC — даже если больше нигде не используется.

```csharp
public class Window   // живёт недолго
{
    public Window(AppSettings settings) => settings.Changed += OnSettingsChanged;   // подписка
    private void OnSettingsChanged(object? s, EventArgs e) { /* перерисовать */ }
}
// AppSettings — singleton на всё время работы приложения.
// Каждое открытое и «закрытое» окно остаётся в памяти: на него ссылается settings.Changed.
```

Классическая утечка памяти в .NET: долгоживущий publisher (static, singleton, кэш) + короткоживущие подписчики без отписки. Решения:
- **Отписываться** — в `Dispose` (паттерн «подписался в конструкторе — отписался в Dispose»).
- Отписка от **того же** делегата: method group работает (`-= OnChanged` создаёт равный делегат), а анонимную лямбду нужно сохранить в поле, иначе `-=` не найдёт её.
- Weak events — продвинутые сценарии (WPF `WeakEventManager`), модуль 12.

```csharp
EventHandler handler = (s, e) => Console.WriteLine("changed");
settings.Changed += handler;
// ...
settings.Changed -= handler;   // работает: тот же экземпляр делегата
// settings.Changed -= (s, e) => Console.WriteLine("changed");  // НЕ работает: другая лямбда
```

> **Почему так?** GC собирает объекты, недостижимые от корней (static-поля, стек). Делегат в событии — обычная сильная ссылка, и GC не знает, что «подписка больше не нужна». Ответственность за отписку — на программисте, и именно поэтому `IDisposable` так часто встречается рядом с подписками.

> 💼 **На собеседовании:** «Как события приводят к утечкам? Publisher хранит делегат, делегат хранит ссылку на subscriber. Если publisher долгоживущий, подписчики не собираются GC. Решение — отписываться (обычно в Dispose), использовать method group или сохранённый делегат для `-=`».

---

## 8. Observer pattern и event bus

**Observer** (наблюдатель) — паттерн, где объект (subject) оповещает зависимых (observers) об изменениях, не зная их конкретных типов. В C# события — встроенная реализация observer. Альтернатива в BCL — `IObservable<T>`/`IObserver<T>` (основа Reactive Extensions).

**Event bus** (шина событий) идёт дальше: издатель и подписчик не знают даже друг о друге — только о типе сообщения:

```csharp
bus.Subscribe<OrderPlaced>(e => emailService.SendConfirmation(e.OrderId));
bus.Subscribe<OrderPlaced>(e => warehouse.Reserve(e.OrderId));
bus.Publish(new OrderPlaced(OrderId: 42));   // оба обработчика вызваны
```

Так устроены MediatR (notifications), доменные события, брокеры сообщений (RabbitMQ, Kafka — уже между процессами).

> **Почему так?** Слабая связанность (loose coupling): модуль заказов не должен зависеть от email, склада и бонусов. Добавить новую реакцию — значит подписаться, не трогая код издателя. Цена — неявность: по коду издателя не видно, кто и что сделает.

→ **Задача 5** — `EventBus` с отпиской через `IDisposable`.

---

## 9. Делегаты как «строительные блоки»: middleware

Функция, принимающая и возвращающая функцию (higher-order function — функция высшего порядка), позволяет собирать поведение как конструктор. Именно так устроен конвейер ASP.NET Core:

```csharp
public delegate void RequestHandler(RequestContext context);

Func<RequestHandler, RequestHandler> logging = next => context =>
{
    Console.WriteLine($"→ {context.Path}");
    next(context);                         // передать дальше
    Console.WriteLine($"← {context.StatusCode}");
};

RequestHandler endpoint = ctx => ctx.StatusCode = 200;
RequestHandler app = logging(endpoint);    // «обернули» endpoint
app(new RequestContext("/orders"));
// вывод:
// → /orders
// ← 200
```

Каждый middleware получает `next` и решает: сделать что-то до, вызвать `next` (или не вызвать — short-circuit, например при ошибке авторизации), сделать что-то после. Конвейер собирается «матрёшкой» с конца.

> **Почему так?** Сквозные задачи (логирование, авторизация, обработка ошибок, замер времени) не смешиваются с бизнес-логикой и переставляются как кубики. Замыкание на `next` хранит «остаток конвейера». В модуле 15 ты увидишь `app.Use(async (context, next) => { ...; await next(); ... })` — та же идея с async.

→ **Задача 7** ⭐ — свой `PipelineBuilder`.

---

## Типичные ошибки

1. **Захват переменной цикла `for` в лямбде** — все делегаты видят последнее значение.
2. **Вызов события без `?.`** — `NullReferenceException`, когда подписчиков нет.
3. **Публичное поле-делегат вместо `event`** — внешний код может стереть подписки или вызвать событие.
4. **Нет отписки** от долгоживущего publisher — утечка памяти.
5. **Отписка анонимной лямбдой** (`-= x => ...`) — ничего не удаляет.
6. **Ожидание результатов всех методов multicast-делегата** — возвращается только последний.
7. **Проверка аргументов внутри итератора** в своих LINQ-подобных методах — исключение «запаздывает».
8. **Изменение списка подписчиков во время рассылки** без snapshot — `InvalidOperationException`.
9. **`async void` обработчики** (кроме событий UI) — исключения не перехватываются (подробно в модуле 13).
10. **Тяжёлые замыкания в горячем коде** — лишние аллокации; используй `static`-лямбды, где захват не нужен.

---

## Шпаргалка

```csharp
delegate int Op(int a, int b);            // свой тип делегата
Op op = Add; op(1, 2); op.Invoke(1, 2);   // method group, вызов

Action / Action<T>                        // void
Func<T, TResult>                          // результат — последний параметр
Predicate<T>                              // T → bool (≠ Func<T, bool>!)

x => x * 2                                // expression lambda
(a, b) => { return a + b; }               // statement lambda
static x => x * 2                         // без захвата
_ => 0                                    // параметр не нужен

d += M; d -= M;                           // multicast; результат — последнего
d.GetInvocationList()

for (int i...) { int copy = i; list.Add(() => copy); }   // ловушка for

public event EventHandler<XxxEventArgs>? Xxx;
protected virtual void OnXxx(XxxEventArgs e) => Xxx?.Invoke(this, e);
ctor: source.Xxx += OnXxx;   Dispose: source.Xxx -= OnXxx;

Func<Handler, Handler> middleware = next => ctx => { /*до*/ next(ctx); /*после*/ };
```

---

## Глоссарий

| Термин | Что значит |
|---|---|
| Delegate | Тип-ссылка на метод с определённой сигнатурой |
| Multicast delegate | Делегат со списком из нескольких методов (invocation list) |
| `Func` / `Action` / `Predicate` | Готовые generic-делегаты: с результатом / без результата / T → bool |
| Lambda | Анонимная функция `x => ...` |
| Method group | Имя метода без вызова, преобразуемое в делегат |
| Closure | Замыкание: лямбда + захваченные внешние переменные |
| Display class | Скрытый класс, который компилятор создаёт для захваченных переменных |
| Event | Член класса для оповещения подписчиков; снаружи только `+=`/`-=` |
| `EventArgs` | Базовый класс данных события |
| Observer pattern | Паттерн «наблюдатель»: subject оповещает observers |
| Event bus | Шина событий: издатели и подписчики связаны только типом сообщения |
| Middleware | Звено конвейера обработки, оборачивающее следующее звено |
| Higher-order function | Функция, принимающая или возвращающая функцию |
| Memoization | Кэширование результатов функции по аргументам |

---

## Вопросы на собеседовании

<details><summary><b>Что такое делегат?</b></summary>

Типобезопасная ссылка на метод (или несколько методов). Объявление `delegate` создаёт класс-наследник `MulticastDelegate`, экземпляр которого хранит `Target` (объект или null) и `Method`. Делегаты позволяют передавать поведение параметром: callbacks, LINQ, события, стратегии.
</details>

<details><summary><b>Чем Func отличается от Action и Predicate?</b></summary>

`Action<…>` — метод без возвращаемого значения, `Func<…, TResult>` — с результатом (последний параметр типа), `Predicate<T>` — `T → bool`. Это готовые generic-делегаты из BCL, чтобы не объявлять свой тип на каждую сигнатуру. `Predicate<int>` и `Func<int, bool>` — разные типы, напрямую не присваиваются.
</details>

<details><summary><b>Что такое multicast-делегат? Что вернёт вызов, если методов несколько?</b></summary>

Делегат со списком методов, которые вызываются по порядку (`+=` добавляет, `-=` удаляет). Возвращается результат последнего метода, остальные теряются. Если метод бросает исключение, следующие не вызываются. Получить все результаты можно, перебрав `GetInvocationList()` вручную.
</details>

<details><summary><b>Чем event отличается от публичного поля-делегата?</b></summary>

`event` снаружи разрешает только `+=` и `-=`. Вызвать событие и присвоить делегат (`= null`) может только класс-владелец. С публичным полем любой код может стереть все подписки или вызвать событие от чужого имени. Event — инкапсуляция для делегатов.
</details>

<details><summary><b>Что такое замыкание? Какая ловушка с циклом for?</b></summary>

Замыкание — лямбда, использующая внешние переменные; захватывается сама переменная (компилятор переносит её в поле скрытого класса), а не её значение. В `for` переменная одна на весь цикл, поэтому лямбды, вызванные после цикла, видят последнее значение (`3 3 3`). Решение — локальная копия в теле цикла. В `foreach` с C# 5 переменная своя на каждой итерации.
</details>

<details><summary><b>Как события могут привести к утечке памяти?</b></summary>

Publisher хранит делегат, делегат — ссылку на subscriber. Если publisher живёт долго (singleton, static), то подписчики не собираются GC, пока не отпишутся. Решение — отписка (обычно в `Dispose`), weak events в специальных случаях. Для отписки нужен тот же делегат: method group подходит, анонимную лямбду нужно сохранить.
</details>

<details><summary><b>Зачем писать Event?.Invoke(this, e), а не Event(this, e)?</b></summary>

Если подписчиков нет, поле события равно `null` — прямой вызов бросит `NullReferenceException`. `?.Invoke` проверяет и вызывает, читая поле один раз, — это ещё и защищает от гонки, когда последний подписчик отписывается между проверкой и вызовом в другом потоке.
</details>

<details><summary><b>Что такое EventHandler&lt;TEventArgs&gt; и почему у событий есть sender?</b></summary>

Стандартный делегат для событий: `void (object? sender, TEventArgs e)`. `sender` — источник события (один обработчик можно подписать на несколько объектов и различать их), `e` — данные события, наследник `EventArgs`. Единый паттерн делает события совместимыми с инструментами и понятными всем .NET-разработчикам.
</details>

<details><summary><b>Что выведет код с for и лямбдами?</b></summary>

```csharp
var list = new List<Func<int>>();
for (int i = 0; i < 3; i++) list.Add(() => i);
Console.WriteLine(string.Join(",", list.Select(f => f())));
```
`3,3,3`. Все лямбды захватили одну переменную `i`, которая после цикла равна 3. С `int copy = i;` — `0,1,2`.
</details>

<details><summary><b>Что такое лямбда и method group? Есть ли разница в производительности?</b></summary>

Лямбда — анонимная функция, которую компилятор превращает в метод (и, если есть захват, — в метод display class). Method group — ссылка на существующий метод по имени. Обе преобразуются в делегат. Лямбда с захватом аллоцирует display class; лямбды без захвата компилятор кэширует в static-поле. Method group исторически создавал новый делегат при каждом использовании (C# 11 начал кэшировать для static-методов).
</details>

<details><summary><b>Почему события в .NET возвращают void?</b></summary>

Событие — multicast: подписчиков может быть много, а вернуть можно только результат последнего. Это неоднозначно, поэтому соглашение — `void`. Если подписчикам нужно «ответить» (например, отменить действие), используют изменяемые EventArgs: `CancelEventArgs.Cancel = true`.
</details>

<details><summary><b>Как устроен middleware-конвейер на делегатах?</b></summary>

Middleware — функция `Func<Handler, Handler>`: получает следующий обработчик `next` и возвращает новый, который может выполнить код до, вызвать `next` (или не вызывать — short-circuit) и выполнить код после. Конвейер собирается с конца: последний middleware оборачивает конечный обработчик и т.д. Так устроен ASP.NET Core.
</details>

---

## Дополнительно

- [Delegates — learn.microsoft.com](https://learn.microsoft.com/dotnet/csharp/delegates-overview)
- [Events](https://learn.microsoft.com/dotnet/csharp/events-overview)
- [Lambda expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions)
- [Standard .NET event patterns](https://learn.microsoft.com/dotnet/csharp/event-pattern)
- [ASP.NET Core Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware/) — забегая вперёд, в модуль 15.
- Джеффри Рихтер, «CLR via C#» — главы 11 «Events» и 17 «Delegates».
- Джон Скит, «C# in Depth» — главы про делегаты, анонимные методы, лямбды и захват переменных.
