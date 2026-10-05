# Модуль 11. LINQ

> **Зачем:** LINQ (Language Integrated Query — запросы, встроенные в язык) — то, чем .NET-разработчик пользуется каждый день: фильтрация, сортировка, группировка, отчёты, преобразование DTO, запросы к базе через EF Core. На собеседовании LINQ проверяют и вопросами («что такое deferred execution», «`IEnumerable` vs `IQueryable`», «`First` vs `Single`»), и задачами «напиши запрос: топ-3 покупателя по сумме заказов». **Время:** ~6–7 ч. **Нужно:** модули 08–10 (generics, коллекции и `yield`, делегаты и лямбды).

## Что ты будешь уметь

- Писать запросы в method syntax и query syntax и переводить одно в другое.
- Уверенно использовать основные операторы: `Where`, `Select`, `SelectMany`, `OrderBy/ThenBy`, `GroupBy`, `Join`, `Any/All`, `First/Single`, агрегаты, `Distinct`, `Skip/Take`, `ToDictionary/ToLookup`, `Aggregate`, `Zip`, `Chunk`.
- Объяснять deferred vs immediate execution и находить баги, вызванные ленивостью.
- Избегать multiple enumeration.
- Объяснять разницу `IEnumerable<T>` и `IQueryable<T>`.
- Писать свои операторы LINQ через `yield`.

---

## 1. Что такое LINQ

LINQ — набор **extension methods** для `IEnumerable<T>` (класс `System.Linq.Enumerable`) плюс синтаксис запросов в языке. Каждый оператор принимает последовательность и делегат и возвращает новую последовательность или значение.

```csharp
var orders = new List<Order> { /* ... */ };

// Императивно: как получить результат
var result = new List<string>();
foreach (var o in orders)
    if (o.Status == OrderStatus.Completed && o.Total > 100)
        result.Add(o.Customer);
result.Sort(StringComparer.Ordinal);

// Декларативно (LINQ): что хотим получить
var customers = orders
    .Where(o => o.Status == OrderStatus.Completed && o.Total > 100)
    .Select(o => o.Customer)
    .OrderBy(c => c, StringComparer.Ordinal)
    .ToList();
```

Ты уже писал такие операторы сам: `Filter`/`Map` в модуле 10 — это и есть `Where`/`Select`. Никакой магии: `yield return` + делегаты.

> **Почему так?** Декларативный код короче, его легче читать («отфильтруй, выбери, отсортируй») и сложнее ошибиться в индексах и временных списках. Тот же синтаксис работает с коллекциями в памяти, XML и базами данных (через `IQueryable`). Цена — накладные расходы (делегаты, итераторы, аллокации) и риск незаметных ошибок из-за ленивости. В горячих циклах иногда выгоднее `for`.

---

## 2. Method syntax и query syntax

```csharp
// Method syntax (fluent)
var q1 = orders
    .Where(o => o.Total > 100)
    .OrderByDescending(o => o.Total)
    .Select(o => new { o.Id, o.Customer });

// Query syntax — компилятор переводит его в те же вызовы методов
var q2 =
    from o in orders
    where o.Total > 100
    orderby o.Total descending
    select new { o.Id, o.Customer };
```

Query syntax удобен для `join`, `let` (промежуточная переменная) и вложенных `from`. Но многих операторов в нём нет (`Count`, `First`, `Distinct`, `Take`, `ToList`), поэтому на практике **чаще используют method syntax**, а синтаксисы можно смешивать: `(from o in orders where ... select o).Count()`.

```csharp
var bigOrders =
    from o in orders
    let total = o.Total            // вычислить один раз и переиспользовать
    where total >= 1000
    orderby total descending, o.Id // второй ключ — через запятую (= ThenBy)
    select o.Id;
```

> **Почему так?** Query syntax — синтаксический сахар, близкий к SQL, чтобы запросы читались естественно. Компилятор просто ищет методы `Where`, `Select`, `SelectMany`… по имени (паттерн, а не интерфейс) — поэтому query syntax работает и с `IQueryable`, и с любыми типами, у которых есть такие методы.

> 💼 **На собеседовании:** «Query syntax компилируется в вызовы методов, разницы в производительности нет. Я в основном пишу method syntax, query syntax — для join и сложных запросов с `let`».

