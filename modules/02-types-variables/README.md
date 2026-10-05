# Модуль 02. Типы данных и переменные

> **Зачем:** «Чем value type отличается от reference type?», «Что такое boxing?», «Почему string immutable?», «Почему деньги нельзя хранить в double?» — это, пожалуй, самые частые вопросы на собеседовании Junior .NET. В работе от понимания типов зависят баги с копированием данных, потерей копеек, переполнением счётчиков и «на моей машине работает, а у клиента запятая». **Время:** ~6–7 ч. **Нужно:** модуль 01.

## Что ты будешь уметь

- Объяснить разницу между value и reference types и предсказать поведение при присваивании и передаче в метод.
- Выбирать числовой тип (`int`/`long`/`double`/`decimal`) и обрабатывать переполнение (`checked`).
- Работать с `null`: nullable value types (`int?`) и nullable reference types (`string?`).
- Безопасно преобразовывать типы: implicit/explicit, `Parse`/`TryParse`, `Convert`, культура.
- Объяснить boxing/unboxing и найти его в коде.
- Понимать, почему `string` immutable, когда нужен `StringBuilder` и как правильно сравнивать строки.

---

## 1. Value types и reference types

Все типы в .NET делятся на две категории:

| | **Value type** (тип-значение) | **Reference type** (ссылочный тип) |
|---|---|---|
| Примеры | `int`, `double`, `bool`, `char`, `decimal`, `DateTime`, `enum`, любой `struct` | `string`, `object`, массивы, любой `class`, `record`, delegate |
| Переменная хранит | **само значение** | **ссылку** (адрес) на объект в heap |
| Присваивание `b = a` | **копирует значение** | **копирует ссылку** — обе переменные указывают на один объект |
| Может быть `null` | нет (только `int?`) | да |
| Наследуется от | `System.ValueType` → `object` | `object` |

```csharp
int a = 10;
int b = a;          // копия значения
b = 20;
Console.WriteLine(a);   // вывод: 10 — a не изменилась

int[] x = { 1, 2, 3 };
int[] y = x;        // копия ССЫЛКИ: x и y — один и тот же массив
y[0] = 99;
Console.WriteLine(x[0]); // вывод: 99 — изменили через y, видно через x
```

**Где это хранится (упрощённо, подробно — в модуле 12).** У каждого потока есть **stack** (стек — быстрая память для локальных переменных вызова метода, освобождается автоматически при выходе из метода). Объекты reference types живут в **managed heap** (куча — общая память, которую чистит Garbage Collector). Локальная переменная `int` лежит прямо в stack; локальная переменная `int[]` лежит в stack, но содержит лишь ссылку, а сам массив — в heap.

⚠️ Фраза «value types всегда живут в stack» — **миф**, который любят проверять на собеседовании. Value type хранится **там, где объявлен**: поле `int` внутри объекта класса живёт в heap вместе с объектом; элементы `int[]` — в heap внутри массива.

> **Почему так?** Value types — для маленьких, «атомарных» данных (число, дата, координата): их дёшево копировать, и они не нагружают GC — не нужно выделять память в heap и потом её собирать. Reference types — для сущностей с идентичностью и большим состоянием: копировать 1 МБ данных при каждом присваивании было бы безумием, а ссылка — всего 8 байт на 64-битной системе. Компромисс: при работе с reference types легко случайно изменить «чужой» объект через вторую ссылку.

> 💼 **На собеседовании:** «Value type хранит значение, при присваивании и передаче в метод копируется. Reference type хранит ссылку на объект в heap, при присваивании копируется ссылка, и обе переменные указывают на один объект. Value types хранятся там, где объявлены: локальные — обычно на стеке, поля классов — в heap».

---

## 2. Встроенные типы

| C# | .NET тип | Размер | Диапазон / точность |
|----|----------|--------|---------------------|
| `byte` / `sbyte` | `System.Byte` / `SByte` | 1 байт | 0..255 / −128..127 |
| `short` / `ushort` | `Int16` / `UInt16` | 2 | ±32 767 / 0..65 535 |
| `int` / `uint` | `Int32` / `UInt32` | 4 | ±2,1 млрд / 0..4,2 млрд |
| `long` / `ulong` | `Int64` / `UInt64` | 8 | ±9,2·10¹⁸ |
| `float` | `Single` | 4 | ~6–9 значащих цифр, двоичная дробь |
| `double` | `Double` | 8 | ~15–17 значащих цифр, двоичная дробь |
| `decimal` | `Decimal` | 16 | 28–29 значащих цифр, **десятичная** дробь |
| `bool` | `Boolean` | 1 | `true`/`false` |
| `char` | `Char` | 2 | один UTF-16 символ (code unit) |
| `string` | `String` | ссылка | неизменяемая последовательность `char` |
| `object` | `Object` | ссылка | базовый тип всех типов |

