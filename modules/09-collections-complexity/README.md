# Модуль 09. Коллекции и сложность алгоритмов

> **Зачем:** в любом реальном проекте ты каждый день выбираешь коллекцию: список заказов, словарь пользователей по id, множество уже обработанных сообщений, очередь задач. Неудачный выбор превращает O(1) в O(n), и сервис «ложится» на 100 000 записей. На собеседовании это одна из главных тем: «как устроен `Dictionary`», «чем `List` отличается от `LinkedList`», «какая сложность у…», «зачем `GetHashCode`», плюс алгоритмические задачи вроде Two Sum и LRU-кэша. **Время:** ~6–7 ч. **Нужно:** модули 06 (Equals/GetHashCode) и 08 (generics).

## Что ты будешь уметь

- Оценивать сложность кода в нотации Big-O и объяснять её.
- Объяснять, как устроены `List<T>`, `Dictionary<TKey, TValue>`, `HashSet<T>`, `LinkedList<T>`.
- Выбирать коллекцию под задачу: `Queue`, `Stack`, `PriorityQueue`, `SortedDictionary`, `SortedSet`.
- Ориентироваться в иерархии интерфейсов `IEnumerable` → `ICollection` → `IList`/`IDictionary` и `IReadOnly*`.
- Писать свои итераторы через `yield return` и понимать их ленивость.
- Решать типовые задачи: частота слов, удаление дубликатов, Two Sum, LRU-кэш.

---

## 1. Big-O: как оценивать сложность

**Big-O** (асимптотическая сложность) показывает, как растёт время (или память) работы алгоритма при росте размера входных данных `n`. Константы и младшие слагаемые отбрасываются: важна «форма роста».

| Сложность | Название | Пример | n = 1 000 000 |
|---|---|---|---|
| O(1) | константная | `array[i]`, `dict[key]` | 1 операция |
| O(log n) | логарифмическая | бинарный поиск, `SortedSet.Add` | ~20 |
| O(n) | линейная | один проход по массиву, `List.Contains` | 10⁶ |
| O(n log n) | линейно-логарифмическая | `Array.Sort`, `OrderBy` | ~2·10⁷ |
| O(n²) | квадратичная | вложенные циклы по одному массиву | 10¹² — часы |

```csharp
// O(n²): для каждого элемента — проход по списку результатов
var unique = new List<int>();
foreach (int x in numbers)
    if (!unique.Contains(x))   // Contains — O(n)
        unique.Add(x);

// O(n): проверка в HashSet — O(1) в среднем
var seen = new HashSet<int>();
var result = new List<int>();
foreach (int x in numbers)
    if (seen.Add(x))           // Add возвращает false, если элемент уже был
        result.Add(x);
```

Важные оговорки:
- **Средний и худший случай** различаются: `Dictionary` — O(1) в среднем, но O(n) в худшем (все ключи в одном бакете).
- **Амортизированная сложность**: `List.Add` обычно O(1), но иногда O(n) (расширение массива); «в среднем по многим операциям» — O(1).
- Сложность по **памяти** тоже считается: Two Sum за O(n) по времени требует O(n) дополнительной памяти.

> **Почему так?** Big-O отвечает на главный практический вопрос: «что будет, когда данных станет в 100 раз больше?». O(n) станет медленнее в 100 раз, O(n²) — в 10 000 раз. Константы зависят от железа и компилятора, а форма роста — нет, поэтому сравнивают именно её.

> 💼 **На собеседовании:** после любой алгоритмической задачи тебя спросят «какая сложность по времени и памяти?». Называй обе и объясняй: «один проход — O(n), внутри поиск в HashSet за O(1), итого O(n) по времени и O(n) по памяти на множество».

---

## 2. Массив и `List<T>`

**Массив** `T[]` — непрерывный блок памяти фиксированного размера. Доступ по индексу — O(1): адрес = начало + индекс × размер элемента.

**`List<T>`** — обёртка над массивом, которая умеет расти:
- `Count` — сколько элементов реально хранится;
- `Capacity` — размер внутреннего массива.

Когда `Count == Capacity`, `Add` создаёт новый массив **вдвое больше** и копирует элементы.

