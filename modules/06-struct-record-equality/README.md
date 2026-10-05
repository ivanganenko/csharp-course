# Модуль 06. struct, record и равенство

> **Зачем:** каждый `Dictionary`, `HashSet`, `Distinct()`, `Contains()` и сравнение DTO в тестах опирается на `Equals`/`GetHashCode`. Неправильное равенство — источник тихих багов «объект есть в словаре, но не находится». На собеседовании: «struct vs class», «зачем GetHashCode», «что такое record», «что будет, если переопределить Equals без GetHashCode» — вопросы из топ-20. **Время:** ~6–8 ч. **Нужно:** модули 02, 04, 05.

## Что ты будешь уметь

- Выбирать между `class` и `struct` и объяснять семантику копирования.
- Писать `readonly struct`, видеть ловушки mutable struct.
- Использовать `record class` / `record struct`, positional records, `with`.
- Реализовывать `Equals`, `GetHashCode`, `==`, `IEquatable<T>` по контракту.
- Объяснять, как `Dictionary`/`HashSet` используют хэш, и почему ключ должен быть неизменяемым.
- Пользоваться tuples и деконструкцией.

---

## 1. struct vs class: семантика копирования

`struct` — **value type** (тип-значение): переменная содержит **сами данные**. `class` — **reference type**: переменная содержит **ссылку**.

```csharp
public struct PointS { public int X; }
public class  PointC { public int X; }

var s1 = new PointS { X = 1 };
var s2 = s1;          // копируются ДАННЫЕ
s2.X = 99;
Console.WriteLine(s1.X);   // вывод: 1

var c1 = new PointC { X = 1 };
var c2 = c1;          // копируется ССЫЛКА
c2.X = 99;
Console.WriteLine(c1.X);   // вывод: 99
```

Копирование происходит при **присваивании, передаче в метод, возврате из метода и чтении из коллекции**.

| | `class` | `struct` |
|---|---|---|
| Категория | reference type | value type |
| Где живёт | managed heap | там, где объявлен: локальная переменная — обычно стек, поле класса — внутри объекта в куче, элемент массива — в массиве |
| Присваивание | копия ссылки | копия всех полей |
| `null` | может быть | нет (кроме `Nullable<T>` / `T?`) |
| Наследование | да | нет (только интерфейсы); неявно `sealed`, базовый — `ValueType` |
| Конструктор без параметров | свой | всегда есть `default` (все поля обнулены) |
| Нагрузка на GC | да (аллокация) | нет, если не упакован (boxing) |
| `Equals` по умолчанию | по ссылке | по значению полей (через рефлексию — медленно) |

**Когда struct** (рекомендации Microsoft): маленький (до ~16 байт), логически представляет **одно значение** (как `int`, `DateTime`, `Point`, `Money`), неизменяемый, редко упаковывается. Во всех остальных случаях — `class`.

> **Почему так?** Struct экономит аллокации: массив из миллиона `struct Point` — один объект в куче, а из миллиона `class Point` — миллион объектов и работа для GC. Но копирование большой структуры (например, 100 байт) при каждой передаче в метод дороже, чем передача 8-байтной ссылки. Поэтому struct выгоден только маленьким. А неизменяемость нужна из-за копий: если struct можно менять, легко изменить **копию** и думать, что изменил оригинал (раздел 2).

> 💼 **На собеседовании:** «Struct — value type, копируется целиком, не может быть null и не наследуется; class — reference type, копируется ссылка. Struct выбираю для маленьких неизменяемых значений вроде координат или денег — чтобы не нагружать GC. "Struct живёт на стеке" — упрощение: struct-поле класса живёт в куче внутри объекта».

---

## 2. Ловушки mutable struct и `readonly struct`

```csharp
public struct Counter
{
    public int Value { get; set; }
    public void Increment() => Value++;
}

var list = new List<Counter> { new Counter() };
list[0].Increment();               // компилируется! Но индексатор вернул КОПИЮ — список не изменился
Console.WriteLine(list[0].Value);  // вывод: 0
// list[0].Value = 5;              // а это НЕ компилируется: CS1612 — компилятор понимает, что меняешь копию

foreach (var c in list) c.Increment();  // тоже меняет копию (переменная foreach — копия элемента)

var copy = list[0];                // правильно: копия → изменение → запись обратно
copy.Increment();
list[0] = copy;
Console.WriteLine(list[0].Value);  // вывод: 1
```