`int` — это просто **alias** (псевдоним) для `System.Int32`: `int x` и `Int32 x` — одно и то же.

```csharp
var million = 1_000_000;     // _ — разделитель для читаемости
var hex = 0xFF;              // 255
var bits = 0b1010;           // 10
var big = 5_000_000_000L;    // L — long
var price = 9.99m;           // m — decimal (без m это double и ошибка компиляции)
var ratio = 0.5f;            // f — float
char letter = 'A';           // одинарные кавычки — char, двойные — string
```

**Значения по умолчанию** (`default`): числа — `0`, `bool` — `false`, `char` — `'\0'`, reference types — `null`. Поля класса и элементы массива инициализируются ими автоматически, а **локальную переменную** нужно присвоить до чтения — иначе ошибка компиляции CS0165.

> **Почему так?** Фиксированные размеры (`int` — всегда 32 бита, на любой ОС и процессоре) — часть CTS: поведение программы не зависит от платформы, в отличие от C, где `int` бывает разным. А требование присвоить локальную переменную до использования — защита от целого класса багов «читаем мусор из памяти», характерных для C/C++.

---

## 3. Целые числа: деление, переполнение, `checked`

```csharp
Console.WriteLine(7 / 2);      // вывод: 3   — деление int на int целочисленное (отбрасывает дробь)
Console.WriteLine(-7 / 2);     // вывод: -3  — округление к нулю, а не вниз
Console.WriteLine(7 % 3);      // вывод: 1   — остаток
Console.WriteLine(7 / 2.0);    // вывод: 3.5 — один операнд double → вещественное деление

int max = int.MaxValue;
Console.WriteLine(max + 1);    // вывод: -2147483648 — переполнение «заворачивает» значение!

checked
{
    int boom = max + 1;        // OverflowException
}
int safe = checked(max + 1);   // то же для одного выражения
```

- По умолчанию арифметика **unchecked**: при переполнении старшие биты отбрасываются, исключения нет.
- **Константные** выражения проверяются компилятором: `int x = int.MaxValue + 1;` — ошибка компиляции CS0220.
- Целочисленное деление на ноль → `DivideByZeroException`.
- Включить проверку для всего проекта: `<CheckForOverflowUnderflow>true</CheckForOverflowUnderflow>` в `.csproj`.

> **Почему так?** Проверка переполнения на каждой операции стоит процессорного времени, а в 99% арифметики (индексы, счётчики в циклах) переполнения не бывает. Поэтому C# выбрал скорость по умолчанию и дал инструмент (`checked`) для мест, где переполнение — баг. Знаменитый пример — `(low + high) / 2` в бинарном поиске: этот баг прожил в стандартной библиотеке Java около 9 лет, пока массивы не стали достаточно большими.

> 💼 **На собеседовании:** «Что будет при `int.MaxValue + 1`?» — «По умолчанию unchecked: получим `int.MinValue`. В `checked`-контексте — `OverflowException`. Для констант — ошибка компиляции».

→ **Задача 2** (SafeMath, см. exercises.md)

---

## 4. `float`, `double`, `decimal` — и почему деньги только в `decimal`

```csharp
Console.WriteLine(0.1 + 0.2 == 0.3);     // вывод: False  (double)
Console.WriteLine(0.1 + 0.2);            // вывод: 0.30000000000000004
Console.WriteLine(0.1m + 0.2m == 0.3m);  // вывод: True   (decimal)

Console.WriteLine(1.0 / 0);              // вывод: ∞ (PositiveInfinity; символ зависит от культуры), исключения нет
Console.WriteLine(0.0 / 0);              // вывод: NaN
// Console.WriteLine(1m / 0);            // decimal: DivideByZeroException
```