```csharp
var list = new List<int>();
for (int i = 0; i < 9; i++)
{
    list.Add(i);
    Console.Write($"{list.Capacity} ");
}
// вывод: 4 4 4 4 8 8 8 8 16
```

| Операция `List<T>` | Сложность |
|---|---|
| `list[i]`, `list[i] = x` | O(1) |
| `Add` в конец | O(1) амортизированно |
| `Insert(0, x)`, `RemoveAt(0)` | O(n) — сдвиг всех элементов |
| `Contains`, `IndexOf`, `Remove(x)` | O(n) — линейный поиск |
| `Sort` | O(n log n) |
| `BinarySearch` (на отсортированном) | O(log n) |

```csharp
var orders = new List<Order>(capacity: 10_000); // размер известен — избегаем перевыделений
```

> **Почему так?** Удвоение — компромисс между памятью и скоростью. При росте «на 1» каждое `Add` копировало бы весь массив (O(n²) на n добавлений). При удвоении суммарно копируется не больше 2n элементов — O(1) на операцию в среднем. Платим за это неиспользованной памятью (до половины массива). Если размер известен заранее — передай capacity.

> 💼 **На собеседовании:** «`List<T>` — динамический массив. Доступ по индексу O(1), добавление в конец — амортизированно O(1) за счёт удвоения capacity, вставка и удаление в начале — O(n), поиск по значению — O(n)».

---

## 3. `LinkedList<T>` — двусвязный список

Каждый элемент — отдельный объект-узел (`LinkedListNode<T>`) со ссылками `Previous` и `Next`.

```csharp
var list = new LinkedList<string>();
LinkedListNode<string> b = list.AddLast("b");
list.AddFirst("a");
list.AddAfter(b, "c");
list.Remove(b);                        // O(1) — узел уже на руках
Console.WriteLine(string.Join(",", list));  // вывод: a,c
```

| Операция | `List<T>` | `LinkedList<T>` |
|---|---|---|
| Доступ по индексу | O(1) | O(n) — нет индексатора |
| Вставка/удаление в начале | O(n) | O(1) |
| Вставка/удаление по известному узлу | O(n) | O(1) |
| Поиск по значению | O(n) | O(n) |
| Память на элемент | только значение | значение + 2 ссылки + заголовок объекта |

На практике `LinkedList` используют редко: узлы разбросаны по куче, процессорный кэш работает плохо, и `List<T>` почти всегда быстрее даже там, где теоретически проигрывает. Классическое применение — **LRU-кэш** (в паре со словарём, который хранит ссылки на узлы).

> **Почему так?** Big-O не учитывает cache locality (локальность данных): массив читается из памяти подряд «пачками», а прыжки по ссылкам между узлами — это промахи кэша. Поэтому выбирай `LinkedList` только когда действительно нужны O(1)-операции с узлами в середине.

---

## 4. `Dictionary<TKey, TValue>` — хэш-таблица

Самая важная коллекция для собеседования. Хранит пары ключ → значение и находит значение по ключу за **O(1) в среднем**.

### Как устроен

1. Для ключа вызывается `GetHashCode()` → целое число.
2. По хэшу вычисляется номер **бакета** (bucket — «корзина»): примерно `hash % buckets.Length`.
3. В бакете может оказаться несколько ключей (**коллизия** — collision): разные ключи дали один бакет. Они хранятся цепочкой, и `Dictionary` сравнивает ключи через `Equals`, пока не найдёт нужный.
4. Когда элементов становится больше, чем бакетов, таблица **расширяется** (resize/rehash): создаётся массив побольше (размер — простое число), все элементы раскладываются заново. Это O(n), но редко — амортизированно O(1).

```csharp
var stock = new Dictionary<string, int>
{
    ["apple"] = 10,
    ["pear"] = 3,
};

stock["plum"] = 7;                    // добавить или перезаписать
stock.Add("kiwi", 1);                 // добавить; если ключ есть — ArgumentException
bool ok = stock.TryAdd("kiwi", 5);    // false, без исключения

// stock["banana"]                    // KeyNotFoundException!
if (stock.TryGetValue("banana", out int count))  // один поиск вместо ContainsKey + индексатор
    Console.WriteLine(count);

Console.WriteLine(stock.GetValueOrDefault("banana"));  // вывод: 0
```

