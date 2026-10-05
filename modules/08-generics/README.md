# Модуль 08. Generics

> **Зачем:** generics (обобщения — типы и методы, параметризованные другими типами) — фундамент всего современного .NET: `List<T>`, `Dictionary<TKey, TValue>`, `Task<T>`, `IEnumerable<T>`, LINQ, DI-контейнер, EF Core. Без понимания generics невозможно читать чужой код и проектировать свой. На собеседовании спрашивают: «зачем нужны generics», «какие бывают constraints», «что такое covariance/contravariance», «чем отличаются generics в C# и Java». **Время:** ~5–6 ч. **Нужно:** модули 02–07 (boxing, интерфейсы, record, исключения).

## Что ты будешь уметь

- Объяснять, какие проблемы решают generics: типобезопасность, отсутствие boxing, переиспользование кода.
- Писать generic-классы, generic-методы и generic-интерфейсы.
- Ограничивать параметры типа через constraints (`where T : class, struct, new(), notnull, IComparable<T>`, базовый класс, интерфейс).
- Правильно использовать `default(T)` и `EqualityComparer<T>.Default`.
- Понимать, что static-поле generic-типа своё для каждого закрытого типа.
- Объяснять covariance (`out`) и contravariance (`in`) на примере `IEnumerable<out T>` и `Action<in T>`.

---

## 1. Зачем нужны generics

До .NET 2.0 (2005) generics не было. Универсальные коллекции хранили всё как `object`:

```csharp
using System.Collections;

var list = new ArrayList();
list.Add(42);        // int упаковывается в object — boxing (аллокация в куче)
list.Add("oops");    // компилятор не возражает: тип — object
int first = (int)list[0]!;   // явный cast + unboxing
int second = (int)list[1]!;  // InvalidCastException во время ВЫПОЛНЕНИЯ
```

Три проблемы:
1. **Нет типобезопасности** — ошибка типа обнаруживается в runtime, а не при компиляции.
2. **Boxing/unboxing** для value types — лишние аллокации и нагрузка на GC.
3. **Касты везде** — шумный и хрупкий код.

С generics:

```csharp
var numbers = new List<int>();
numbers.Add(42);          // без boxing: внутри List<int> лежит int[]
// numbers.Add("oops");   // ошибка компиляции CS1503 — поймали ДО запуска
int first = numbers[0];   // без cast
```

Альтернатива «написать `IntList`, `StringList`, `OrderList`…» — копипаста. Generics дают **один** код для любых типов с полной проверкой компилятора.

> **Почему так?** Ошибку, найденную компилятором, исправить в сотни раз дешевле, чем найденную пользователем в production. Generics переносят проверку типов на этап компиляции и одновременно убирают boxing — выигрыш и в надёжности, и в производительности. Поэтому необобщённые `ArrayList`/`Hashtable` в новом коде считаются устаревшими.

> 💼 **На собеседовании:** «Generics дают типобезопасность на этапе компиляции, избавляют от boxing для value types и позволяют переиспользовать алгоритм для разных типов. Классический пример — `ArrayList` против `List<T>`».

---

## 2. Generic-классы, методы, интерфейсы

### Generic-класс

```csharp
public class Box<T>
{
    public Box(T value) => Value = value;
    public T Value { get; }
}

var intBox = new Box<int>(5);           // T = int
var textBox = new Box<string>("hi");    // T = string
Console.WriteLine(intBox.Value + 1);    // вывод: 6
```

`Box<T>` — **открытый** (open) generic-тип, `Box<int>` — **закрытый** (closed, constructed) тип. Параметров может быть несколько: `Dictionary<TKey, TValue>`. Соглашение об именах: `T` для одного параметра, `TKey`, `TValue`, `TResult` — для нескольких.

### Generic-метод и вывод типов (type inference)

```csharp
public static class ArrayHelper
{
    public static T Last<T>(T[] items) => items[^1];
}

int x = ArrayHelper.Last(new[] { 1, 2, 3 });        // T выведен как int — вывод: 3
string s = ArrayHelper.Last<string>(["a", "b"]);    // явное указание T
```