---

## 3. Основные операторы

### Фильтрация и проекция

```csharp
var numbers = new[] { 1, 2, 3, 4, 5, 6 };

numbers.Where(n => n % 2 == 0);               // 2, 4, 6
numbers.Where((n, index) => index < 2);        // 1, 2 — перегрузка с индексом
numbers.Select(n => n * n);                    // 1, 4, 9, 16, 25, 36
numbers.Select((n, i) => $"{i}:{n}");          // "0:1", "1:2", ...
new object[] { 1, "a", 2 }.OfType<int>();      // 1, 2 — фильтр по типу
```

### `SelectMany` — «расплющить»

```csharp
// У каждого заказа — список строк. Нужен общий список строк всех заказов.
IEnumerable<IReadOnlyList<OrderLine>> nested = orders.Select(o => o.Lines);   // список списков
IEnumerable<OrderLine> flat = orders.SelectMany(o => o.Lines);                 // плоский список

var products = orders.SelectMany(o => o.Lines, (o, line) => $"{o.Id}: {line.Product}");
```

> **Почему так?** `Select` даёт ровно один результат на элемент, поэтому для «один-ко-многим» получается последовательность последовательностей. `SelectMany` склеивает их в одну — аналог вложенного `foreach`.

### Сортировка

```csharp
orders.OrderBy(o => o.Customer)
      .ThenByDescending(o => o.Total);   // второй ключ — ThenBy, НЕ ещё один OrderBy

// Ошибка: второй OrderBy пересортирует всё заново, первый ключ потеряется
orders.OrderBy(o => o.Customer).OrderBy(o => o.Total);
```

`OrderBy` — **стабильная** сортировка (равные элементы сохраняют исходный порядок), O(n log n). Для строк по умолчанию сравнение **culture-sensitive**; для детерминированного результата передавай `StringComparer.Ordinal`. В .NET 7+ есть `Order()`/`OrderDescending()` для сортировки самих элементов.

### Выбор одного элемента

| Оператор | Нет элементов | Больше одного |
|---|---|---|
| `First()` | `InvalidOperationException` | возвращает первый |
| `FirstOrDefault()` | `default` (null/0) | возвращает первый |
| `Single()` | `InvalidOperationException` | `InvalidOperationException` |
| `SingleOrDefault()` | `default` | `InvalidOperationException` |
| `Last()` / `LastOrDefault()` | как First, но с конца | — |
| `ElementAt(i)` | `ArgumentOutOfRangeException` | — |

```csharp
var user = users.Single(u => u.Email == email);          // email уникален — больше одного = баг данных
var latest = orders.OrderByDescending(o => o.Date).FirstOrDefault();   // может не быть
var safe = numbers.FirstOrDefault(n => n > 100, defaultValue: -1);      // .NET 6+: свой default
```

> **Почему так?** `Single` выражает инвариант «должен быть ровно один» и упадёт, если данные его нарушают, — это fail fast. `First` говорит «мне нужен любой первый». Выбор между ними — способ задокументировать ожидания прямо в коде. Минус `Single`: он обязан проверить, что второго нет, — то есть прочитать последовательность дальше первого совпадения.

### Квантификаторы: `Any`, `All`, `Contains`

```csharp
bool hasCancelled = orders.Any(o => o.Status == OrderStatus.Cancelled);  // остановится на первом
bool allPaid = orders.All(o => o.IsPaid);                               // остановится на первом false
bool notEmpty = orders.Any();                                           // лучше, чем Count() > 0
Console.WriteLine(new int[0].All(n => n > 100));                        // вывод: True (!)
```

`All` на пустой последовательности — `true` (vacuous truth — «все ноль элементов удовлетворяют условию»). Классический вопрос на собеседовании.

> **Почему так?** `Any()` останавливается на первом элементе — O(1) для проверки «не пусто». `Count() > 0` для ленивой последовательности перебирает всё — O(n). (Для `ICollection<T>` `Count()` использует свойство `Count`, но привычка `Any()` правильнее.)

### Агрегаты