### Роль `GetHashCode` и `Equals`

- `GetHashCode` выбирает бакет — поиск сужается до нескольких элементов.
- `Equals` окончательно подтверждает, что ключ тот самый.
- **Контракт:** если `a.Equals(b)`, то `a.GetHashCode() == b.GetHashCode()`. Обратное не обязательно (коллизии допустимы).

Что ломается при нарушении:

```csharp
public class BadKey   // переопределён Equals, но НЕ GetHashCode
{
    public int Id { get; init; }
    public override bool Equals(object? obj) => obj is BadKey other && other.Id == Id;
}

var dict = new Dictionary<BadKey, string> { [new BadKey { Id = 1 }] = "one" };
Console.WriteLine(dict.ContainsKey(new BadKey { Id = 1 }));
// вывод: False — хэши разных объектов разные (по умолчанию — «по ссылке»), ищем не в том бакете
```

Вторая ловушка — **изменяемый ключ**: если поменять поле, участвующее в хэше, после добавления в словарь, элемент «потеряется» в старом бакете. Поэтому ключи должны быть неизменяемыми (`string`, `int`, `record`, `readonly record struct`).

> **Почему так?** Хэш-таблица превращает «найди среди n элементов» в «вычисли номер ячейки» — отсюда O(1). Плата — память (бакеты с запасом), отсутствие порядка и зависимость от качества хэш-функции: плохой `GetHashCode` (например, всегда `return 0`) свалит всё в один бакет, и словарь деградирует до O(n).

> 💼 **На собеседовании:** «Dictionary — хэш-таблица: по `GetHashCode` ключа выбирается бакет, внутри бакета ключи сравниваются через `Equals`. Поиск, вставка, удаление — O(1) в среднем, O(n) в худшем при плохом хэше. Если переопределяешь `Equals` — обязательно переопредели `GetHashCode`, иначе равные ключи окажутся в разных бакетах. Порядок перечисления не гарантирован».

**Порядок элементов:** `Dictionary` **не гарантирует** порядок перечисления. Сейчас он часто совпадает с порядком добавления, но после удалений меняется. Не полагайся на него — нужна сортировка, используй `SortedDictionary` или сортируй явно.

### Компараторы

Правило равенства можно задать снаружи через `IEqualityComparer<T>`:

```csharp
var users = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
users["Admin"] = 1;
Console.WriteLine(users.ContainsKey("ADMIN"));  // вывод: True
```

→ **Задача 1** (см. exercises.md) — подсчёт частоты слов.

---

## 5. `HashSet<T>` — множество

`HashSet<T>` — та же хэш-таблица, но только ключи без значений. Отвечает на вопрос «есть ли элемент?» за O(1).

```csharp
var visited = new HashSet<string> { "home", "catalog" };
Console.WriteLine(visited.Add("cart"));   // вывод: True — добавлен
Console.WriteLine(visited.Add("home"));   // вывод: False — уже был

var a = new HashSet<int> { 1, 2, 3 };
var b = new HashSet<int> { 2, 3, 4 };
a.IntersectWith(b);                        // пересечение «на месте»: a = {2, 3}
Console.WriteLine(string.Join(",", a));    // вывод: 2,3
```

Операции над множествами: `UnionWith`, `IntersectWith`, `ExceptWith`, `IsSubsetOf`, `Overlaps`.

> **Почему так?** Частая ошибка новичков — проверять вхождение через `List.Contains` в цикле. `HashSet` существует ровно для этого: проверка O(1) вместо O(n). Правило: «нужно часто спрашивать "есть ли?" — бери HashSet».

→ **Задача 2** — удаление дубликатов с сохранением порядка.
→ **Задача 3** — Two Sum за O(n).

---

## 6. Отсортированные коллекции

| Коллекция | Внутри | Операции | Когда |
|---|---|---|---|
| `SortedDictionary<K, V>` | красно-чёрное дерево | O(log n) | нужен порядок ключей и частые вставки |
| `SortedList<K, V>` | два отсортированных массива | поиск O(log n), вставка O(n) | данные почти не меняются, важна память |
| `SortedSet<T>` | красно-чёрное дерево | O(log n), `Min`/`Max`, `GetViewBetween` | уникальные элементы в порядке |