Компилятор выводит `T` по аргументам. Если вывести нельзя (например, `T` только в возвращаемом значении), укажи тип явно: `CreateMany<StringBuilder>(3)`.

### Generic-интерфейс

```csharp
public interface IRepository<T>
{
    T? GetById(int id);
    void Add(T entity);
}

public class BookRepository : IRepository<Book> { /* ... */ }        // закрываем T
public class InMemoryRepository<T> : IRepository<T> { /* ... */ }   // пробрасываем T дальше
```

> **Почему так?** Generic-метод удобнее generic-класса, когда параметр нужен только одной операции: `GenericAlgorithms.Max(list)` не требует создавать объект «максимизатор целых чисел». Generic-класс нужен, когда тип — часть **состояния** (элементы стека, значения кэша).

→ **Задача 1** (см. exercises.md) — свой `SimpleStack<T>`.

---

## 3. Как generics устроены в CLR

C# компилирует `List<T>` в IL **один раз** — с параметром типа. Конкретизация происходит в runtime (JIT):

- Для **reference types** (`List<string>`, `List<Order>`) JIT генерирует **один общий** машинный код: все ссылки одного размера, работа с ними одинакова.
- Для **каждого value type** (`List<int>`, `List<double>`, `List<DateOnly>`) JIT генерирует **отдельный** специализированный код — поэтому внутри `List<int>` лежит настоящий `int[]` без boxing.

Это называется **reification** (овеществление): информация о `T` есть во время выполнения, `typeof(T)` работает, `List<int>` и `List<string>` — разные типы.

```csharp
Console.WriteLine(typeof(List<int>) == typeof(List<string>));  // вывод: False
Console.WriteLine(typeof(List<>).Name);                        // вывод: List`1
```

В Java generics реализованы через **type erasure** (стирание типов): в runtime `List<Integer>` — это просто `List` с `Object` внутри, а примитивы обязательно упаковываются.

> **Почему так?** Разработчики .NET 2.0 изменили сам runtime (а не только компилятор), чтобы получить производительность для value types и полную информацию о типах в runtime. Java выбрала обратную совместимость со старыми JVM. Отсюда и популярный вопрос на собеседовании «чем generics C# отличаются от Java».

> 💼 **На собеседовании:** «В .NET generics реифицированы: для reference types JIT использует общий код, для каждого value type — отдельную специализацию, поэтому `List<int>` не делает boxing. В Java — type erasure».

---

## 4. Constraints — ограничения параметров типа

Внутри `Max<T>` компилятор знает о `T` только то, что это `object`. Значит, `a.CompareTo(b)` или `a > b` написать нельзя:

```csharp
public static T Max<T>(T a, T b) => a > b ? a : b;  // CS0019: оператор > нельзя применить к T
```

Constraint (`where`) сообщает компилятору, что `T` умеет больше — и **сужает** набор допустимых типов:

```csharp
public static T Max<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) >= 0 ? a : b;

Console.WriteLine(Max(3, 7));              // вывод: 7
Console.WriteLine(Max("apple", "pear"));   // вывод: pear
// Max(new object(), new object());        // CS0311: object не реализует IComparable<object>
```

| Constraint | Значит | Зачем |
|---|---|---|
| `where T : class` | reference type | можно вернуть `null`, `T?` — nullable reference |
| `where T : class?` | reference type, допускается nullable | редко |
| `where T : struct` | non-nullable value type | `T?` становится `Nullable<T>`; без boxing |
| `where T : notnull` | любой non-nullable тип | ключи словарей (`Dictionary<TKey, TValue> where TKey : notnull`) |
| `where T : new()` | есть public конструктор без параметров | можно писать `new T()` |
| `where T : BaseClass` | наследник `BaseClass` (или он сам) | доступ к членам базового класса |
| `where T : IInterface` | реализует интерфейс | вызов методов интерфейса |
| `where T : U` | `T` — это `U` или наследник `U` | связь двух параметров |
| `where T : unmanaged` | value type без ссылок внутри | низкоуровневый код, `Span`, interop |

Комбинации пишутся через запятую, **порядок важен**: сначала `class`/`struct`/`notnull`/`unmanaged`, потом базовый класс, потом интерфейсы, `new()` — последним:

```csharp
public class InMemoryRepository<T> where T : class, IEntity, new() { }