Другие места, где struct тихо копируется: `readonly`-поле (вызов метода на нём меняет «защитную копию» — defensive copy), свойство-getter, возвращающее struct, `in`-параметр.

Решение — **`readonly struct`**: компилятор гарантирует, что все поля `readonly`, и копию «изменить» уже нельзя.

```csharp
public readonly struct Temperature
{
    public Temperature(double celsius) => Celsius = celsius;
    public double Celsius { get; }                                 // только get / init
    public Temperature Add(double delta) => new(Celsius + delta);  // «изменение» = новое значение
}
```

> **Почему так?** Индексатор `List<T>` — это метод `get_Item`, он **возвращает значение**, то есть копию struct. Для массивов `arr[0].Increment()` сработает, потому что элемент массива — переменная, а не результат метода. Это неочевидное различие — главная причина правила «struct должен быть immutable». `readonly struct` делает ошибку невозможной на уровне компилятора и убирает defensive copies (JIT больше не нужно копировать структуру «на всякий случай» перед вызовом метода).

→ **Задача 4** (Warehouse) в [exercises.md](exercises.md).

---

## 3. record: равенство по значению «из коробки»

**Record** (C# 9) — тип, предназначенный для **данных**. Компилятор генерирует: равенство по значению (`Equals`, `GetHashCode`, `==`, `!=`, `IEquatable<T>`), `ToString` с именами свойств, `Deconstruct`, поддержку `with`.

```csharp
public record Person(string Name, int Age);     // positional record (record class)

var a = new Person("Anna", 30);
var b = new Person("Anna", 30);
Console.WriteLine(a == b);                // вывод: True   (у обычного класса было бы False)
Console.WriteLine(ReferenceEquals(a, b)); // вывод: False  (это всё ещё два объекта)
Console.WriteLine(a);                     // вывод: Person { Name = Anna, Age = 30 }

var older = a with { Age = 31 };          // копия с изменённым свойством; a не меняется
var (name, age) = older;                  // Deconstruct
Console.WriteLine($"{name} {age}");       // вывод: Anna 31
```

- `record` / `record class` — reference type. `record struct` — value type. `readonly record struct` — неизменяемый value type.
- Positional record class генерирует `init`-свойства (неизменяемые); positional `record struct` (без `readonly`) — **изменяемые** `{ get; set; }`.
- Можно добавлять свои члены, валидацию, переопределять `ToString`.
- `with` делает **поверхностную** копию (shallow copy): ссылочные поля (списки) копируются как ссылки.

Валидация в positional record — через явное свойство, инициализированное параметром:

```csharp
public record Email(string Value)
{
    public string Value { get; init; } = Value.Contains('@')
        ? Value.Trim().ToLowerInvariant()
        : throw new ArgumentException("Invalid email", nameof(Value));
}
```

⚠️ Такой инициализатор не срабатывает в `with { Value = ... }` — для этого нужен `init`-аксессор с проверкой (так сделано в задаче 5).

> **Ловушка с коллекциями:** record сравнивает свойства через `EqualityComparer<T>.Default`. Для `List<string>` это сравнение **ссылок**, поэтому два record'а с одинаковыми, но разными экземплярами списков **не равны**. Нужно переопределить `Equals`/`GetHashCode` (задача 2).

> **Почему так?** До records для каждого DTO/value object приходилось писать ~50 строк шаблонного `Equals`/`GetHashCode`/`==`/`ToString` — и ошибаться в них (забыть поле, рассогласовать хэш). Records убрали этот шаблон и сделали неизменяемость стилем по умолчанию. Record class остался reference type, потому что большинство DTO — это объекты, которые передаются по ссылке и могут быть большими; для маленьких значений есть `record struct`.

> 💼 **На собеседовании:** «Record — тип для данных с равенством по значению, сгенерированными `ToString`, `Deconstruct` и `with`. `record class` — ссылочный, `record struct` — значимый. Использую для DTO, сообщений, value objects. Подвох: равенство поверхностное — коллекции внутри сравниваются по ссылке».

→ **Задача 2** (records) в [exercises.md](exercises.md).

---

## 4. Равенство: reference vs value, `Equals`, `==`

В .NET два вида равенства:
- **reference equality** (ссылочное) — это один и тот же объект? `ReferenceEquals(a, b)`;
- **value equality** (по значению) — объекты содержат одинаковые данные?

| Тип | `Equals` по умолчанию | `==` по умолчанию |
|---|---|---|
| class | по ссылке | по ссылке |
| struct | по значению (рефлексия, медленно, с boxing) | **не определён** — ошибка компиляции, пока не перегрузишь |
| record / record struct | по значению (сгенерирован) | по значению (сгенерирован) |
| string | по значению (особый случай: класс, но immutable и с перегрузками) | по значению |

```csharp
object o1 = "hi", o2 = new string("hi");
Console.WriteLine(o1 == o2);        // вывод: False — для object == выбран при компиляции и сравнивает ссылки!
Console.WriteLine(o1.Equals(o2));   // вывод: True  — Equals виртуальный, вызвался string.Equals
```

> **Почему так?** `==` — **статический** оператор: какую перегрузку вызвать, решает компилятор по типам переменных (как с перегрузкой методов в модуле 05). `Equals` — **виртуальный** метод: решается в runtime по типу объекта. Поэтому в generic-коде и коллекциях используют `Equals` (через `EqualityComparer<T>.Default`), а не `==`.

### Как правильно реализовать равенство в классе

1. Реализуй `IEquatable<T>.Equals(T? other)` — основная логика.
2. Переопредели `Equals(object?)` — делегируй в п.1.
3. Переопредели `GetHashCode()` — на тех же полях, что участвуют в `Equals`.
4. (По желанию) перегрузи `==` и `!=` — через `Equals`, аккуратно с `null`.

```csharp
public sealed class Isbn : IEquatable<Isbn>
{
    public Isbn(string value) => Value = value;
    public string Value { get; }

    public bool Equals(Isbn? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Isbn other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Isbn? a, Isbn? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(Isbn? a, Isbn? b) => !(a == b);
}
```

Свойства, которые должен соблюдать `Equals`: **рефлексивность** (`a.Equals(a)`), **симметричность** (`a.Equals(b) == b.Equals(a)`), **транзитивность**, **стабильность**, `a.Equals(null) == false`.

> ⚠️ Внутри `operator ==` нельзя писать `a == null` — это рекурсивный вызов самого оператора → `StackOverflowException`. Используй `a is null` (pattern matching не вызывает пользовательские операторы).

→ **Задача 1** (Point) в [exercises.md](exercises.md).

---

## 5. `GetHashCode` и как работают Dictionary/HashSet

`Dictionary<TKey, TValue>` и `HashSet<T>` — хэш-таблицы. Упрощённо:
1. вычисляют `key.GetHashCode()` и по нему выбирают **бакет** (bucket, «корзину»);
2. в бакете перебирают элементы и сравнивают через `Equals`.

Отсюда **контракт** `GetHashCode`:
- если `a.Equals(b)`, то **обязательно** `a.GetHashCode() == b.GetHashCode()`;
- обратное не обязательно: разные объекты могут иметь одинаковый хэш (коллизия) — это нормально, но много коллизий → поиск деградирует от O(1) к O(n);
- хэш объекта не должен меняться, пока объект лежит в коллекции.

```csharp
// Переопределили Equals, но забыли GetHashCode:
var set = new HashSet<BadPoint> { new BadPoint(1, 2) };
Console.WriteLine(set.Contains(new BadPoint(1, 2)));   // вывод: False (почти всегда)
// хэши разные (по умолчанию — от «идентичности» объекта) → ищем в другом бакете → Equals даже не вызывается
```

```csharp
// Изменяемый ключ:
var p = new MutablePoint { X = 1 };
var dict = new Dictionary<MutablePoint, string> { [p] = "A" };
p.X = 2;                                   // хэш изменился
Console.WriteLine(dict.ContainsKey(p));    // вывод: False — объект «потерялся» в старом бакете
```

Как считать хэш: `HashCode.Combine(X, Y, Z)` (до 8 значений) или `new HashCode()` + `Add` в цикле. Избегай `X ^ Y` — даёт одинаковый хэш для `(1, 2)` и `(2, 1)`, а `(5, 5)` и `(7, 7)` — оба 0.

> **Почему так?** Хэш — это «адрес» для быстрого поиска: вместо сравнения с каждым из миллиона элементов сравниваем только с несколькими в одном бакете. Но это работает, лишь если равные объекты гарантированно попадают в один бакет — отсюда и контракт. Компилятор не может его проверить (только warning CS0659), поэтому ошибка проявляется в runtime и только в коллекциях — классический «невидимый» баг.

> 💼 **На собеседовании:** «Dictionary сначала по GetHashCode находит бакет, потом сравнивает Equals внутри него. Поэтому равные объекты обязаны иметь равные хэши, иначе словарь их не найдёт. Если переопределил Equals — переопредели GetHashCode на тех же полях. Ключи должны быть неизменяемыми».

→ **Задача 6 ⭐** (составной ключ) в [exercises.md](exercises.md).

---

## 6. Immutability (неизменяемость)

Неизменяемый объект нельзя изменить после создания: `string`, `DateTime`, `record` с `init`, `readonly struct`.

Плюсы:
- **безопасная передача** — никто не изменит объект у тебя за спиной;
- **потокобезопасность** без `lock` — нечего синхронизировать;
- **безопасные ключи** словарей — хэш не меняется;
- **проще рассуждать** — значение объекта не зависит от времени.

Минус — каждое «изменение» создаёт новый объект (аллокации). Для `string` это решает `StringBuilder`; для коллекций есть `ImmutableList<T>` с разделением структуры.

```csharp
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount, a.Currency);
}

var price = new Money(10m, "USD");
var total = price + new Money(5m, "USD");   // price по-прежнему 10 USD
```

> **Почему так?** Большинство багов в многопоточном и сложном коде — это «кто-то где-то изменил общий объект». Неизменяемость устраняет весь класс таких багов. Поэтому современный C# двигается в эту сторону: records, `init`, `readonly struct`, `required`, immutable collections.

→ **Задача 5** (Money) в [exercises.md](exercises.md).

---

## 7. Tuples (кортежи)

`ValueTuple` — лёгкий **value type** для группировки нескольких значений без объявления типа.

```csharp
(int Min, int Max) MinMax(int[] values) => (values.Min(), values.Max());

var result = MinMax([3, 1, 4]);
Console.WriteLine($"{result.Min}..{result.Max}");   // вывод: 1..4
var (min, max) = MinMax([3, 1, 4]);                 // деконструкция
Console.WriteLine((1, 2) == (1, 2));                // вывод: True — равенство по значению
(min, max) = (max, min);                            // swap без временной переменной
```

- Имена элементов (`Min`, `Max`) существуют только для компилятора; в runtime это `Item1`, `Item2`.
- Старый `System.Tuple<T1,T2>` — **reference type** с `Item1`/`Item2`, сейчас почти не используется.
- Когда tuple, а когда record: tuple — для локальных/приватных результатов «на пару строк»; record — когда данные уходят в публичный API, имеют смысл и поведение.

> **Почему так?** Раньше для возврата двух значений были неудобные варианты: `out`-параметры, `Tuple<,>` с безликими `Item1`, или отдельный класс на одно использование. `ValueTuple` — value type (без аллокации) с именами и деконструкцией. Но именованный кортеж в публичном API — слабый контракт: `(int, int)` ничего не говорит о смысле, и его поля изменяемы.

→ **Задача 3** (Statistics) в [exercises.md](exercises.md).

---

## Типичные ошибки

1. Переопределили `Equals`, но не `GetHashCode` (или на других полях) → объект «не находится» в `HashSet`/`Dictionary`.
2. `GetHashCode` использует изменяемые поля, а объект лежит в словаре.
3. `operator ==` реализован через `left == null` → бесконечная рекурсия.
4. Struct без `IEquatable<T>` в качестве ключа словаря → boxing и рефлексия при каждом сравнении.
5. Изменяемая struct в `List<T>` / `foreach` → изменения теряются.
6. Большие struct (десятки полей) «ради производительности» → дорогие копии.
7. Ожидание, что `record` сравнит списки по содержимому.
8. Ожидание, что `with` делает глубокую копию.
9. Сравнение `object`-переменных через `==` (сравнит ссылки).
10. Хэш через `X ^ Y` → много коллизий.
11. Равенство в иерархии классов (наследник добавил поле — симметричность нарушена); решение — `sealed` или records (у них есть `EqualityContract`).

## Шпаргалка

```csharp
// Класс с равенством по значению
public sealed class P : IEquatable<P>
{
    public int X { get; } public int Y { get; }
    public P(int x, int y) => (X, Y) = (x, y);
    public bool Equals(P? o) => o is not null && X == o.X && Y == o.Y;
    public override bool Equals(object? o) => o is P p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public static bool operator ==(P? a, P? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(P? a, P? b) => !(a == b);
}

public record PersonDto(string Name, int Age);          // reference, init-свойства
public record struct Size(int W, int H);                 // value, ИЗМЕНЯЕМЫЕ свойства!
public readonly record struct Rgb(byte R, byte G, byte B);   // value, неизменяемый
var p2 = p1 with { Age = 31 };                           // shallow copy

public readonly struct Vec { public Vec(double x) => X = x; public double X { get; } }

(string Name, int Count) t = ("a", 1); var (n, c) = t;   // tuple + деконструкция
```

Контракт: `a.Equals(b)` ⇒ `a.GetHashCode() == b.GetHashCode()`. Ключ словаря — неизменяемый.

## Глоссарий

| Термин | Что значит |
|---|---|
| value type | тип, переменная которого содержит данные; копируется целиком |
| reference type | тип, переменная которого содержит ссылку на объект в куче |
| `struct` | пользовательский value type |
| `readonly struct` | struct, все поля которого неизменяемы (проверяет компилятор) |
| defensive copy | скрытая копия struct, которую делает компилятор, чтобы защитить readonly-значение |
| record | тип для данных с генерируемым равенством по значению, `ToString`, `with` |
| positional record | record с параметрами в объявлении: `record P(int X)` |
| `with` expression | создание копии record/struct с изменёнными свойствами |
| shallow copy | копирование полей «как есть», ссылки остаются общими |
| reference equality | равенство ссылок (тот же объект) |
| value equality | равенство по содержимому |
| `IEquatable<T>` | интерфейс строго типизированного `Equals(T)` |
| hash code | целое число для быстрого поиска в хэш-таблице |
| bucket | «корзина» хэш-таблицы, куда попадают элементы с похожим хэшем |
| collision | разные объекты с одинаковым хэшем/бакетом |
| immutability | неизменяемость после создания |
| tuple / ValueTuple | лёгкая группа значений `(int, string)` |
| deconstruction | распаковка в переменные: `var (a, b) = x;` |

## Вопросы на собеседовании

<details><summary><b>Чем struct отличается от class? Когда использовать struct?</b></summary>

Struct — value type: хранит данные прямо в переменной, копируется при присваивании и передаче, не может быть null (без `?`), не поддерживает наследование. Class — reference type: переменная хранит ссылку, объект в куче, копируется ссылка. Struct выбирают для маленьких (≈ до 16 байт), неизменяемых, логически «одно значение» типов: координаты, деньги, цвет, диапазон. Выгода — нет аллокаций и нагрузки на GC; цена — копирование и риск ошибок с изменяемыми struct.
</details>

<details><summary><b>Зачем переопределять <code>GetHashCode</code>, если переопределил <code>Equals</code>?</b></summary>

Потому что хэш-коллекции (`Dictionary`, `HashSet`, LINQ `Distinct`, `GroupBy`) сначала ищут бакет по `GetHashCode`, и только внутри бакета вызывают `Equals`. Контракт: равные объекты обязаны иметь равные хэши. Если хэш остался «по ссылке», два равных объекта попадут в разные бакеты, и `Contains` вернёт false. Хэш считают по тем же полям, что и Equals, например `HashCode.Combine(X, Y)`.
</details>

<details><summary><b>Могут ли у разных объектов быть одинаковые хэш-коды? А у равных — разные?</b></summary>

Разные объекты с одинаковым хэшем — нормально, это коллизия: хэш — `int`, а возможных объектов больше, чем 2³². Коллекция разрешит коллизию через `Equals`, но много коллизий снижает производительность до O(n). Равные объекты с разными хэшами — **нарушение контракта**, коллекции начнут работать неправильно.
</details>

<details><summary><b>Что такое record? Чем record class отличается от record struct?</b></summary>

Record — тип для данных, для которого компилятор генерирует равенство по значению (`Equals`, `GetHashCode`, `==`, `IEquatable<T>`), `ToString` с именами свойств, `Deconstruct` и поддержку `with`. `record class` — reference type (positional-свойства `init`). `record struct` — value type, positional-свойства по умолчанию изменяемые (`get; set;`); `readonly record struct` — неизменяемый. Применение: DTO, сообщения, value objects.
</details>

<details><summary><b>Что делает <code>with</code>? Это глубокая копия?</b></summary>

`with` создаёт копию record (или struct, или анонимного типа) и присваивает указанные `init`-свойства. Копия **поверхностная**: ссылочные поля (списки, объекты) копируются как ссылки, и оригинал с копией будут делить один и тот же список. Для record class `with` вызывает сгенерированный конструктор копирования (`<Clone>$`), который можно переопределить.
</details>

<details><summary><b>Чем <code>==</code> отличается от <code>Equals</code>?</b></summary>

`Equals` — виртуальный метод, вызывается реализация реального типа объекта в runtime. `==` — статический оператор, перегрузка выбирается компилятором по типам переменных. Для классов без перегрузки `==` сравнивает ссылки. Классический пример: `object a = "x", b = new string('x', 1); a == b` → false (ссылки), `a.Equals(b)` → true. Для struct `==` не определён, пока его не перегрузить. Для records и string оба сравнивают по значению.
</details>

<details><summary><b>Что выведет код?</b>

```csharp
var list = new List<Pt> { new Pt { X = 1 } };
foreach (var p in list) { /* p.X = 5;  не компилируется */ }
var item = list[0];
item.X = 10;
Console.WriteLine(list[0].X);
struct Pt { public int X; }
```
</summary>

`1`. `list[0]` возвращает копию struct; меняем локальную копию `item`, элемент списка остаётся прежним. Чтобы изменить элемент, нужно записать копию обратно: `list[0] = item;`. Если бы `Pt` был классом, вывелось бы `10`.
</details>

<details><summary><b>Почему ключ словаря должен быть неизменяемым?</b></summary>

При добавлении словарь кладёт элемент в бакет по текущему хэшу. Если после этого изменить поле, участвующее в `GetHashCode`, хэш станет другим, и поиск пойдёт в другой бакет — элемент «потеряется»: `ContainsKey` вернёт false, хотя объект в словаре, а удалить его тоже не получится. Поэтому ключи — неизменяемые типы: string, int, records, readonly struct.
</details>

<details><summary><b>Как по умолчанию работает <code>Equals</code> у struct? Почему его стоит переопределять?</b></summary>

`ValueType.Equals` сравнивает все поля. Если поля — только примитивы без ссылок, может использоваться быстрое побайтовое сравнение, иначе — рефлексия, что медленно. Кроме того, вызов `Equals(object)` требует boxing аргумента. Поэтому для struct, которые сравниваются или используются как ключи, реализуют `IEquatable<T>` и `GetHashCode` (или используют `record struct`, где это сгенерировано).
</details>

<details><summary><b>Что такое <code>readonly struct</code> и defensive copy?</b></summary>

`readonly struct` — struct, у которой все поля readonly; компилятор запрещает изменять состояние после конструктора. Defensive copy — скрытая копия, которую компилятор делает при вызове метода на readonly-переменной обычной struct (например, `readonly`-поле или `in`-параметр): он не знает, изменит ли метод состояние, и защищает оригинал копией. Для `readonly struct` копия не нужна — меньше затрат и нет сюрпризов.
</details>

<details><summary><b>Tuple или record?</b></summary>

`ValueTuple` — для локальных, короткоживущих группировок: вернуть два значения из private-метода, swap, ключ в LINQ. Record — когда у данных есть смысл, имя, валидация, поведение, и они уходят в публичный API или сериализуются. Имена элементов tuple существуют только при компиляции, его поля изменяемы, а `(int, int)` в сигнатуре ничего не говорит о смысле.
</details>

<details><summary><b>Как правильно реализовать равенство для класса?</b></summary>

Реализовать `IEquatable<T>` с основной логикой, переопределить `Equals(object)` (делегирует), `GetHashCode` (на тех же полях, через `HashCode.Combine`), при необходимости `==`/`!=` с аккуратной обработкой null (`is null`). Сделать поля неизменяемыми и класс `sealed`, чтобы не было проблем с симметричностью у наследников. Альтернатива — использовать `record`.
</details>

## Дополнительно

- [Struct types (learn.microsoft.com)](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct), [Choosing between class and struct](https://learn.microsoft.com/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
- [Records](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
- [How to define value equality](https://learn.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type)
- [`Object.GetHashCode` — remarks](https://learn.microsoft.com/dotnet/api/system.object.gethashcode)
- [Tuple types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-tuples)
- Книги: Рихтер, «CLR via C#» — гл. 5 (value/reference types, boxing, равенство и хэш-коды); Скит, «C# in Depth» — главы о tuples и records.