```csharp
var schedule = new SortedDictionary<TimeOnly, string>
{
    [new TimeOnly(14, 0)] = "Код-ревью",
    [new TimeOnly(9, 30)] = "Стендап",
};
foreach (var (time, title) in schedule)
    Console.WriteLine($"{time:HH:mm} {title}");
// вывод:
// 09:30 Стендап
// 14:00 Код-ревью
```

> **Почему так?** Дерево поддерживает порядок при каждой вставке — платим O(log n) вместо O(1) у хэш-таблицы. Если отсортированный вид нужен один раз в конце — дешевле собрать в `Dictionary`/`List` и отсортировать один раз (O(n log n)).

---

## 7. `Queue<T>`, `Stack<T>`, `PriorityQueue<TElement, TPriority>`

```csharp
var queue = new Queue<string>();          // FIFO — first in, first out
queue.Enqueue("order-1");
queue.Enqueue("order-2");
Console.WriteLine(queue.Dequeue());       // вывод: order-1

var history = new Stack<string>();        // LIFO — last in, first out (undo, навигация «назад»)
history.Push("page-1");
history.Push("page-2");
Console.WriteLine(history.Pop());         // вывод: page-2

var tasks = new PriorityQueue<string, int>();   // меньший приоритет — первым (min-heap)
tasks.Enqueue("обновить README", 3);
tasks.Enqueue("упал прод", 1);
Console.WriteLine(tasks.Dequeue());       // вывод: упал прод
```

Все `Enqueue`/`Dequeue`/`Push`/`Pop` у `Queue` и `Stack` — O(1) (кольцевой буфер и массив). У `PriorityQueue` (.NET 6+) — O(log n): внутри двоичная куча (binary heap).

**Ловушка:** `PriorityQueue` **не стабилен** — элементы с одинаковым приоритетом выходят в произвольном порядке. Нужен FIFO среди равных — добавь к приоритету порядковый номер: `(priority, sequence)`.

> **Почему так?** Куча не поддерживает полный порядок элементов (это было бы дорого), а лишь гарантирует, что минимальный — на вершине. Поэтому всё, что сверх «достань минимум», — твоя ответственность.

→ **Задача 5** — «выбери коллекцию»: очередь обращений в поддержку.

---

## 8. Иерархия интерфейсов коллекций

```
IEnumerable<T>                 — можно перебрать (foreach). Только GetEnumerator.
 └─ ICollection<T>             — + Count, Add, Remove, Contains, Clear
     ├─ IList<T>               — + индексатор, Insert, IndexOf, RemoveAt
     ├─ ISet<T>                — + операции над множествами
     └─ IDictionary<K, V>      — + доступ по ключу, Keys, Values

IReadOnlyCollection<out T>     — IEnumerable<T> + Count
 ├─ IReadOnlyList<out T>       — + индексатор только на чтение
 └─ IReadOnlyDictionary<K, V>  — + TryGetValue, ContainsKey, индексатор на чтение
```

Как выбирать тип параметра и возвращаемого значения:
- **Параметр** — самый общий интерфейс, которого хватает: нужно только перебрать — `IEnumerable<T>`; нужен `Count`/индекс — `IReadOnlyList<T>`/`IReadOnlyCollection<T>`.
- **Возвращаемое значение** — read-only интерфейс (`IReadOnlyList<T>`), чтобы вызывающий не мог изменить внутреннее состояние объекта.

```csharp
public class Order
{
    private readonly List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines;   // читать можно, менять — только через методы Order
    public void AddLine(OrderLine line) { /* проверки инвариантов */ _lines.Add(line); }
}
```

Важно: `IReadOnlyList` — это **представление** (view), а не неизменяемая копия. Владелец (`Order`) всё ещё может менять список, и изменения будут видны. Для гарантированной неизменяемости — immutable-коллекции (раздел 11).

> **Почему так?** Принцип «принимай общее, возвращай конкретное, но безопасное». Общий параметр позволяет передать и массив, и список, и LINQ-запрос. Read-only результат защищает инварианты класса — это инкапсуляция из модуля 04.