public class Converter<TIn, TOut>
    where TIn : notnull
    where TOut : struct
{ }
```

> **Почему так?** Constraint — это контракт в обе стороны. Автор generic-кода получает право вызывать методы (`CompareTo`, `entity.Id`, `new T()`), а пользователь — ошибку компиляции, если подставил неподходящий тип. Без constraints пришлось бы проверять тип в runtime (`if (item is IComparable c)`) и бросать исключения — то есть вернуться к проблемам `ArrayList`.

> 💼 **На собеседовании:** перечисли основные constraints и приведи пример: «`where T : IComparable<T>` — чтобы сравнивать элементы в Max/Sort; `where T : class, IEntity` — в репозитории, чтобы иметь доступ к `Id` и возвращать `null`; `new()` — чтобы создавать экземпляры».

→ **Задача 2** — `GenericAlgorithms`: `Max`, `Clamp`, `Swap`, `CreateMany`.
→ **Задача 3** — `InMemoryRepository<T> where T : class, IEntity`.

---

## 5. `default(T)` и сравнение значений T

`default(T)` — значение по умолчанию для типа: `0` для чисел, `false` для `bool`, `null` для reference types, «все поля по нулям» для struct.

```csharp
static T FirstOrDefault<T>(List<T> items) => items.Count > 0 ? items[0] : default!;

Console.WriteLine(FirstOrDefault(new List<int>()));      // вывод: 0
Console.WriteLine(FirstOrDefault(new List<string>()) is null);  // вывод: True
```

С C# 7.1 можно писать просто `default` — тип выводится из контекста.

**Ловушка:** `item == default` для неограниченного `T` не компилируется (CS0019) — компилятор не знает, есть ли у `T` оператор `==`. Правильный способ:

```csharp
static bool IsDefault<T>(T value) => EqualityComparer<T>.Default.Equals(value, default!);

Console.WriteLine(IsDefault(0));        // вывод: True
Console.WriteLine(IsDefault("x"));      // вывод: False
Console.WriteLine(IsDefault<string?>(null));  // вывод: True
```

`EqualityComparer<T>.Default` выбирает лучшую стратегию: если `T` реализует `IEquatable<T>` — вызывает `Equals(T)` **без boxing**, иначе — `object.Equals`. Так же сравнивают элементы `List<T>.Contains`, `Dictionary`, `HashSet`.

> **Почему так?** Операторы в C# — статические методы, а до C# 11 интерфейсы не могли требовать наличия статических членов. Поэтому generic-код не может полагаться на `==`. `EqualityComparer<T>.Default` — универсальный «мост», который работает и для `int`, и для `string`, и для твоего record.

> **Про nullable и `default!`.** Для неограниченного `T` выражение `default` может быть `null` (если `T` — reference type). Если тип возврата объявлен как `T`, компилятор предупредит. Варианты: вернуть `T?`, пометить атрибутом `[MaybeNullWhen(false)]` (как в Try-методах) или подавить `!`, если ты точно понимаешь контракт.

---

## 6. Try-pattern в generic-коде

Методы `TryGetValue`, `TryPop`, `TryParse` возвращают `bool` и результат через `out`. В generic-коде к ним добавляют атрибут из `System.Diagnostics.CodeAnalysis`:

```csharp
using System.Diagnostics.CodeAnalysis;

public bool TryPop([MaybeNullWhen(false)] out T item)
{
    if (_count == 0) { item = default; return false; }
    item = _items[--_count];
    return true;
}
```

`[MaybeNullWhen(false)]` говорит анализатору nullable: «если метод вернул `false`, в `item` может быть `null`; если `true` — значение нормальное». Поэтому после `if (stack.TryPop(out var x))` компилятор не ругается на `x`.

> **Почему так?** Исключения дорогие и предназначены для исключительных ситуаций. «Стек пуст» или «ключа нет в кэше» — обычный сценарий, поэтому BCL предлагает пару: `Pop()` (бросает) и `TryPop()` (не бросает). Используй Try-версию, когда отсутствие значения — нормальный путь выполнения.

→ **Задача 4** — `Cache<TKey, TValue>` с `TryGet` и `GetOrAdd`.

---

## 7. Static-поля в generic-типах

Каждый **закрытый** generic-тип — отдельный тип со своими static-полями:

```csharp
public class Counter<T>
{
    public static int Count;
}