```csharp
orders.Count();                      // количество
orders.Count(o => o.Total > 100);    // с условием
orders.Sum(o => o.Total);            // пусто → 0
orders.Average(o => o.Total);        // пусто → InvalidOperationException!
orders.Max(o => o.Total);            // пусто → InvalidOperationException (для non-nullable)
orders.MaxBy(o => o.Total);          // .NET 6+: сам ЭЛЕМЕНТ с максимумом, а не значение
orders.MinBy(o => o.Date);

// Aggregate — общий «свёрточный» оператор (это твой Reduce из модуля 10)
var csv = new[] { "a", "b", "c" }.Aggregate((acc, s) => acc + "," + s);   // "a,b,c"
var product = new[] { 1, 2, 3, 4 }.Aggregate(1, (acc, n) => acc * n);    // 24
```

### Множества и уникальность

```csharp
new[] { 1, 2, 2, 3 }.Distinct();                       // 1, 2, 3
orders.DistinctBy(o => o.Customer);                    // .NET 6+: первый заказ каждого покупателя
a.Union(b); a.Intersect(b); a.Except(b);               // операции над множествами
```

### Разбиение на страницы и пакеты

```csharp
int page = 3, pageSize = 20;
var pageItems = items.Skip((page - 1) * pageSize).Take(pageSize);   // пагинация
items.TakeWhile(x => x < 10); items.SkipWhile(x => x < 10);
foreach (int[] chunk in Enumerable.Range(1, 10).Chunk(4))           // .NET 6+
    Console.WriteLine(string.Join(",", chunk));
// вывод:
// 1,2,3,4
// 5,6,7,8
// 9,10
```

### `Zip` — «сшить» две последовательности

```csharp
var names = new[] { "Anna", "Boris" };
var scores = new[] { 90, 75, 60 };
foreach (var (name, score) in names.Zip(scores))     // останавливается на более короткой
    Console.WriteLine($"{name}: {score}");
// вывод:
// Anna: 90
// Boris: 75

var prices = new[] { 100m, 105m, 103m };
var changes = prices.Zip(prices.Skip(1), (prev, cur) => cur - prev);   // 5, -2
```

→ **Задача 1** (см. exercises.md) — отчёты по заказам.

---

## 4. Группировка: `GroupBy`, `ToLookup`, `ToDictionary`

```csharp
var byCustomer = orders.GroupBy(o => o.Customer);
foreach (IGrouping<string, Order> group in byCustomer)
    Console.WriteLine($"{group.Key}: {group.Count()} заказ(ов), {group.Sum(o => o.Total)}");

// Группа — это последовательность с ключом; сразу проецируем в отчёт
var report = orders
    .GroupBy(o => new { o.Date.Year, o.Date.Month })      // составной ключ — анонимный тип
    .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(o => o.Total) })
    .OrderBy(x => x.Year).ThenBy(x => x.Month);
```

| | `GroupBy` | `ToLookup` | `ToDictionary` |
|---|---|---|---|
| Выполнение | отложенное | немедленное | немедленное |
| Значение по ключу | группа | последовательность | одно значение |
| Дубликат ключа | — | в одну группу | `ArgumentException` |
| Нет ключа | — | пустая последовательность | `KeyNotFoundException` |

```csharp
ILookup<string, Order> lookup = orders.ToLookup(o => o.Customer);
Console.WriteLine(lookup["Nobody"].Count());   // вывод: 0 — без исключения

Dictionary<int, Order> byId = orders.ToDictionary(o => o.Id);   // Id уникален
// orders.ToDictionary(o => o.Customer);   // ArgumentException, если у покупателя 2 заказа
```

> **Почему так?** Анонимный тип в ключе `GroupBy` работает, потому что компилятор генерирует для него `Equals`/`GetHashCode` по значениям всех свойств (модуль 06). `ToLookup` — «словарь списков» для многократного доступа по ключу; `GroupBy` — для однократного прохода по группам в составе запроса.

→ **Задача 2** — аналитика продаж (группировки и агрегаты).

---

## 5. Соединения: `Join`, `GroupJoin`, left join