> 💼 **На собеседовании:** «`IEnumerable<T>` — только перебор; `ICollection<T>` добавляет `Count`, `Add`, `Remove`; `IList<T>` — индекс. Для публичного API возвращаю `IReadOnlyList<T>` или `IReadOnlyCollection<T>`, чтобы не раскрывать возможность изменения».

---

## 9. `IEnumerable`, `IEnumerator` и `yield return`

`foreach` — синтаксический сахар:

```csharp
foreach (var x in items) Console.WriteLine(x);

// компилятор превращает примерно в:
using (IEnumerator<int> e = items.GetEnumerator())
{
    while (e.MoveNext())
    {
        var x = e.Current;
        Console.WriteLine(x);
    }
}
```

`yield return` позволяет написать свой итератор, не реализуя `IEnumerator` вручную. Компилятор генерирует класс-state machine (конечный автомат), который запоминает, где остановился:

```csharp
static IEnumerable<int> Countdown(int from)
{
    Console.WriteLine("старт");
    for (int i = from; i > 0; i--)
        yield return i;          // отдать значение и «заснуть» до следующего MoveNext
    Console.WriteLine("конец");
}

var seq = Countdown(3);          // НИЧЕГО не выведено: тело ещё не выполнялось
foreach (int x in seq) Console.Write($"{x} ");
// вывод: старт
//        3 2 1 конец
```

Свойства итераторов:
- **Ленивость (lazy evaluation):** элементы вычисляются по требованию. Возможны бесконечные последовательности.
- **Каждый `foreach` — заново:** перебрав `seq` дважды, ты дважды выполнишь тело метода (подробнее — multiple enumeration в модуле 11).
- **Проверки аргументов тоже ленивые!** Классическая ловушка:

```csharp
static IEnumerable<int> Take(IEnumerable<int> source, int count)
{
    if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));  // выполнится только при MoveNext!
    foreach (var x in source) { if (count-- == 0) yield break; yield return x; }
}

var q = Take(numbers, -1);   // исключения НЕТ
// ... где-то далеко ...
foreach (var x in q) { }      // исключение вылетит здесь — далеко от места ошибки
```

Решение — разделить метод: внешний (без `yield`) проверяет аргументы и возвращает результат внутреннего итератора (локальной функции):

```csharp
static IEnumerable<int> Take(IEnumerable<int> source, int count)
{
    ArgumentOutOfRangeException.ThrowIfNegative(count);   // сразу
    return Iterator(source, count);

    static IEnumerable<int> Iterator(IEnumerable<int> items, int n)
    {
        foreach (var x in items) { if (n-- == 0) yield break; yield return x; }
    }
}
```

Так устроены операторы LINQ в BCL.

> **Почему так?** Ленивость экономит память и время: можно обработать файл на 10 ГБ построчно или взять первые 5 элементов бесконечной последовательности. Но ленивость переносит выполнение во времени — и вместе с ним ошибки. Поэтому валидацию делают «жадно» (eager), а работу — лениво.

→ **Задача 4** — свои итераторы `Range`, `Naturals`, `EveryNth`.

---

## 10. Изменение коллекции во время `foreach`

```csharp
var cart = new List<string> { "milk", "bread", "milk" };
foreach (var item in cart)
    if (item == "milk")
        cart.Remove(item);
// InvalidOperationException: Collection was modified; enumeration operation may not execute.
```

Коллекции BCL хранят счётчик версий (`_version`); enumerator проверяет его на каждом `MoveNext`. Варианты исправления:

```csharp
cart.RemoveAll(item => item == "milk");              // лучший вариант для List

for (int i = cart.Count - 1; i >= 0; i--)            // обратный цикл for — индексы не «съезжают»
    if (cart[i] == "milk") cart.RemoveAt(i);

foreach (var item in cart.ToList())                   // перебор копии (лишняя аллокация)
    if (item == "milk") cart.Remove(item);
```

> **Почему так?** Если бы изменение было разрешено, enumerator мог бы пропустить элемент или выйти за границу массива — тихий баг. «Fail fast» (упасть сразу и громко) лучше, чем неверный результат. Для `Dictionary` с .NET Core 3.0 `Remove` во время перебора разрешён, но полагаться на это в общем случае не стоит.