Counter<int>.Count++;
Counter<int>.Count++;
Counter<string>.Count++;
Console.WriteLine(Counter<int>.Count);     // вывод: 2
Console.WriteLine(Counter<string>.Count);  // вывод: 1
```

Это используют намеренно — например, кэш на тип: `EqualityComparer<T>.Default` вычисляется один раз для каждого `T` и хранится в static-поле `EqualityComparer<T>`. Static-конструктор `Counter<T>` тоже выполняется отдельно для каждого `T`.

> **Почему так?** Раз `List<int>` и `List<string>` — разные типы в runtime (reification), у каждого своё static-состояние. Это одновременно ловушка («я думал, счётчик общий») и мощный приём «generic static cache» — быстрый кэш без словаря и блокировок.

→ **Задача 5** (🧪 исследование) — boxing в `ArrayList` vs `List<int>` и static-поля generic-типов.

---

## 8. Covariance и contravariance

Начнём с вопроса: если `string` — это `object`, то является ли `List<string>` списком `List<object>`?

```csharp
List<string> strings = new() { "a" };
// List<object> objects = strings;   // CS0029 — нельзя!
// objects.Add(42);                   // ...иначе можно было бы положить int в список строк
```

Generic-типы по умолчанию **инвариантны** (invariant): `List<string>` и `List<object>` никак не связаны. Но для «только читающих» интерфейсов это ограничение излишне:

```csharp
IEnumerable<string> strings = new List<string> { "a", "b" };
IEnumerable<object> objects = strings;   // OK! IEnumerable<out T> — ковариантен
```

`IEnumerable<T>` только **отдаёт** `T` наружу — через него нельзя ничего добавить. Значит, безопасно считать последовательность строк последовательностью объектов.

### Covariance — `out T`

```csharp
public interface IProducer<out T>
{
    T Produce();              // T только в выходной позиции
    // void Accept(T item);   // CS1961: out-параметр нельзя использовать как входной
}
```

Ковариантность сохраняет направление наследования: `string → object` ⇒ `IProducer<string> → IProducer<object>`. Примеры в BCL: `IEnumerable<out T>`, `IReadOnlyList<out T>`, `IReadOnlyCollection<out T>`, `Func<out TResult>`.

### Contravariance — `in T`

```csharp
Action<object> printAnything = o => Console.WriteLine(o);
Action<string> printString = printAnything;   // OK! Action<in T> — контравариантен
printString("hello");                          // вывод: hello
```

Если метод умеет обрабатывать **любой** `object`, он тем более справится со `string`. Направление **разворачивается**: `string → object` ⇒ `Action<object> → Action<string>`. Примеры в BCL: `Action<in T>`, `IComparer<in T>`, `IEqualityComparer<in T>`, `Predicate<in T>`.

`Func<in T, out TResult>` — сразу оба вида: вход контравариантен, выход ковариантен.

### Правила и ограничения

- Вариантность объявляется **только у интерфейсов и делегатов** — у классов и struct нельзя (`class Box<out T>` — ошибка CS1960). Поэтому `List<T>` инвариантен, а `IReadOnlyList<out T>` — ковариантен.
- Работает только для **reference types**: `IEnumerable<int>` нельзя присвоить `IEnumerable<object>` — для этого понадобился бы boxing каждого элемента.
- Мнемоника: **out** — «наружу отдаю» (producer), **in** — «внутрь принимаю» (consumer).

> **Почему так?** Вариантность — компромисс между гибкостью и безопасностью. Если бы generics были ковариантны всегда (как массивы — см. ниже), возможны ошибки в runtime. Если бы были всегда инвариантны — нельзя было бы передать `IEnumerable<Cat>` в метод, принимающий `IEnumerable<Animal>`. Поэтому C# разрешает вариантность явно и только там, где компилятор может доказать безопасность: `out` — только выходы, `in` — только входы.

**Исторический урок — ковариантность массивов:**

```csharp
object[] items = new string[2];   // компилируется: массивы ковариантны (наследие Java)
items[0] = 42;                     // ArrayTypeMismatchException в runtime!
```

Массивы ковариантны небезопасно, и CLR вынужден проверять тип при каждой записи в массив reference types. Generic-вариантность сделана правильно: небезопасный код просто не компилируется.

> 💼 **На собеседовании:** «Covariance (`out`) позволяет использовать `IEnumerable<string>` там, где ожидается `IEnumerable<object>`, — тип только возвращается. Contravariance (`in`) позволяет использовать `Action<object>` как `Action<string>` — тип только принимается. Работает только для интерфейсов и делегатов и только для reference types. `List<T>` инвариантен, потому что и принимает, и отдаёт `T`».

→ **Задача 6** ⭐ — вариантность в системе сообщений.

---

## 9. Generic math (обзор)

С C# 11 / .NET 7 интерфейсы могут содержать `static abstract` члены, и в BCL появились интерфейсы для чисел: `INumber<T>`, `IAdditionOperators<TSelf, TOther, TResult>` и др. Теперь можно написать одну функцию суммы для всех числовых типов:

```csharp
using System.Numerics;