```csharp
// Inner join: только сотрудники с существующим отделом
var pairs = employees.Join(
    departments,
    e => e.DepartmentId,          // ключ внешней последовательности
    d => (int?)d.Id,              // ключ внутренней (типы ключей должны совпадать)
    (e, d) => new { e.Name, Department = d.Name });

// То же в query syntax
var pairs2 =
    from e in employees
    join d in departments on e.DepartmentId equals (int?)d.Id
    select new { e.Name, Department = d.Name };
```

`GroupJoin` — для каждого элемента внешней последовательности даёт **группу** совпадений (возможно, пустую). Это основа **left join**:

```csharp
// Все сотрудники; отдел — null, если не найден
var leftJoin =
    from e in employees
    join d in departments on e.DepartmentId equals (int?)d.Id into matches
    from d in matches.DefaultIfEmpty()             // пустая группа → один элемент null
    select new { e.Name, Department = d?.Name ?? "(нет)" };

// Каждый отдел с количеством сотрудников, включая пустые отделы
var stats = departments.GroupJoin(
    employees,
    d => (int?)d.Id,
    e => e.DepartmentId,
    (d, staff) => new { d.Name, Count = staff.Count() });
```

В .NET 10 добавили готовые операторы `LeftJoin` и `RightJoin`, но форма `GroupJoin` + `DefaultIfEmpty` встречается во всём существующем коде и на собеседованиях.

> **Почему так?** `Join` внутри строит хэш-таблицу по ключам внутренней последовательности, поэтому работает за O(n + m). Наивный вариант — `employees.Select(e => departments.FirstOrDefault(d => d.Id == e.DepartmentId))` — O(n × m): на 10 000 × 1 000 это 10 млн сравнений.

→ **Задача 3** — запросы по сотрудникам и отделам.

---

## 6. Deferred vs immediate execution

**Самая важная тема модуля.** Большинство операторов LINQ (`Where`, `Select`, `OrderBy`, `GroupBy`, `Skip`, `Take`…) **не выполняют** запрос — они возвращают объект-«рецепт». Выполнение происходит при переборе: `foreach`, `ToList()`, `Count()`, `First()` и т.п.

```csharp
var numbers = new List<int> { 1, 2, 3 };
var query = numbers.Where(n => n > 1);    // ничего не выполнено

numbers.Add(4);                            // меняем источник ПОСЛЕ создания запроса
Console.WriteLine(string.Join(",", query));   // вывод: 2,3,4 — запрос видит текущие данные

var snapshot = numbers.Where(n => n > 1).ToList();   // выполнено СЕЙЧАС
numbers.Add(5);
Console.WriteLine(snapshot.Count);         // вывод: 3 — снимок не меняется
```

| Отложенное (deferred) | Немедленное (immediate) |
|---|---|
| `Where`, `Select`, `SelectMany`, `OrderBy`, `ThenBy`, `GroupBy`, `Join`, `Distinct`, `Skip`, `Take`, `Zip`, `Chunk`, `Concat`, `Reverse` | `ToList`, `ToArray`, `ToDictionary`, `ToLookup`, `ToHashSet`, `Count`, `Sum`, `Average`, `Min`, `Max`, `First`, `Single`, `Any`, `All`, `Contains`, `Aggregate` |

Ещё тонкость: отложенные операторы делятся на **streaming** (`Where`, `Select` — отдают элемент сразу, как обработали) и **non-streaming** (`OrderBy`, `GroupBy` — при первом `MoveNext` читают **весь** источник, ведь чтобы отдать минимальный элемент, надо увидеть все).

Ловушки ленивости:

```csharp
// 1. Захваченная переменная меняется до выполнения
int threshold = 10;
var expensive = products.Where(p => p.Price > threshold);
threshold = 1000;
var list = expensive.ToList();     // фильтрует по 1000, а не по 10!

// 2. Исключение возникает не там, где написан запрос
var parsed = lines.Select(int.Parse);     // ок
// ... далеко отсюда:
foreach (var n in parsed) { }             // FormatException — здесь

// 3. Изменение источника во время перебора
foreach (var item in items.Where(i => i.Quantity == 0))
    items.Remove(item);                   // InvalidOperationException: Collection was modified

// 4. Возврат «живого» запроса из метода, который обещает снимок
public IEnumerable<Item> GetSnapshot() => _items.Where(...);   // это не снимок!
```