---

## 11. Immutable, frozen и concurrent коллекции (обзор)

- **`System.Collections.Immutable`** (`ImmutableList<T>`, `ImmutableDictionary<K, V>`): любая «модификация» возвращает новую коллекцию, старая не меняется. Удобно для многопоточности и функционального стиля; операции дороже (O(log n)).
- **`FrozenDictionary<K, V>`, `FrozenSet<T>`** (.NET 8+, `System.Collections.Frozen`): создаются один раз (дорого), зато чтение максимально быстрое. Для справочников, которые не меняются после старта приложения.
- **`ConcurrentDictionary`, `ConcurrentQueue`** и др. — потокобезопасные коллекции. Подробно — в модуле 13.

```csharp
using System.Collections.Frozen;

FrozenDictionary<string, decimal> vatRates = new Dictionary<string, decimal>
{
    ["standard"] = 0.20m,
    ["reduced"] = 0.10m,
}.ToFrozenDictionary();
```

> **Почему так?** Обычные коллекции оптимизированы под «читаем и пишем». Если запись бывает один раз, а чтение — миллионы раз, можно потратить больше времени на построение и получить более быстрый поиск. Разные профили нагрузки — разные коллекции.

---

## 12. Как выбрать коллекцию

| Нужно | Бери | Почему |
|---|---|---|
| Последовательность с доступом по индексу | `List<T>` / `T[]` | O(1) по индексу, компактно |
| Найти по ключу | `Dictionary<K, V>` | O(1) |
| «Уже видели?» / уникальность | `HashSet<T>` | O(1) Contains/Add |
| Обработка в порядке поступления | `Queue<T>` | FIFO O(1) |
| Отмена действий / «назад» | `Stack<T>` | LIFO O(1) |
| Самый срочный первым | `PriorityQueue<T, P>` | O(log n) |
| Данные всегда отсортированы по ключу | `SortedDictionary` / `SortedSet` | O(log n) с порядком |
| Частые вставки/удаления по узлу + порядок | `LinkedList<T>` (+ словарь) | O(1) по узлу |
| Справочник, не меняется после старта | `FrozenDictionary` | быстрейшее чтение |
| Многопоточный доступ | `Concurrent*` | модуль 13 |

→ **Задача 6** (🧪 исследование) — замер `List.Contains` vs `HashSet.Contains` и роста capacity.
→ **Задача 7** ⭐ — LRU-кэш на `Dictionary` + `LinkedList`.

---

## Типичные ошибки

1. **`List.Contains` / `IndexOf` в цикле** — O(n²). Нужен `HashSet` или `Dictionary`.
2. **`ContainsKey` + индексатор** вместо `TryGetValue` — двойной поиск; индексатор без проверки — `KeyNotFoundException`.
3. **`Equals` без `GetHashCode`** у ключа — словарь «не находит» элементы.
4. **Изменяемый ключ** в `Dictionary`/`HashSet` — элемент теряется после изменения.
5. **Расчёт на порядок `Dictionary`/`HashSet`** — не гарантирован.
6. **Изменение коллекции в `foreach`** — `InvalidOperationException`.
7. **Проверка аргументов внутри метода с `yield`** — исключение появляется не там и не тогда.
8. **Возврат внутреннего `List<T>` из публичного свойства** — внешний код ломает инварианты.
9. **`LinkedList` «потому что вставка O(1)»** — на практике `List` почти всегда быстрее.
10. **Ожидание FIFO от `PriorityQueue`** при равных приоритетах.

---

## Шпаргалка