static T Sum<T>(IEnumerable<T> values) where T : INumber<T>
{
    T total = T.Zero;                     // static-член интерфейса
    foreach (T value in values) total += value;   // оператор + через интерфейс
    return total;
}

Console.WriteLine(Sum(new[] { 1, 2, 3 }));         // вывод: 6
Console.WriteLine(Sum(new[] { 1.5m, 2.25m }));     // вывод: 3.75 (в русской культуре — 3,75)
```

Для Junior достаточно знать, что такая возможность есть и решает старую проблему «нельзя использовать `+` в generic-коде». В повседневном коде её применяют редко — в основном авторы библиотек.

> **Почему так?** Раньше приходилось писать `Sum(int[])`, `Sum(double[])`, `Sum(decimal[])` отдельно — LINQ так и делает (десятки перегрузок `Sum`). Static abstract members в интерфейсах позволили выразить «у типа есть оператор +» как constraint.

---

## 10. Когда generics не нужны

- Если метод работает с **конкретным** типом — не обобщай «на будущее» (YAGNI — You Aren't Gonna Need It).
- Если всё, что нужно от параметра, — интерфейс, иногда достаточно принять интерфейс: `void Print(IEnumerable<object> items)`. Generic-метод `Print<T>(IEnumerable<T>)` выигрывает, когда `T` — value type (нет boxing) или когда `T` возвращается наружу.
- `where T : SomeClass` вместо параметра типа `SomeClass` оправдан, если `T` возвращается (`T Clone<T>(T x) where T : Shape` возвращает именно `Circle`, а не `Shape`).

> **Почему так?** Generics добавляют абстракцию, а любая абстракция стоит читаемости. Хороший generic-код появляется из повторения: увидел три одинаковых класса для разных типов — обобщай.

---

## Типичные ошибки

1. **`item == default` или `a > b` для неограниченного `T`** — не компилируется. Используй `EqualityComparer<T>.Default` и constraint `IComparable<T>`.
2. **Ожидание, что `List<Cat>` можно передать как `List<Animal>`.** Нельзя — инвариантность. Принимай `IEnumerable<Animal>` или `IReadOnlyList<Animal>`.
3. **Неправильный порядок constraints** (`where T : new(), class`) — ошибка компиляции: `new()` всегда последний.
4. **Забыть, что `T?` означает разное** для `class` (nullable reference annotation) и `struct` (`Nullable<T>`). Без constraint `T?` — просто «может быть default».
5. **Static-поле generic-класса как «общий» счётчик** — на деле он свой для каждого `T`.
6. **Обобщение «на всякий случай»** — `Helper<T>` с одним использованием только усложняет код.
7. **Хранить в generic-коллекции ссылки на удалённые элементы** (в своём стеке не обнулять ячейку) — объект не соберёт GC.
8. **Использовать `ArrayList`/`Hashtable` в новом коде** — boxing и потеря типобезопасности.

---

## Шпаргалка

```csharp
class Box<T> { public T Value { get; set; } = default!; }   // generic-класс
T Max<T>(T a, T b) where T : IComparable<T> => ...;         // generic-метод + constraint
interface IRepository<T> where T : class, IEntity { }       // generic-интерфейс

