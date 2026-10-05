# Модуль 06 — задачи

> Код пишется в `starter/Module06.Tasks/`. Тесты: `cd modules/06-struct-record-equality/starter && dotnet test`.
>
> Сигнатуры `public`-членов не меняй. Можно добавлять private-поля/методы и заменять `=> throw ...` на auto-properties. В задаче 5 разрешено (и нужно) добавить в record явное свойство `Currency`.

## Задача 1. Равенство для класса Point  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуй в `Point` полный набор: `IEquatable<Point>.Equals`, `Equals(object?)`, `GetHashCode`, `==`, `!=`. Две точки с одинаковыми координатами должны быть равны везде, включая `HashSet<Point>` и `Dictionary<Point, string>`. Корректно обработай `null` с обеих сторон `==`.

**Зачем:** «реализуйте Equals и GetHashCode» — классика собеседования и live coding. Проверяют знание контракта и ловушку рекурсии в `operator ==`.
**Файл:** `Module06.Tasks/Point.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~PointTests"`
**Мини-эксперимент:** временно верни в `GetHashCode` `base.GetHashCode()` и запусти тесты. Какие упали и почему `Equals`-тесты при этом зелёные?
**Подумай / ответь письменно:**
1. Почему `Dictionary` не находит ключ, если `GetHashCode` не переопределён, хотя `Equals` правильный?
2. Почему внутри `operator ==` нельзя писать `left == null`?

## Задача 2. Records: DTO, `with` и ловушка с коллекцией  ·  ⏱ ~35 мин  ·  🟢 базовая

**Что сделать:**
1. В `ProductOperations` реализуй `ApplyDiscount` и `Rename` — через `with`, не изменяя исходный record.
2. В record `ShoppingList` переопредели равенство: списки равны, если совпадает владелец и товары поэлементно в том же порядке (даже если это разные экземпляры `List`). Согласуй `GetHashCode`.

**Зачем:** records — стандарт для DTO в современных .NET-проектах (Web API, сообщения). Нужно понимать, что они генерируют, и где их равенство «поверхностное».
**Файл:** `Module06.Tasks/Records.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~RecordTests"`
**Подумай / ответь письменно:**
1. Что сгенерировал компилятор для `ProductDto`? (Посмотри на SharpLab.io — вкладка C#.)
2. Почему `ShoppingList` без переопределения считал бы разные экземпляры списков неравными? Почему тест `with` проверяет `Assert.Same(a.Items, b.Items)`?

## Задача 3. Tuples: несколько значений из метода  ·  ⏱ ~20 мин  ·  🟢 базовая

**Что сделать:** реализуй в `Statistics`: `MinMax` (за один проход, именованный кортеж), `CountAndAverage` (без переполнения `int` при больших числах), `Swap` (через деконструкцию).

**Зачем:** tuples постоянно встречаются в коде (`TryGet...`, LINQ, deconstruction). Плюс напоминание про overflow из модуля 02.
**Файл:** `Module06.Tasks/Statistics.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~StatisticsTests"`
**Подумай / ответь письменно:**
1. `(int Min, int Max)` — value type или reference type? Что такое `Item1` и куда деваются имена `Min`/`Max` после компиляции?
2. Когда вместо кортежа стоит объявить record?

## Задача 4. Склад и ловушка mutable struct  ·  ⏱ ~35 мин  ·  🟡 средняя  ·  🧪 с исследованием

**Что сделать:**
1. **Исследование (сначала!):** во временном консольном проекте создай `List<StockEntry>` с одним элементом и попробуй три способа увеличить количество: `list[0].Add(5)`, `foreach (var e in list) e.Add(5)`, `list[0].Quantity += 5`. Что компилируется? Что реально меняет список? Запиши вывод.
2. Реализуй `Warehouse` поверх `List<StockEntry>`: `Receive`, `TryShip`, `GetQuantity`. Изменения должны реально попадать в список.

**Зачем:** самая коварная ловушка value types; на собеседовании её дают как «что выведет код» и как вопрос «почему struct должен быть immutable».
**Файл:** `Module06.Tasks/Warehouse.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~WarehouseTests"`
**Подумай / ответь письменно:**
1. Почему `list[0].Quantity += 5` не компилируется, а `list[0].Add(5)` — компилируется? Что делает каждый?
2. Как переделать `StockEntry`, чтобы ошибка стала невозможной? (Подсказка: `readonly record struct` + `with`.) Почему для массива `arr[0].Add(5)` работает?

## Задача 5. Money как `readonly record struct`  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** доведи `Money` из модуля 04 до value object на `readonly record struct`:
- валидация и нормализация валюты, которая срабатывает **и в конструкторе, и в `with`** (подсказка в XML-комментарии);
- операторы `+`, `-`, `*` (с округлением), `<`, `>`, `<=`, `>=`; разные валюты → `InvalidOperationException`;
- `Zero`, `ToString` → `"12.50 EUR"`.
Равенство писать **не нужно** — его генерирует компилятор. Убедись в этом тестом `Equality_IsByValue`.

**Зачем:** перегрузка операторов, records, value types и неизменяемость в одной задаче. Сравни с классом из модуля 04: сколько кода ушло?
**Файл:** `Module06.Tasks/Money.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~MoneyTests"`
**Подумай / ответь письменно:**
1. Почему `Money` разумно сделать struct, а `BankAccount` — нет?
2. Почему валидация в инициализаторе `{ get; init; } = Normalize(Currency)` не сработала бы в `with`?
3. Что будет у `default(Money)`? Чем это опасно и как с этим живут?

## Задача 6. Составной ключ для Dictionary: бронирование мест  ·  ⏱ ~50 мин  ·  🔴 сложная  ·  ⭐ со звёздочкой

**Что сделать:** реализуй `readonly struct SeatKey` (зал, ряд, место) с равенством, где название зала сравнивается **без учёта регистра**, и согласованным `GetHashCode`. Затем `BookingService` на `Dictionary<SeatKey, string>`: бронь, проверка, отмена, список мест клиента с сортировкой. `default(SeatKey)` не должен ронять `GetHashCode`.

**Зачем:** задача на понимание того, как хэш-таблица использует `GetHashCode` + `Equals`. Ключевая ловушка: `Equals` без учёта регистра + обычный `Hall.GetHashCode()` = «бронь есть, но не находится». Встречается в реальных проектах (кэши, составные ключи, группировки).
**Файл:** `Module06.Tasks/SeatBooking.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SeatBookingTests"`
**Подумай / ответь письменно:**
1. Почему здесь не подошёл `readonly record struct SeatKey(string Hall, int Row, int Seat)` «как есть»? Какие два способа решить задачу без ручного `Equals` ты видишь? (Подсказка: нормализация в конструкторе; `IEqualityComparer<T>` для словаря.)
2. Что произойдёт, если в `GetHashCode` использовать `Hall.GetHashCode()`? Какой тест упадёт и почему именно он?
3. Почему struct-ключу важно реализовать `IEquatable<SeatKey>`?