> **Почему так?** Ленивость даёт композицию без промежуточных коллекций (`Where` → `Select` → `Take(10)` обработает ровно столько элементов, сколько нужно), работу с бесконечными и огромными последовательностями и — для `IQueryable` — возможность собрать весь запрос и отправить в БД одним SQL. Плата — выполнение «когда-то потом» с теми данными, которые будут тогда.

> 💼 **На собеседовании:** «Deferred execution: операторы вроде `Where`/`Select` возвращают запрос, который выполняется при переборе — каждый раз заново и с актуальными данными. Immediate: `ToList`, `Count`, `First`, агрегаты выполняются сразу. Если нужен снимок или запрос будет перебираться несколько раз — материализую через `ToList()`».

→ **Задача 4** — найди и исправь баги ленивого выполнения в `Inventory`.

---

## 7. Multiple enumeration

Каждое перечисление ленивого запроса выполняет его **заново**:

```csharp
IEnumerable<Order> bigOrders = orders.Where(o => IsBig(o));   // IsBig — дорогой вызов

if (bigOrders.Any())                       // проход 1 (до первого совпадения)
{
    Console.WriteLine(bigOrders.Count());  // проход 2 — полный
    foreach (var o in bigOrders) { }       // проход 3 — полный
}
```

Последствия: лишняя работа (×3 вызовов `IsBig`), повторные запросы к БД (если это `IQueryable`), **разные результаты** между проходами (если источник изменился), падение на одноразовых источниках (поток из файла, `DataReader`, `IAsyncEnumerable` из сети). Rider и анализаторы подсвечивают это предупреждением «Possible multiple enumeration».

Решение — материализовать один раз:

```csharp
List<Order> bigOrders = orders.Where(o => IsBig(o)).ToList();
if (bigOrders.Count > 0) { /* ... */ }
```

Но не впадай в крайность: `ToList()` «на всякий случай» после каждого оператора — лишние аллокации. Материализуй, когда результат перебирается больше одного раза, нужен снимок или его надо отдать наружу.

> **Почему так?** `IEnumerable<T>` — это обещание «могу перечислить», а не «уже лежит в памяти». Метод, принимающий `IEnumerable<T>`, не знает, что за ним: массив или запрос к БД. Поэтому хорошая практика — перебирать параметр-`IEnumerable` один раз или материализовать его в начале метода.

---

## 8. `IEnumerable<T>` vs `IQueryable<T>`

```csharp
IEnumerable<Order> inMemory = list.Where(o => o.Total > 100);     // Func<Order, bool> — код
IQueryable<Order> inDb = db.Orders.Where(o => o.Total > 100);      // Expression<Func<Order, bool>> — данные
```

| | `IEnumerable<T>` (LINQ to Objects) | `IQueryable<T>` (LINQ to Entities и др.) |
|---|---|---|
| Где выполняется | в памяти, в твоём процессе | у провайдера: обычно БД (SQL) |
| Лямбды становятся | делегатами (`Func<…>`) — скомпилированный код | **expression trees** (`Expression<Func<…>>`) — дерево, которое можно разобрать |
| Методы | `System.Linq.Enumerable` | `System.Linq.Queryable` |
| Что можно в лямбде | любой C#-код | только то, что провайдер умеет перевести в SQL |

Главная ловушка — незаметный переход из `IQueryable` в `IEnumerable`:

```csharp
// Плохо: AsEnumerable/ToList слишком рано — из БД загружается ВСЯ таблица, фильтр в памяти
var bad = db.Orders.ToList().Where(o => o.Total > 100);

// Плохо: параметр IEnumerable — Where уже вызовет Enumerable.Where, фильтр в памяти
IEnumerable<Order> source = db.Orders;
var alsoBad = source.Where(o => o.Total > 100);

// Хорошо: фильтр, сортировка и Take попадут в SQL (WHERE / ORDER BY / TOP), из БД придут 10 строк
var good = db.Orders.Where(o => o.Total > 100).OrderBy(o => o.Date).Take(10).ToList();
```

Подробно — в модуле 16 (EF Core), там ты увидишь сгенерированный SQL.