- `float`/`double` — **двоичная** плавающая точка (стандарт IEEE 754). Число 0.1 в двоичной системе — бесконечная дробь (как 1/3 в десятичной), поэтому хранится приближённо. Быстро (аппаратная поддержка), огромный диапазон (до 10³⁰⁸).
- `decimal` — **десятичная** плавающая точка: 0.1 хранится точно. Медленнее (программная реализация, в ~10–20 раз), меньше диапазон (~7,9·10²⁸), 16 байт.
- `double` сравнивают с допуском: `Math.Abs(a - b) < 1e-9`, а не `==`.

**Округление.** `Math.Round(2.5)` → `2`, а `Math.Round(3.5)` → `4`: по умолчанию **banker's rounding** (банковское округление, `MidpointRounding.ToEven` — к ближайшему чётному). Для «школьного» — `Math.Round(x, 2, MidpointRounding.AwayFromZero)`.

| Задача | Тип |
|--------|-----|
| деньги, бухгалтерия, цены, налоги | `decimal` |
| физика, графика, научные расчёты, ML, координаты | `double` |
| экономия памяти в больших массивах, GPU, игры | `float` |

> **Почему так?** Процессоры аппаратно считают в двоичной плавающей точке — это быстро, и для физики погрешность в 15-м знаке не важна. Но в бухгалтерии ошибка в копейку недопустима: суммы должны сходиться до копейки, поэтому .NET даёт отдельный десятичный тип ценой скорости и памяти. Банковское округление по умолчанию — тоже не случайность: при массовом округлении «от нуля» сумма систематически смещается вверх, а «к чётному» ошибки взаимно гасятся.

> 💼 **На собеседовании:** «double — двоичная дробь, не может точно представить 0.1, поэтому `0.1 + 0.2 != 0.3`. decimal — десятичная, хранит такие числа точно; для денег — только decimal. Цена — скорость и память».

→ **Задача 3** (MoneyCalculator)

---

## 5. `var`, `const`, `readonly`

```csharp
var count = 10;                 // компилятор выводит тип int — это СТАТИЧЕСКАЯ типизация
var names = new List<string>(); // тип очевиден из правой части — var уместен
// count = "ten";               // ошибка компиляции: count — int навсегда

const double Pi = 3.14159;      // константа времени компиляции: только литералы/константные выражения
// const DateTime Now = DateTime.Now;   // ошибка: не константа

class Config
{
    public readonly DateTime CreatedAt = DateTime.Now; // присваивается при создании или в конструкторе, потом не меняется
}
```

- `var` — **не** динамическая типизация (это `dynamic`): тип выводится один раз при компиляции. Используйте, когда тип очевиден из правой части.
- `const` — значение вшивается компилятором прямо в место использования (даже в других сборках!). Только для примитивов, `string` и `enum`.
- `readonly` — поле, которое можно присвоить только при объявлении или в конструкторе; значение вычисляется во время выполнения. Подробно `const` vs `readonly` vs `static readonly` — в модуле 04.

> **Почему так?** `var` уменьшает шум (`Dictionary<string, List<int>> map = new Dictionary<string, List<int>>()`), не жертвуя типобезопасностью. А «вшивание» `const` в вызывающий код — ловушка версионирования: если библиотека поменяет `const MaxSize = 10` на `20`, а ваш проект не пересобран, он продолжит видеть `10`. Поэтому для значений, которые могут измениться, используют `static readonly`.

---

## 6. `null`: nullable value types и nullable reference types

**Nullable value types.** Value type не может быть `null`, но иногда «значения нет» — это нормальная ситуация (возраст не указан). Для этого есть `Nullable<T>`, коротко `T?`:

```csharp
int? age = null;                        // Nullable<int> — struct с полями HasValue и Value
Console.WriteLine(age.HasValue);        // вывод: False
Console.WriteLine(age ?? 18);           // вывод: 18 — оператор ?? (null-coalescing): «если null, то…»
age ??= 21;                             // присвоить, только если null
Console.WriteLine(age.Value);           // вывод: 21 (при null — InvalidOperationException)

if (age is int realAge)                 // pattern matching: проверка + распаковка в одной строке
{
    Console.WriteLine(realAge + 1);     // вывод: 22
}
```