```csharp
// List<T>: [i] O(1), Add O(1)*, Insert/RemoveAt(0) O(n), Contains O(n)
var list = new List<int>(capacity: 100);

// Dictionary: O(1) в среднем
dict[key] = value;                       // добавить/перезаписать
dict.TryAdd(key, value);                 // добавить, если нет
if (dict.TryGetValue(key, out var v)) {} // искать — так
dict.GetValueOrDefault(key);
new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

// HashSet: Add возвращает bool
if (seen.Add(x)) { /* впервые */ }

// Queue (FIFO), Stack (LIFO), PriorityQueue (min-heap, НЕ стабилен)
pq.Enqueue(item, (priority, seq++));

// Итератор с жадной проверкой
IEnumerable<T> M<T>(IEnumerable<T> src) { ArgumentNullException.ThrowIfNull(src); return Iter(src); static IEnumerable<T> Iter(IEnumerable<T> s) { foreach (var x in s) yield return x; } }

// Удаление при переборе
list.RemoveAll(x => cond);  // или for в обратном порядке

// API: принимай IEnumerable<T>/IReadOnlyList<T>, возвращай IReadOnlyList<T>
```

---

## Глоссарий

| Термин | Что значит |
|---|---|
| Big-O | Оценка роста времени/памяти алгоритма при росте входа |
| Амортизированная сложность | Средняя стоимость операции в длинной серии (List.Add — O(1)) |
| Capacity | Размер внутреннего массива коллекции (≥ Count) |
| Hash table | Хэш-таблица: массив бакетов, индекс которого вычисляется из хэша ключа |
| Bucket | Ячейка хэш-таблицы, где лежат ключи с одинаковым номером |
| Collision | Коллизия: разные ключи попали в один бакет |
| Rehash / resize | Перестройка хэш-таблицы большего размера |
| Binary heap | Двоичная куча — основа PriorityQueue: минимум всегда на вершине |
| Red-black tree | Сбалансированное дерево поиска (SortedDictionary, SortedSet) |
| Iterator / `yield return` | Метод, лениво возвращающий элементы; компилятор строит state machine |
| Lazy evaluation | Ленивое вычисление — значение вычисляется при запросе |
| Cache locality | Локальность данных: соседние в памяти данные читаются быстрее |
| LRU | Least Recently Used — вытеснение давно неиспользованного |
| Immutable / Frozen collection | Неизменяемая коллекция / коллекция, оптимизированная для чтения |

---

## Вопросы на собеседовании

<details><summary><b>Как устроен Dictionary&lt;TKey, TValue&gt;?</b></summary>

Это хэш-таблица. Для ключа вычисляется `GetHashCode`, по нему выбирается бакет; в бакете может быть несколько ключей (коллизии), и нужный находится сравнением через `Equals`. Поиск, вставка и удаление — O(1) в среднем, O(n) в худшем случае (все ключи в одном бакете из-за плохого хэша). При заполнении таблица расширяется и элементы перераспределяются (rehash). Порядок перечисления не гарантирован.
</details>

<details><summary><b>Зачем переопределять GetHashCode вместе с Equals?</b></summary>

Хэш-коллекции сначала ищут бакет по `GetHashCode`, а потом сравнивают `Equals`. Если два объекта равны по `Equals`, но имеют разные хэши (по умолчанию хэш «по ссылке»), они окажутся в разных бакетах, и `Dictionary`/`HashSet` не найдут элемент. Контракт: равные объекты обязаны иметь равные хэш-коды. Обратное не требуется.
</details>

<details><summary><b>Чем List&lt;T&gt; отличается от массива и от LinkedList&lt;T&gt;?</b></summary>

Массив — фиксированного размера. `List<T>` — динамический массив: растёт удвоением capacity, `Add` — амортизированно O(1), доступ по индексу — O(1), вставка в начало — O(n). `LinkedList<T>` — двусвязный список: вставка/удаление по известному узлу O(1), но нет индекса (O(n)), больше памяти и хуже cache locality. На практике `List<T>` выбирают почти всегда.
</details>

<details><summary><b>Какая сложность у операций List, Dictionary, HashSet, SortedDictionary?</b></summary>

`List`: индекс O(1), `Add` O(1) амортизированно, `Insert`/`Remove` O(n), `Contains` O(n). `Dictionary`/`HashSet`: `Add`/`Remove`/`Contains` O(1) в среднем. `SortedDictionary`/`SortedSet`: O(log n), зато элементы упорядочены. `PriorityQueue`: `Enqueue`/`Dequeue` O(log n), `Peek` O(1).
</details>

<details><summary><b>Чем IEnumerable&lt;T&gt; отличается от ICollection&lt;T&gt; и IList&lt;T&gt;?</b></summary>