> **Почему так?** Чтобы отправить запрос в БД, провайдеру нужно **прочитать** лямбду как данные («сравнить свойство Total с константой 100») и перевести в SQL. Скомпилированный делегат — «чёрный ящик», его можно только вызвать. Поэтому `Queryable`-методы принимают `Expression<Func<…>>`, и компилятор строит дерево выражения вместо кода.

> 💼 **На собеседовании:** «`IEnumerable` выполняет запрос в памяти, лямбды — это делегаты. `IQueryable` хранит expression tree, который провайдер (EF Core) переводит в SQL и выполняет в БД. Важно не материализовать запрос (`ToList`, `AsEnumerable`) до фильтрации, иначе из базы загрузится вся таблица».

---

## 9. Производительность LINQ

- Каждый оператор — итератор + вызов делегата на элемент. В обычном бизнес-коде это незаметно; в горячих циклах (миллионы итераций в секунду) `for` по массиву быстрее.
- В .NET 7–10 LINQ серьёзно оптимизирован (векторизованные `Sum`/`Min`/`Max` для массивов, специализации для `List`/массивов), поэтому «LINQ всегда медленный» — устаревший миф.
- Порядок операторов важен: сначала `Where`, потом `OrderBy` — сортируется меньше элементов.
- `Any()` вместо `Count() > 0`; `TryGetValue`/`HashSet` вместо `Where(...).Any()` в цикле (O(n²)).
- `Contains` по `List` внутри `Where` — O(n × m); положи значения в `HashSet`.

```csharp
// O(n × m)
var vipOrders = orders.Where(o => vipCustomers.Contains(o.Customer));   // vipCustomers — List<string>
// O(n + m)
var vipSet = vipCustomers.ToHashSet();
var vipOrders2 = orders.Where(o => vipSet.Contains(o.Customer));
```

> **Почему так?** LINQ скрывает циклы, но не отменяет их. Сложность запроса — сумма сложностей операторов и делегатов внутри. Читаемость LINQ — повод писать его по умолчанию, а профилировщик — повод переписать горячее место.

---

## 10. Свои операторы

Оператор LINQ — это extension method над `IEnumerable<T>` с `yield return` и жадной проверкой аргументов (модули 09–10):

```csharp
public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
{
    ArgumentNullException.ThrowIfNull(source);
    return Iterator(source);

    static IEnumerable<T> Iterator(IEnumerable<T?> items)
    {
        foreach (T? item in items)
            if (item is not null) yield return item;
    }
}
```

Хороший оператор: ленивый, однопроходный, не материализует источник без необходимости, работает с бесконечными последовательностями, валидирует аргументы сразу. Готовые коллекции дополнительных операторов — библиотека MoreLINQ.

→ **Задача 5** (🧪 исследование) — query syntax, expression trees, multiple enumeration своими глазами.
→ **Задача 6** ⭐ — свои `Batch`, `DistinctByKey`, `Pairwise`.

---

## Типичные ошибки

1. **Возврат «живого» запроса** вместо снимка — данные меняются после возврата из метода.
2. **Multiple enumeration** — `Count()`, затем `foreach`, затем `ToList()` по одному ленивому запросу.
3. **`OrderBy(...).OrderBy(...)`** вместо `ThenBy`.
4. **`First()` там, где элемента может не быть** (`InvalidOperationException`), или `FirstOrDefault()` без проверки на `null`.
5. **`Single` vs `First`** — использование `First`, когда нарушение уникальности должно быть ошибкой.
6. **`Average`/`Max` на пустой последовательности** — исключение.
7. **`Count() > 0` вместо `Any()`**.
8. **Изменение коллекции во время перебора запроса над ней.**
9. **`ToList()` на `IQueryable` до фильтрации** — загрузка всей таблицы.
10. **Побочные эффекты в `Select`/`Where`** (изменение состояния, логирование) — выполняются неизвестно когда и сколько раз.
11. **`Contains` по `List` внутри `Where`** — O(n²).
12. **Сортировка строк без компаратора**, когда нужен детерминированный (ordinal) порядок.

---

## Шпаргалка