where T : class          // reference type (можно null)
where T : struct         // value type (T? = Nullable<T>)
where T : notnull        // не nullable
where T : new()          // new T() — ставится последним
where T : Base, IFoo     // базовый класс + интерфейс

default(T) / default                  // 0, false, null, ...
EqualityComparer<T>.Default.Equals    // сравнение T без == и без boxing
Comparer<T>.Default.Compare           // сравнение T для сортировки

interface IProducer<out T>   // covariance: только возвращает T
interface IConsumer<in T>    // contravariance: только принимает T
IEnumerable<string> → IEnumerable<object>   ✔ (out)
Action<object> → Action<string>             ✔ (in)
List<string> → List<object>                 ✘ (invariant)

Counter<int>.Count и Counter<string>.Count — РАЗНЫЕ static-поля
```

---

## Глоссарий

| Термин | Что значит |
|---|---|
| Generics | Обобщения: типы и методы с параметрами типа (`List<T>`) |
| Type parameter | Параметр типа (`T`, `TKey`) — «переменная» для типа |
| Type argument | Конкретный тип, подставленный вместо параметра (`int` в `List<int>`) |
| Open / closed generic type | Открытый (`List<>`) / закрытый (`List<int>`) generic-тип |
| Constraint | Ограничение на параметр типа (`where T : ...`) |
| Type inference | Вывод компилятором параметров типа по аргументам метода |
| Boxing | Упаковка value type в объект в куче |
| Reification | Генерики существуют в runtime как реальные типы (C#), в отличие от type erasure (Java) |
| Invariance | Generic-типы с разными аргументами не совместимы (`List<T>`) |
| Covariance (`out`) | Совместимость в направлении наследования: `IEnumerable<string>` → `IEnumerable<object>` |
| Contravariance (`in`) | Совместимость в обратном направлении: `Action<object>` → `Action<string>` |
| `default(T)` | Значение по умолчанию для типа |
| Generic math | Обобщённая арифметика через `INumber<T>` и static abstract members (.NET 7+) |

---

## Вопросы на собеседовании

<details><summary><b>Зачем нужны generics? Какие проблемы они решают?</b></summary>

Generics позволяют написать код один раз для разных типов, сохраняя проверку типов компилятором. До них коллекции (`ArrayList`) хранили `object`: ошибки типов ловились только в runtime через `InvalidCastException`, а value types упаковывались (boxing) — лишние аллокации. `List<int>` хранит `int[]` без boxing и не даст положить строку. Итог: типобезопасность, производительность, переиспользование кода.
</details>

<details><summary><b>Какие constraints бывают у параметров типа?</b></summary>

`class` (reference type), `struct` (non-nullable value type), `notnull`, `unmanaged`, `new()` (конструктор без параметров), базовый класс, интерфейс, другой параметр типа (`where T : U`). Constraints дают право вызывать члены (`CompareTo`, `new T()`) и ограничивают допустимые типы на этапе компиляции. `new()` пишется последним, `class`/`struct` — первыми.
</details>

<details><summary><b>Что такое default(T) и когда он нужен?</b></summary>

Значение по умолчанию для типа: `0`, `false`, `null`, struct с нулевыми полями. Нужен в generic-коде, когда надо вернуть «пустое» значение, не зная, reference это тип или value: `TryGetValue` возвращает `default` в out-параметре при неудаче. Сравнивать с `default` нужно через `EqualityComparer<T>.Default.Equals`, а не `==`.
</details>

<details><summary><b>Чем generics в C# отличаются от generics в Java?</b></summary>

В .NET generics реифицированы: информация о типе сохраняется в runtime, `typeof(T)` работает, `List<int>` и `List<string>` — разные типы. Для value types JIT генерирует специализированный код без boxing, для reference types — общий код. В Java — type erasure: в runtime остаётся `List<Object>`, примитивы всегда упаковываются, `new T()` невозможен.
</details>

<details><summary><b>Что такое covariance и contravariance? Примеры из BCL.</b></summary>

Covariance (`out T`) — можно использовать более конкретный тип там, где ожидается более общий: `IEnumerable<string>` как `IEnumerable<object>`. Возможна, если `T` только возвращается. Contravariance (`in T`) — наоборот: `Action<object>` как `Action<string>`, `IComparer<object>` как `IComparer<string>`; возможна, если `T` только принимается. Вариантность есть только у интерфейсов и делегатов и только для reference types.
</details>

<details><summary><b>Почему List&lt;string&gt; нельзя присвоить переменной List&lt;object&gt;?</b></summary>

`List<T>` и принимает `T` (`Add`), и отдаёт (`indexer`), поэтому он инвариантен. Если бы присваивание было разрешено, через `List<object>` можно было бы добавить `int` в список строк — нарушение типобезопасности. Если нужно только читать — используй `IEnumerable<object>` или `IReadOnlyList<object>`: они ковариантны.
</details>

<details><summary><b>Почему массивы ковариантны и чем это плохо?</b></summary>

`object[] arr = new string[1];` компилируется — так сделали для совместимости с Java на заре .NET. Но запись `arr[0] = 42` упадёт в runtime с `ArrayTypeMismatchException`, а CLR вынужден проверять тип при каждой записи в массив reference types (небольшая потеря производительности). Generic-вариантность безопасна: проверка на этапе компиляции.
</details>

<details><summary><b>Есть ли у каждого List&lt;T&gt; свои static-поля?</b></summary>

Да. Каждый закрытый тип (`Counter<int>`, `Counter<string>`) — отдельный тип в runtime со своими static-полями и своим static-конструктором. Это используют для кэша на тип (так устроен `EqualityComparer<T>.Default`), но это и ловушка, если ожидаешь общий счётчик.
</details>

<details><summary><b>Как написать generic-метод, который находит максимум?</b></summary>

```csharp
static T Max<T>(IEnumerable<T> items) where T : IComparable<T>
{
    using var e = items.GetEnumerator();
    if (!e.MoveNext()) throw new InvalidOperationException("Empty");
    T max = e.Current;
    while (e.MoveNext())
        if (e.Current.CompareTo(max) > 0) max = e.Current;
    return max;
}
```
Constraint `IComparable<T>` нужен, чтобы вызвать `CompareTo`; без него оператор `>` для `T` не компилируется.
</details>

<details><summary><b>Что значит constraint new() и какие у него ограничения?</b></summary>

Он требует public конструктор без параметров и разрешает `new T()`. Передать аргументы в конструктор так нельзя — для этого используют фабрику (`Func<T>`) или DI. Раньше `new T()` внутри использовал reflection (`Activator.CreateInstance<T>`) и был медленнее обычного `new`; в современных версиях .NET это оптимизировано, но фабрики всё равно гибче.
</details>

<details><summary><b>Можно ли сделать вариантным класс?</b></summary>

Нет, только интерфейсы и делегаты. Класс имеет поля, которые можно и читать, и писать, поэтому компилятор не может гарантировать безопасность. Решение — объявить вариантный интерфейс (`IReadOnlyList<out T>`) и реализовать его классом.
</details>

<details><summary><b>Что такое generic math?</b></summary>

Возможность .NET 7+ (C# 11) описывать арифметику в generic-коде через интерфейсы `INumber<T>`, `IAdditionOperators<...>` и static abstract members: `T.Zero`, `a + b` для `T`. Позволяет написать один `Sum<T>` для `int`, `double`, `decimal`. Для Junior достаточно знать о существовании и решаемой проблеме.
</details>

---

## Дополнительно

- [Generics в C# — learn.microsoft.com](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/generics)
- [Constraints on type parameters](https://learn.microsoft.com/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Covariance and contravariance in generics](https://learn.microsoft.com/dotnet/standard/generics/covariance-and-contravariance)
- [Generic math](https://learn.microsoft.com/dotnet/standard/generics/math)
- Джеффри Рихтер, «CLR via C#» — глава 12 «Generics» (как generics устроены в CLR).
- Джон Скит, «C# in Depth» — главы про generics и вариантность (C# 2 и C# 4).