`IEnumerable<T>` — только перебор через `foreach`, может быть ленивым и даже бесконечным. `ICollection<T>` добавляет `Count`, `Add`, `Remove`, `Contains`, `Clear`. `IList<T>` — доступ по индексу, `Insert`, `RemoveAt`. Параметры лучше объявлять как `IEnumerable<T>`/`IReadOnlyList<T>`, а возвращать read-only интерфейсы.
</details>

<details><summary><b>Что такое yield return и как он работает?</b></summary>

Позволяет написать итератор без ручной реализации `IEnumerator`. Компилятор превращает метод в класс-state machine: при каждом `MoveNext` код выполняется до следующего `yield return` и «замирает». Выполнение ленивое: тело не запускается до начала перебора, а каждый новый `foreach` начинает заново. Ловушка: проверки аргументов в таком методе тоже ленивые — их выносят во внешний метод без `yield`.
</details>

<details><summary><b>Что будет, если изменить List во время foreach?</b></summary>

`InvalidOperationException: Collection was modified`. Коллекция увеличивает внутренний счётчик версий при каждом изменении, enumerator проверяет его на `MoveNext`. Решения: `RemoveAll`, обратный цикл `for`, перебор копии (`ToList()`).
</details>

<details><summary><b>Когда HashSet лучше List?</b></summary>

Когда нужно часто проверять вхождение или поддерживать уникальность: `Contains`/`Add` — O(1) против O(n). Плата — больше памяти и отсутствие индекса и гарантированного порядка. Если нужен порядок и уникальность — `List` + `HashSet` вместе (как в задаче удаления дубликатов) или `SortedSet`.
</details>

<details><summary><b>Как реализовать LRU-кэш с O(1) на операцию?</b></summary>

`Dictionary<TKey, LinkedListNode<Entry>>` + `LinkedList<Entry>`. Словарь даёт поиск узла за O(1), список хранит порядок использования: при обращении узел переносится в начало (O(1), т.к. ссылка на узел есть), при переполнении удаляется последний узел, а его ключ (хранится в узле) — из словаря. Массив или `List` не подходят: удаление из середины O(n).
</details>

<details><summary><b>Какие проблемы с изменяемыми ключами словаря?</b></summary>

Хэш вычисляется при добавлении и определяет бакет. Если потом изменить поле, участвующее в `GetHashCode`, новый хэш укажет на другой бакет — элемент «пропадёт»: `ContainsKey` вернёт `false`, хотя объект внутри. Поэтому ключи делают неизменяемыми: `string`, числа, `record`, `readonly struct`.
</details>

<details><summary><b>Стабилен ли PriorityQueue? Как обеспечить FIFO при равных приоритетах?</b></summary>

Нет: это двоичная куча, порядок равных элементов не гарантирован. Чтобы получить FIFO, приоритетом делают кортеж `(priority, sequenceNumber)`, где номер растёт с каждым добавлением; кортежи сравниваются лексикографически.
</details>

<details><summary><b>Зачем возвращать IReadOnlyList&lt;T&gt;, а не List&lt;T&gt;?</b></summary>

Чтобы вызывающий код не мог изменить внутреннее состояние объекта в обход его методов и проверок (инкапсуляция). При этом `IReadOnlyList` — представление, а не копия: владелец может менять коллекцию. Для полной неизменяемости — копия или `ImmutableList`/`FrozenSet`.
</details>

---

## Дополнительно

- [Коллекции и структуры данных — learn.microsoft.com](https://learn.microsoft.com/dotnet/standard/collections/)
- [Выбор класса коллекции](https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class)
- [Iterators (yield)](https://learn.microsoft.com/dotnet/csharp/iterators)
- [PriorityQueue](https://learn.microsoft.com/dotnet/api/system.collections.generic.priorityqueue-2)
- [Frozen collections](https://learn.microsoft.com/dotnet/api/system.collections.frozen)
- Джеффри Рихтер, «CLR via C#» — главы о коллекциях и `GetHashCode`.
- Джон Скит, «C# in Depth» — глава об итераторах (C# 2).
- Для алгоритмов: «Грокаем алгоритмы» (А. Бхаргава) — простое введение в Big-O и хэш-таблицы.