```csharp
// Фильтр/проекция
Where(x => ...)  Select(x => ...)  SelectMany(x => x.Items)  OfType<T>()  Cast<T>()
// Сортировка
OrderBy / OrderByDescending → ThenBy / ThenByDescending     Order()  (.NET 7+)
// Один элемент
First / FirstOrDefault / Single / SingleOrDefault / Last / ElementAt
// Квантификаторы (останавливаются рано)
Any()  Any(pred)  All(pred) /* пусто → true */  Contains(x)
// Агрегаты
Count  Sum /* пусто → 0 */  Average/Min/Max /* пусто → исключение */  MinBy/MaxBy  Aggregate
// Множества
Distinct  DistinctBy  Union  Intersect  Except
// Разбиение
Skip  Take  SkipWhile  TakeWhile  Chunk(n)
// Группировка/соединения
GroupBy(k)  ToLookup(k)  ToDictionary(k, v)  Join  GroupJoin + DefaultIfEmpty (left join)  Zip
// Материализация (immediate)
ToList  ToArray  ToHashSet  ToDictionary  ToLookup

// Deferred: запрос выполняется при переборе, каждый раз заново.
// IEnumerable — делегаты, в памяти; IQueryable — expression tree, в БД.
```

---

## Глоссарий

| Термин | Что значит |
|---|---|
| LINQ | Language Integrated Query — запросы к данным средствами языка |
| Method / query syntax | Запись через вызовы методов / SQL-подобная запись `from … where … select` |
| Projection | Проекция — преобразование элемента (`Select`) |
| Deferred execution | Отложенное выполнение — запрос выполняется при переборе |
| Immediate execution | Немедленное выполнение (`ToList`, `Count`, `First`…) |
| Streaming / non-streaming | Оператор отдаёт элементы по одному / сначала читает весь источник (`OrderBy`) |
| Materialization | Материализация — выполнение запроса с сохранением результата в коллекцию |
| Multiple enumeration | Повторное перечисление ленивого запроса — повторное выполнение |
| `IGrouping<TKey, T>` | Группа элементов с общим ключом |
| `ILookup<TKey, T>` | «Словарь последовательностей», вычисляется сразу |
| Left join | Соединение, сохраняющее все элементы левой последовательности |
| `IQueryable<T>` | Запрос, выполняемый провайдером (обычно БД) |
| Expression tree | Дерево выражения — код, представленный как данные |
| Vacuous truth | `All` на пустой последовательности — `true` |

---

## Вопросы на собеседовании

<details><summary><b>Что такое deferred execution?</b></summary>

Большинство операторов LINQ (`Where`, `Select`, `OrderBy`, `GroupBy`…) не выполняют запрос, а возвращают объект, описывающий его. Запрос выполняется при переборе (`foreach`, `ToList`, `Count`, `First`), причём каждый раз заново и с текущими данными источника. Немедленно выполняются операторы, возвращающие не последовательность: агрегаты, `First`, `Any`, а также `ToList`/`ToArray`/`ToDictionary`/`ToLookup`.
</details>

<details><summary><b>Чем IEnumerable отличается от IQueryable?</b></summary>

`IEnumerable<T>` — LINQ to Objects: выполнение в памяти, лямбды компилируются в делегаты. `IQueryable<T>` — запрос к провайдеру: лямбды превращаются в expression trees, которые провайдер (EF Core) переводит в SQL и выполняет в БД. Если преждевременно перейти к `IEnumerable` (`ToList`, `AsEnumerable`, параметр типа `IEnumerable`), фильтрация произойдёт в памяти после загрузки всех данных.
</details>

<details><summary><b>First, FirstOrDefault, Single, SingleOrDefault — в чём разница?</b></summary>

`First` — первый элемент, исключение, если пусто. `FirstOrDefault` — `default`, если пусто. `Single` — ровно один элемент, исключение, если ноль или больше одного. `SingleOrDefault` — `default`, если пусто, исключение, если больше одного. `Single` выражает инвариант уникальности (например, поиск по уникальному email), но должен проверить отсутствие второго элемента.
</details>

<details><summary><b>Что такое multiple enumeration и чем она опасна?</b></summary>

Повторный перебор одного ленивого запроса: каждый раз запрос выполняется заново. Опасна лишней работой (повторные вычисления, запросы к БД), разными результатами между перечислениями, если данные изменились, и падением на одноразовых источниках (потоки, DataReader). Решение — материализовать результат один раз через `ToList()`/`ToArray()`.
</details>