**Nullable reference types** (NRT, C# 8+). Reference types *всегда* могли быть `null`, и `NullReferenceException` — самая частая ошибка в .NET. С включённым `<Nullable>enable</Nullable>` (в этом курсе он включён) компилятор **анализирует** код:

```csharp
string name = null;           // warning CS8600: null в ненулевой тип
string? nickname = null;      // OK: явно сказали «может быть null»

Console.WriteLine(nickname.Length);    // warning CS8602: возможное разыменование null
Console.WriteLine(nickname?.Length);   // ?. — null-conditional: если null, результат null
Console.WriteLine(nickname!.Length);   // ! — null-forgiving: «компилятор, поверь мне» (почти всегда плохая идея)

if (nickname is not null)
{
    Console.WriteLine(nickname.Length); // OK: компилятор понял, что здесь не null (flow analysis)
}
```

Важно: NRT — это **только анализ компилятора и предупреждения**. В runtime `string` и `string?` — один и тот же тип, проверки не добавляются. А `int?` — это действительно другой тип (`Nullable<int>`).

> **Почему так?** Тони Хоар, придумавший null, назвал его «ошибкой на миллиард долларов». Сделать reference types не-nullable в рантайме было нельзя — сломался бы весь существующий код. Поэтому C# 8 добавил аннотации и статический анализ: обратная совместимость сохранена, а большинство NRE ловятся при компиляции. Для value types `Nullable<T>` — реальный struct: ему нужно где-то хранить флаг `HasValue`.

> 💼 **На собеседовании:** «Чем `int?` отличается от `string?`» — «`int?` — это `Nullable<int>`, отдельный struct с `HasValue`/`Value`. `string?` — аннотация для компилятора: тип в рантайме тот же `string`, просто компилятор предупреждает о возможном null».

---

## 7. Преобразования типов: implicit, explicit, Parse, TryParse, Convert

```csharp
int i = 42;
long l = i;              // implicit (неявное): расширение, потерь нет
double d = i;            // implicit
int back = (int)l;       // explicit (явное, cast): возможна потеря данных
int cut = (int)3.99;     // вывод: 3 — дробная часть ОТБРАСЫВАЕТСЯ, а не округляется
int rounded = Convert.ToInt32(3.5);  // вывод: 4; Convert.ToInt32(2.5) → 2 (banker's rounding!)

int parsed = int.Parse("123");               // "abc" → FormatException, null → ArgumentNullException
int conv = Convert.ToInt32((string?)null);   // вывод: 0 — Convert трактует null как 0
bool ok = int.TryParse("abc", out var n);    // ok = false, n = 0 — без исключений
```

| Способ | Когда использовать | Плохой ввод |
|--------|-------------------|-------------|
| `(int)x` cast | числовые преобразования, приведение типов в иерархии | потеря данных молча / `InvalidCastException` |
| `int.Parse(s)` | строка **гарантированно** корректна (конфиг, который мы сами записали) | исключение |
| `int.TryParse(s, out v)` | **пользовательский ввод**, файлы, сеть | `false` |
| `Convert.ToInt32(x)` | универсальные преобразования из `object`, `null` → 0 | исключение |

**Культура.** `double.Parse("12.5")` на русской Windows бросит `FormatException` (в ru-RU разделитель дробной части — запятая), а на немецкой вернёт `125` (точка там — разделитель тысяч). Для машинных данных (файлы, API, конфиги) всегда указывайте `CultureInfo.InvariantCulture`:

```csharp
using System.Globalization;
var value = double.Parse("12.5", CultureInfo.InvariantCulture);       // 12.5 на любой машине
var text = value.ToString("F2", CultureInfo.InvariantCulture);        // "12.50"
```

> **Почему так?** Неявно разрешены только преобразования **без потери информации** — компилятор не даст случайно потерять данные; явный cast — это вы подписываетесь «я знаю, что делаю». Пара `Parse`/`TryParse` — общий паттерн .NET (Try-pattern): исключения — для ошибок программиста, а некорректный ввод пользователя — нормальная ситуация, которая не должна стоить броска исключения.

→ **Задача 1** (InputParser)

---

## 8. Boxing и unboxing

**Boxing** (упаковка) — превращение value type в `object` (или в интерфейс): CLR **выделяет в heap объект**, копирует туда значение и возвращает ссылку. **Unboxing** (распаковка) — обратное: проверка типа и копирование значения из heap обратно.

```csharp
int number = 42;
object boxed = number;      // boxing: аллокация в heap + копия
number = 100;
Console.WriteLine(boxed);   // вывод: 42 — в «коробке» независимая копия

int unboxed = (int)boxed;   // unboxing: проверка типа + копия
// long wrong = (long)boxed;  // InvalidCastException! распаковывать можно только в ТОЧНО тот же тип
long right = (int)boxed;    // OK: сначала unboxing в int, потом implicit int → long
```

Где boxing происходит незаметно:

```csharp
ArrayList list = new();     // старая необобщённая коллекция хранит object
list.Add(1);                // boxing!
var items = new List<int>(); 
items.Add(1);               // boxing нет: List<int> хранит int напрямую (generics, модуль 08)

object o = 5;               // boxing
IComparable c = 5;          // boxing: приведение struct к интерфейсу
string s = string.Format("{0}", 5);   // boxing аргумента (params object[])
```

В IL boxing виден как инструкция `box`, unboxing — `unbox.any` (проверьте на SharpLab).

> **Почему так?** Boxing — плата за **единую систему типов**: любое значение можно положить туда, где ждут `object` (до generics, появившихся в .NET 2.0, коллекции были только такими). Но каждая упаковка — аллокация в heap и работа для GC: в горячем цикле на миллион итераций это заметно. Поэтому generics (`List<int>`) и вытеснили `ArrayList`, а современный .NET старается избегать boxing даже при интерполяции строк.

> 💼 **На собеседовании:** «Boxing — упаковка value type в объект в heap при приведении к `object` или интерфейсу; unboxing — обратное извлечение с проверкой типа. Это аллокация и нагрузка на GC, поэтому его избегают с помощью generics. Распаковывать можно только в исходный тип, иначе `InvalidCastException`».

→ **Задача 6** (исследование boxing)

---

## 9. Строки: immutability, сравнение, интернирование, `char`

`string` — reference type, но ведёт себя «как значение»: **immutable** (неизменяемый). Ни один метод не меняет существующую строку — все возвращают новую.

```csharp
var s = "  Hello  ";
s.Trim();                         // результат выброшен! s не изменилась
Console.WriteLine($"[{s}]");      // вывод: [  Hello  ]
s = s.Trim();                     // правильно: присвоить новую строку
Console.WriteLine($"[{s}]");      // вывод: [Hello]
```

**Сравнение.** Для `string` оператор `==` перегружен и сравнивает **содержимое**:

```csharp
string a = "hello";
string b = new string("hello".ToCharArray());   // другая строка в памяти с тем же текстом
Console.WriteLine(a == b);                       // вывод: True  — сравнение значений
Console.WriteLine((object)a == (object)b);       // вывод: False — сравнение ССЫЛОК
Console.WriteLine(string.Equals("Admin", "admin", StringComparison.OrdinalIgnoreCase)); // вывод: True
```

| `StringComparison` | Когда |
|--------------------|-------|
| `Ordinal` | по кодам символов: идентификаторы, ключи, пути, протоколы — **по умолчанию для кода** |
| `OrdinalIgnoreCase` | то же без учёта регистра: логины, email, расширения файлов |
| `CurrentCulture(IgnoreCase)` | текст для **показа и сортировки пользователю** |
| `InvariantCulture` | редко: культурно-нейтральная лингвистическая сортировка |

`a.ToLower() == b.ToLower()` — антипаттерн: две лишние строки и зависимость от культуры (в турецкой культуре `"I".ToLower()` — это `"ı"`).

**String interning** (интернирование). Все строковые **литералы** в программе хранятся в пуле в единственном экземпляре: `(object)"abc" == (object)"abc"` → `True`. Строки, созданные в runtime (конкатенация, `ReadLine`), не интернируются автоматически (можно вручную: `string.Intern(s)`).

**Создание строк:**

```csharp
var name = "Anna";
var greeting = $"Hello, {name}! Today {DateTime.Today:yyyy-MM-dd}";  // интерполяция с форматом
var path = @"C:\Users\anna\file.txt";                                // verbatim: \ не экранирует
var json = """
    { "name": "Anna" }
    """;                                                             // raw string literal (C# 11)
var aligned = $"|{name,-10}|{42,5}|";                                // вывод: |Anna      |   42|
```

**`char`** — один UTF-16 code unit (2 байта). Полезные методы: `char.IsDigit`, `char.IsLetter`, `char.IsLetterOrDigit`, `char.IsWhiteSpace`, `char.ToUpperInvariant`. `char` — это число: `'7' - '0'` даёт `7`, `(int)'A'` — `65`. Эмодзи и редкие символы занимают **два** `char` (surrogate pair), поэтому `"😀".Length == 2`.

> **Почему так?** Immutability делает строки **потокобезопасными** (их можно читать из многих потоков без блокировок), позволяет **интернирование** (литералы можно разделять, раз их никто не изменит), делает строки **безопасными ключами** `Dictionary` (хэш не изменится после вставки) и защищает от изменений «за спиной» (передали имя файла в метод проверки прав — а его подменили). Цена — каждая «модификация» создаёт новую строку, поэтому для сборки строк в цикле есть `StringBuilder`.

> 💼 **На собеседовании:** «Почему string — reference type, но ведёт себя как value?» — «Он immutable, а `==` и `Equals` перегружены для сравнения содержимого. Immutability даёт потокобезопасность, интернирование и безопасность как ключа словаря».

→ **Задача 4** (StringTools)

---

## 10. `StringBuilder`

```csharp
using System.Text;

// Плохо: каждая итерация создаёт НОВУЮ строку и копирует в неё всё накопленное — O(n²)
var report = "";
for (var i = 0; i < 10_000; i++)
{
    report += i + ",";
}

// Хорошо: один растущий буфер, итоговая строка создаётся один раз — O(n)
var sb = new StringBuilder();
for (var i = 0; i < 10_000; i++)
{
    sb.Append(i).Append(',');       // методы возвращают тот же sb — можно чейнить
}
var result = sb.ToString();
```

Полезное: `Append`, `AppendLine` (добавляет `Environment.NewLine` — `"\r\n"` на Windows и `"\n"` на Linux!), `Insert`, `Remove`, `Replace`, `Clear`, `Length`, конструктор с начальной `capacity`.

Когда `StringBuilder` **не** нужен: склейка 2–5 строк в одном выражении (`a + b + c` компилятор превращает в один `string.Concat`), интерполяция, `string.Join` для коллекции.

> **Почему так?** Раз строки immutable, «дописать» к строке невозможно — только создать новую с копией старой. `StringBuilder` — **изменяемый** буфер, который растёт (обычно удваивается) по мере необходимости, поэтому n добавлений стоят O(n), а не O(n²). Правило: цикл с неизвестным числом склеек → `StringBuilder`.

→ **Задача 5** (ReceiptBuilder) · **Задача 7** ⭐ (Run-Length Encoding)

---

## Типичные ошибки

1. **Деньги в `double`** → «потерянные» копейки. Только `decimal`.
2. **`Math.Round` без `MidpointRounding`** → `Math.Round(2.5) == 2`, сюрприз для бизнеса.
3. **`s.Trim();` без присваивания** → строки immutable, результат выброшен.
4. **`int.Parse` на пользовательском вводе** → `FormatException`. Используйте `TryParse`.
5. **Парсинг и форматирование без культуры** → `"12,5"` вместо `"12.5"` на русской Windows.
6. **`(a + b) / 2` для больших int** → переполнение. `a + (b - a) / 2`.
7. **`7 / 2` ожидая 3.5** → целочисленное деление.
8. **`str1.ToLower() == str2.ToLower()`** → лишние аллокации и культурные баги; `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)`.
9. **`+=` строк в цикле** → O(n²); `StringBuilder`.
10. **Распаковка в «не тот» тип**: `(long)boxedInt` → `InvalidCastException`.
11. **`!` (null-forgiving) везде, чтобы убрать warnings** → NRE в runtime; разберитесь, откуда null.
12. **«Value types всегда в стеке»** — миф.

## Шпаргалка

```csharp
// Числа
checked(a + b);                                   // OverflowException при переполнении
long wide = (long)a + b;                          // считаем в более широком типе
Math.Round(x, 2, MidpointRounding.AwayFromZero);  // «школьное» округление
decimal price = 9.99m;                            // деньги

// Парсинг
int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n);
decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d);
x.ToString("0.00", CultureInfo.InvariantCulture);

// null
int? maybe = null;  maybe ?? 0;  maybe.HasValue;  maybe is int v;
string? s = null;   s?.Length;   s ?? "";         string.IsNullOrWhiteSpace(s);

// Строки
string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);   // по любым пробелам
string.Join(' ', words);
new StringBuilder().Append("a").Append('\n').ToString();
```

## Глоссарий

| Термин | Что значит |
|--------|-----------|
| Value type | тип-значение: переменная хранит само значение, копируется при присваивании |
| Reference type | ссылочный тип: переменная хранит ссылку на объект в heap |
| Stack | стек потока: быстрая память для локальных переменных, освобождается при выходе из метода |
| Managed heap | управляемая куча: память для объектов, очищается GC |
| Boxing / unboxing | упаковка value type в объект в heap / извлечение обратно |
| Immutable | неизменяемый: после создания состояние объекта не меняется |
| String interning | хранение одинаковых строковых литералов в единственном экземпляре |
| Nullable value type | `T?` = `Nullable<T>`: value type, который может быть `null` |
| Nullable reference types (NRT) | аннотации `string?` и статический анализ null компилятором |
| Null-coalescing `??` | «значение слева, если не null, иначе справа» |
| Null-conditional `?.` | обращение к члену, только если объект не null |
| Null-forgiving `!` | подавление nullable-предупреждения |
| Implicit / explicit conversion | неявное (без потерь) / явное (cast, возможны потери) преобразование |
| Overflow | переполнение: результат не помещается в тип |
| `checked` / `unchecked` | контекст с проверкой / без проверки переполнения |
| Banker's rounding | округление к ближайшему чётному (`MidpointRounding.ToEven`) |
| IEEE 754 | стандарт двоичной плавающей точки (`float`, `double`) |
| Culture | региональные настройки: разделитель дробей, формат дат (`CultureInfo`) |
| Ordinal comparison | сравнение строк по кодам символов, без учёта культуры |
| Alias | псевдоним типа: `int` = `System.Int32` |

## Вопросы на собеседовании

<details><summary><b>Чем value type отличается от reference type?</b></summary>

Value type хранит само значение; при присваивании и передаче в метод значение копируется, изменения копии не влияют на оригинал. Reference type хранит ссылку на объект в managed heap; при присваивании копируется ссылка, и две переменные указывают на один объект. Value types — `int`, `double`, `bool`, `struct`, `enum`; reference types — `class`, `string`, массивы, делегаты. Value types не могут быть `null` (кроме `Nullable<T>`).
</details>

<details><summary><b>Правда ли, что value types хранятся в стеке, а reference types — в куче?</b></summary>

Упрощённо — для локальных переменных да, но в общем случае нет. Value type хранится там, где объявлен: локальная переменная — обычно на стеке (или в регистре), поле класса — в heap внутри объекта, элементы массива — в heap. Захваченные лямбдой локальные переменные и переменные async-методов тоже оказываются в heap. Объекты reference types — всегда в heap, а ссылки на них могут лежать где угодно.
</details>

<details><summary><b>Что такое boxing и unboxing? Почему их стоит избегать?</b></summary>

Boxing — упаковка value type в объект в heap при приведении к `object` или интерфейсу; unboxing — проверка типа и копирование значения обратно. Boxing требует аллокации и создаёт нагрузку на GC, unboxing — проверку типа. Распаковать можно только в точно исходный тип, иначе `InvalidCastException`. Избегают с помощью generics: `List<int>` вместо `ArrayList`.
```csharp
object o = 42;          // box
int x = (int)o;         // unbox
// long y = (long)o;    // InvalidCastException
```
</details>

<details><summary><b>Почему string — immutable? Какие плюсы и минусы?</b></summary>

Плюсы: потокобезопасность без блокировок, возможность интернирования литералов, безопасное использование как ключа словаря (хэш не меняется), защита от изменений строки «за спиной» после проверки. Минус: каждая «модификация» создаёт новую строку — склейка в цикле даёт O(n²) и много мусора. Решение — `StringBuilder`.
</details>

<details><summary><b>Когда использовать StringBuilder, а когда обычную конкатенацию?</b></summary>

`StringBuilder` — когда строка собирается в цикле или из заранее неизвестного числа частей: он дописывает во внутренний растущий буфер, сложность O(n). Для 2–5 частей в одном выражении проще `+` или интерполяция: компилятор превратит их в один вызов `string.Concat`. Для коллекции строк — `string.Join`.
</details>

<details><summary><b>Почему нельзя хранить деньги в double? Что выбрать?</b></summary>

`double` — двоичная плавающая точка (IEEE 754), десятичные дроби вроде 0.1 в ней представимы только приближённо, поэтому `0.1 + 0.2 != 0.3`, и при суммировании копейки «плывут». `decimal` хранит число в десятичной системе (мантисса + степень десяти), поэтому 0.1 точно. Для денег — `decimal`; цена — он медленнее и занимает 16 байт. Ещё один вариант — хранить целые копейки в `long`.
</details>

<details><summary><b>Что будет при int.MaxValue + 1?</b></summary>

По умолчанию арифметика unchecked — значение «завернётся» в `int.MinValue` без исключения. В `checked`-контексте (выражение, блок или настройка проекта) будет `OverflowException`. Если выражение константное (`int.MaxValue + 1` литералами), компилятор выдаст ошибку CS0220. Unchecked по умолчанию — ради производительности.
</details>

<details><summary><b>Чем int? отличается от string? ?</b></summary>

`int?` — это `Nullable<int>`, отдельный struct с `HasValue` и `Value`, реальный тип в runtime. `string?` — аннотация nullable reference types: в runtime это тот же `string`, а `?` лишь сообщает компилятору, что null допустим, и включает статический анализ с предупреждениями. NRT не добавляют runtime-проверок.
</details>

<details><summary><b>Чем отличаются Parse, TryParse и Convert?</b></summary>

`Parse` бросает исключение при некорректной строке (`FormatException`, `OverflowException`, `ArgumentNullException` для null) — для гарантированно корректных данных. `TryParse` возвращает `bool` и результат через `out` — для пользовательского ввода, без дорогих исключений. `Convert.ToInt32` — универсальное преобразование из разных типов; для `null` возвращает 0, а `double` округляет банковски (`Convert.ToInt32(2.5) == 2`). Для машинных данных указывайте `CultureInfo.InvariantCulture`.
</details>

<details><summary><b>Что выведет код со сравнением строк?</b></summary>

```csharp
string a = "hello";
string b = "hel" + "lo";
string c = new string(new[] { 'h', 'e', 'l', 'l', 'o' });
Console.WriteLine(a == c);
Console.WriteLine((object)a == (object)b);
Console.WriteLine((object)a == (object)c);
```

`True`, `True`, `False`. `==` для `string` сравнивает содержимое. `"hel" + "lo"` — константное выражение, компилятор склеит его в литерал `"hello"`, а литералы интернируются — ссылка та же. `c` создана в runtime — это другой объект, поэтому сравнение ссылок через `object` даёт `False`.
</details>

<details><summary><b>Как правильно сравнивать строки без учёта регистра?</b></summary>

`string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` — для идентификаторов, логинов, ключей; это быстро и не зависит от культуры. Для текста, который сортируется и показывается пользователю, — `CurrentCultureIgnoreCase`. `a.ToLower() == b.ToLower()` — антипаттерн: две лишние аллокации и культурные сюрпризы (турецкая «i»).
</details>

<details><summary><b>Что такое var? Это динамическая типизация?</b></summary>

Нет. `var` — неявная, но статическая типизация: компилятор выводит тип из правой части один раз, и дальше переменная имеет этот тип навсегда. Динамическая типизация в C# — это `dynamic` (проверки в runtime). `var` уместен, когда тип очевиден из правой части (`new`, явный cast), и нужен для анонимных типов.
</details>

<details><summary><b>Что такое string interning?</b></summary>

CLR хранит строковые литералы в пуле интернирования в единственном экземпляре: одинаковые литералы в программе — один объект. Это экономит память и возможно только благодаря immutability. Строки, созданные в runtime, автоматически не интернируются; `string.Intern` делает это вручную, но используется редко. Полагаться на сравнение ссылок строк в бизнес-логике нельзя — сравнивайте значения.
</details>

## Дополнительно

- [Value types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-types) и [Reference types](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/reference-types)
- [Встроенные числовые типы](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/integral-numeric-types), [floating-point](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)
- [checked и unchecked](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/checked-and-unchecked)
- [Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references)
- [Boxing и unboxing](https://learn.microsoft.com/dotnet/csharp/programming-guide/types/boxing-and-unboxing)
- [Рекомендации по сравнению строк](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings)
- [StringBuilder](https://learn.microsoft.com/dotnet/standard/base-types/stringbuilder)
- Рихтер, «CLR via C#» — гл. 5 «Примитивные, ссылочные и значимые типы», гл. 14 «Символы, строки и обработка текста».
- Скит, «C# in Depth» — главы про nullable value types и nullable reference types.
- Эссе «What Every Computer Scientist Should Know About Floating-Point Arithmetic» (Goldberg) — для любопытных.
