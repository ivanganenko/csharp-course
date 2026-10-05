# Модуль 11 — задачи

> Код пишется в `starter/Module11.Tasks/`. Тесты: `cd modules/11-linq/starter && dotnet test`.
> Модели (`Order`, `OrderLine`, `Employee`, `Department`, `StockItem`) уже готовы. Тестовые данные с пояснениями — в `Module11.Tests/TestData.cs`: посчитай ожидаемые значения сам, прежде чем запускать тесты.

## Задача 1. Отчёты по заказам  ·  ⏱ ~40 мин  ·  🟢 базовая

**Что сделать:** реализуй в `OrderReports`: `TotalRevenue` (выручка Completed-заказов), `LargeOrderIds` (сортировка по двум ключам), `TopProducts` (`SelectMany` + `GroupBy` + `Take`), `FirstOrderOf` (`FirstOrDefault`), `IsLoyalCustomer` (`Any`/`Count`). Хотя бы один метод напиши в query syntax.

**Зачем:** это ровно те запросы, которые пишут каждый день в бизнес-коде и просят написать на собеседовании («найди топ-3 товара по продажам»). Тренируешь `ThenBy`, выбор между `First` и `FirstOrDefault`, работу со вложенными коллекциями.

**Файл:** `Module11.Tasks/OrderReports.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~OrderReportsTests"`

**Подумай / ответь письменно:**
1. Что будет, если в `LargeOrderIds` вместо `ThenBy` написать второй `OrderBy`?
2. Почему в `IsLoyalCustomer` стоит материализовать заказы покупателя, если по ним два прохода?

---

## Задача 2. Аналитика продаж  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуй в `SalesAnalytics`: `RevenueByCategory` (`GroupBy` + `ToDictionary`), `SalesByMonth` (группировка по составному ключу год+месяц), `OrderIdsByCustomer` (`ToLookup`), `AverageOrderValueByCustomer` (`Average`), `RevenueChanges` (`Zip`).

**Зачем:** группировки — основа любых отчётов и дашбордов. Составной ключ через анонимный тип, разница `GroupBy`/`ToLookup`/`ToDictionary` и «Average на пустой последовательности» — частые вопросы на собеседовании.

**Файл:** `Module11.Tasks/SalesAnalytics.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SalesAnalyticsTests"`

**Подумай / ответь письменно:**
1. Почему анонимный тип `new { Year, Month }` работает как ключ `GroupBy`? Сработал бы обычный класс без переопределения `Equals`?
2. Чем `lookup["Nobody"]` отличается от `dictionary["Nobody"]`?

---

## Задача 3. Сотрудники и отделы: joins  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** реализуй в `HrQueries`: inner join сотрудников и отделов, left join (сотрудники без отдела — с `null`), сводку по **каждому** отделу через `GroupJoin` (средняя зарплата пустого отдела — 0), self-join «сотрудник — руководитель».

**Зачем:** соединения — мост к SQL (модуль 16): `JOIN`, `LEFT JOIN`, self-join спрашивают на собеседованиях и про LINQ, и про SQL. Заодно поймёшь, почему `Join` работает за O(n + m), а `FirstOrDefault` в `Select` — за O(n × m).

**Файл:** `Module11.Tasks/HrQueries.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~HrQueriesTests"`

**Подумай / ответь письменно:**
1. Почему `departments.GroupJoin(employees, ...)` даёт пустые отделы, а `employees.GroupBy(e => e.DepartmentId)` — нет?
2. Зачем приводить `department.Id` к `int?` в ключе соединения?

---

## Задача 4. Найди баг с deferred execution  ·  ⏱ ~35 мин  ·  🟡 средняя

**Что сделать:** класс `Inventory` **уже написан**, но тесты `InventoryTests` падают: в трёх методах есть ошибки, связанные с отложенным выполнением LINQ. Найди их, объясни и исправь, не меняя сигнатуры и тесты. Над каждым исправлением оставь комментарий: «в чём была ошибка и почему».

**Зачем:** на работе ты чаще чинишь чужой код, чем пишешь с нуля, а баги ленивости — одни из самых коварных: код выглядит правильно, а данные «плавают». Вопрос «что не так с этим кодом?» с `IEnumerable` — классика собеседования.

**Файл:** `Module11.Tasks/Inventory.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~InventoryTests"`

**Подумай / ответь письменно:**
1. Для каждого из трёх багов: какой оператор и в какой момент на самом деле выполнялся?
2. Почему `ImportSkus` падает на `OneShotSequence`, хотя на массиве «работает»? Какие реальные источники данных ведут себя как `OneShotSequence`?

---

## Задача 5. LINQ изнутри  ·  ⏱ ~30 мин  ·  🧪 исследование

**Что сделать:** в консольном приложении вне `starter` (`dotnet new console -n LinqLab`):
1. Перепиши два запроса из задач 1–3 из method syntax в query syntax и обратно. Убедись, что результаты совпадают.
2. Создай `var q = numbers.Where(n => { Console.WriteLine($"check {n}"); return n % 2 == 0; }).Select(n => { Console.WriteLine($"map {n}"); return n * 10; });`. Вызови `q.First()`, затем `q.ToList()`, затем `q.Count()`. Опиши порядок и количество строк в выводе. Добавь `OrderBy(n => n)` между `Where` и `Select` — что изменилось?
3. Выведи `numbers.AsQueryable().Where(n => n > 2).Expression` и `typeof` делегата/выражения. Сравни `Func<int, bool> f = n => n > 2;` и `Expression<Func<int, bool>> e = n => n > 2;` — выведи `e.Body`, `e.Parameters[0]`.
4. Проверь: `new int[0].All(n => n > 100)`, `new int[0].Sum()`, `new int[0].Average()` (оберни в try/catch).

**Зачем:** увидеть ленивость, streaming vs non-streaming и разницу «делегат vs expression tree» своими глазами. После этого ответы на вопросы о deferred execution и `IQueryable` будут не заученными, а понятыми.

**Подумай / ответь письменно:**
1. Почему после добавления `OrderBy` строки `check` для всех элементов печатаются до первой строки `map`?
2. Почему EF Core нужен именно `Expression<Func<...>>`, а не `Func<...>`?

---

## Задача 6. Свои операторы LINQ  ·  ⏱ ~50 мин  ·  ⭐ со звёздочкой

**Что сделать:** реализуй extension-методы `Batch(size)` (аналог `Chunk`), `DistinctByKey(keySelector, comparer)` (аналог `DistinctBy`) и `Pairwise()` (пары соседних элементов). Все — ленивые, однопроходные, работают с бесконечными последовательностями и проверяют аргументы сразу.

**Зачем:** «реализуй свой оператор LINQ» — типичная live coding задача: проверяет `yield`, generics, делегаты, понимание ленивости и edge cases одновременно. `Batch` реально нужен — например, отправлять записи в API пачками по 100.

**Файл:** `Module11.Tasks/MyLinqExtensions.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~MyLinqExtensionsTests"`

**Подумай / ответь письменно:**
1. Почему в `Batch` нельзя переиспользовать один список через `Clear()`? Напиши код вызывающей стороны, который это сломает.
2. Где создаётся `HashSet` в `DistinctByKey` — и что будет, если создать его во внешнем методе (до итератора) и перебрать результат дважды?