<details><summary><b>Чем GroupBy отличается от ToLookup?</b></summary>

`GroupBy` — отложенный, возвращает `IEnumerable<IGrouping<K, T>>` и выполняется при переборе (и заново при каждом). `ToLookup` — немедленный, возвращает `ILookup<K, T>` с быстрым доступом по ключу; для отсутствующего ключа — пустая последовательность, без исключения. `ToLookup` удобен для многократных обращений по ключу.
</details>

<details><summary><b>Зачем SelectMany?</b></summary>

Для «расплющивания» вложенных коллекций: `orders.SelectMany(o => o.Lines)` даёт плоскую последовательность строк всех заказов, тогда как `Select` вернул бы последовательность списков. Аналог вложенного `foreach` или `from … from …` в query syntax; используется и для декартова произведения, и для left join.
</details>

<details><summary><b>Почему Any() лучше, чем Count() > 0?</b></summary>

`Any()` останавливается на первом элементе — O(1). `Count()` для ленивой последовательности перебирает все элементы — O(n), а для `IQueryable` генерирует `COUNT(*)` вместо `EXISTS`. Для коллекций с `Count` разница невелика, но `Any()` точнее выражает намерение.
</details>

<details><summary><b>Как сделать left join в LINQ?</b></summary>

`GroupJoin` + `SelectMany` + `DefaultIfEmpty`: в query syntax — `join d in departments on e.DeptId equals d.Id into g from d in g.DefaultIfEmpty()`. Для элементов без пары `d` будет `null`. В .NET 10 появился готовый оператор `LeftJoin`, но классическую форму нужно знать.
</details>

<details><summary><b>Что выведет код?</b></summary>

```csharp
var list = new List<int> { 1, 2, 3 };
var query = list.Select(x => x * 10);
list.Add(4);
Console.WriteLine(query.Count());
```
`4`. Запрос отложенный: `Select` выполнится при вызове `Count()`, когда в списке уже 4 элемента.
</details>

<details><summary><b>Что вернёт All для пустой последовательности?</b></summary>

`true`. Утверждение «все элементы пустого множества удовлетворяют условию» истинно (vacuous truth): нет ни одного контрпримера. `Any(predicate)` для пустой — `false`. Частая ошибка — проверять «все заказы оплачены» через `All` без проверки, что заказы вообще есть.
</details>

<details><summary><b>Есть ли разница в производительности между query и method syntax?</b></summary>

Нет: query syntax компилируется в те же вызовы методов. Выбор — вопрос читаемости: query syntax удобен для `join`, `let` и нескольких `from`, method syntax — для всего остального, и в нём доступны все операторы.
</details>

<details><summary><b>Как написать свой оператор LINQ?</b></summary>

Extension method над `IEnumerable<T>`: внешний метод проверяет аргументы и возвращает результат внутреннего итератора с `yield return` (локальная функция). Так оператор будет ленивым, однопроходным и бросит `ArgumentNullException` сразу при вызове, а не при переборе.
</details>

<details><summary><b>Почему не стоит делать побочные эффекты внутри Select/Where?</b></summary>

Из-за отложенного выполнения неизвестно, когда и сколько раз выполнится делегат: при повторном переборе — снова, при `Take(1)` — не для всех элементов, при `IQueryable` — вообще не выполнится (переведётся в SQL или упадёт). Побочные эффекты делают в `foreach` по материализованному результату.
</details>

---

## Дополнительно

- [LINQ — learn.microsoft.com](https://learn.microsoft.com/dotnet/csharp/linq/)
- [Standard query operators overview](https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/)
- [Классификация операторов: deferred/immediate, streaming/non-streaming](https://learn.microsoft.com/dotnet/csharp/linq/get-started/introduction-to-linq-queries#classification-of-standard-query-operators-by-manner-of-execution)
- [Expression trees](https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/)
- Джон Скит, серия статей «Edulinq» — реализация всего LINQ to Objects с нуля с объяснениями.
- Джон Скит, «C# in Depth» — главы про LINQ, query expressions и expression trees.
