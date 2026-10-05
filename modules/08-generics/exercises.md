# Модуль 08 — задачи

> Код пишется в `starter/Module08.Tasks/`. Тесты: `cd modules/08-generics/starter && dotnet test`.

## Задача 1. Свой `SimpleStack<T>`  ·  ⏱ ~40 мин  ·  🟢 базовая

**Что сделать:** реализуй generic-стек на основе массива `T[]`: `Push`, `Pop`, `Peek`, `TryPop`, `Clear`, `Count`, `IsEmpty` и перебор `foreach` от вершины ко дну. Массив должен расти (удвоением), когда заканчивается место. Пустой стек при `Pop`/`Peek` → `InvalidOperationException`. Не используй внутри `Stack<T>` и `List<T>`.

**Зачем:** «Реализуй стек/очередь» — частая live coding задача. Ты увидишь, как устроены `Stack<T>` и `List<T>` изнутри (capacity, амортизированный рост), почему `T[]` не делает boxing и зачем нужен Try-pattern.

**Файл:** `Module08.Tasks/SimpleStack.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SimpleStackTests"`

**Подумай / ответь письменно:**
1. Почему массив растёт удвоением, а не на 1 элемент? Какая получается сложность `Push`?
2. Зачем в `Pop` записывать `default` в освободившуюся ячейку, если `_count` всё равно уменьшился?

**Бонус (без тестов):** сделай `SimpleQueue<T>` на кольцевом буфере (circular buffer) — индексы `_head` и `_tail`, которые «заворачиваются» через `%`.

---

## Задача 2. Generic-алгоритмы и constraints  ·  ⏱ ~35 мин  ·  🟢 базовая

**Что сделать:** в статическом классе `GenericAlgorithms` реализуй:
- `Max<T>(IEnumerable<T>) where T : IComparable<T>` — максимум за один проход;
- `Clamp<T>(value, min, max)` — «зажать» значение в диапазон;
- `Swap<T>(ref T, ref T)`;
- `CountDefaults<T>(IEnumerable<T>)` — сколько элементов равны `default(T)`;
- `CreateMany<T>(int count) where T : new()` — список новых экземпляров.

Без LINQ.

**Зачем:** constraints — главный вопрос по generics на собеседовании. Ты на практике увидишь, почему без `IComparable<T>` нельзя сравнить два `T`, почему `==` не работает с неограниченным `T` и как работает `new()`.

**Файл:** `Module08.Tasks/GenericAlgorithms.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~GenericAlgorithmsTests"`

**Подумай / ответь письменно:**
1. Почему в `CountDefaults` нельзя написать `item == default`, а `EqualityComparer<T>.Default.Equals` — можно?
2. Почему `Max` работает с `int`, `double`, `DateOnly`, но не скомпилируется с `object`?

---

## Задача 3. `InMemoryRepository<T>`  ·  ⏱ ~35 мин  ·  🟡 средняя

**Что сделать:** реализуй generic-репозиторий `InMemoryRepository<T> where T : class, IEntity`: `Add` (дубликат Id → исключение), `GetById` (null, если нет), `GetAll` (копия, сортировка по Id), `Update`, `Remove`, `Count`. Поиск по Id — за O(1). Один и тот же класс должен работать для `Book` и `Reader`.

**Зачем:** паттерн Repository встретится в модулях 15–17 и почти в каждом реальном проекте. Ты увидишь, как constraint на интерфейс (`IEntity`) даёт доступ к `Id`, а `class` — право вернуть `null`.

**Файл:** `Module08.Tasks/InMemoryRepository.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~InMemoryRepositoryTests"`

**Подумай / ответь письменно:**
1. Что изменится, если убрать constraint `class`? Что тогда будет означать `T?` в `GetById`?
2. Почему `GetAll` возвращает `IReadOnlyList<T>` и копию, а не внутреннюю коллекцию?

---

## Задача 4. `Cache<TKey, TValue>` со статистикой  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуй кэш с двумя параметрами типа: `Set`, `TryGet` (Try-pattern с `[MaybeNullWhen(false)]`), `GetOrAdd(key, factory)` (factory вызывается только при промахе), `Remove`, `Count`, счётчики `Hits`/`Misses`.

**Зачем:** кэширование — повседневная задача (курсы валют, профили, настройки). `GetOrAdd` с фабрикой — тот же API, что у `ConcurrentDictionary` и `IMemoryCache`. Первое знакомство с `Func<TKey, TValue>` — подробно в модуле 10.

**Файл:** `Module08.Tasks/Cache.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~CacheTests"`

**Подумай / ответь письменно:**
1. Зачем на `TKey` constraint `notnull`? Что будет без него?
2. Почему `GetOrAdd` принимает фабрику, а не готовое значение `GetOrAdd(key, value)`?

---

## Задача 5. Boxing и static-поля generic-типов  ·  ⏱ ~25 мин  ·  🧪 исследование

**Что сделать:** создай консольное приложение вне папки `starter` (`dotnet new console -n GenericsLab`) и:
1. Заполни `ArrayList` и `List<int>` миллионом чисел. До и после замерь `GC.GetAllocatedBytesForCurrentThread()` и выведи разницу. Повтори для `List<string>` и `ArrayList` со строками.
2. Объяви `class Counter<T> { public static int Count; }`, увеличь `Counter<int>.Count` дважды и `Counter<string>.Count` один раз. Выведи оба значения.
3. Выведи `typeof(List<int>) == typeof(List<string>)` и `typeof(List<>).Name`.
4. (Если есть время) открой `List<int>` и `ArrayList` в [SharpLab.io](https://sharplab.io) и найди инструкцию `box` в IL.

**Зачем:** «ArrayList vs List<T>» и «что такое boxing» — классика собеседования. Цифры, полученные своими руками, запоминаются лучше любого определения.

**Подумай / ответь письменно:**
1. Во сколько раз отличаются аллокации для `int`? А для `string`? Почему для строк разница маленькая?
2. Почему у `Counter<int>` и `Counter<string>` разные значения `Count`?

---

## Задача 6. Вариантность в системе сообщений  ·  ⏱ ~45 мин  ·  ⭐ со звёздочкой

**Что сделать:** в `Messaging.cs` есть иерархия `Message` → `EmailMessage`/`SmsMessage` и два интерфейса:
- `IMessageSource<T>` — источник (только отдаёт `T`);
- `IMessageHandler<T>` — обработчик (только принимает `T`).

1. Объяви вариантность: источник — ковариантный, обработчик — контравариантный.
2. Реализуй `QueueMessageSource<T>` (отдаёт сообщения по очереди, потом `null`), `LoggingHandler` (обработчик любых `Message`, пишет `"EmailMessage: Hello"`) и `MessagePump.Drain`.

Тесты проверяют, что источник email-ов можно использовать как источник любых сообщений, а общий `LoggingHandler` — как обработчик email-ов.

**Зачем:** covariance/contravariance — вопрос, на котором «сыпется» большинство junior-кандидатов. Самостоятельно объявив `out`/`in` и увидев, какие методы после этого компилятор запрещает, ты сможешь объяснить тему своими словами.

**Файл:** `Module08.Tasks/Messaging.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~VarianceTests"`

**Подумай / ответь письменно:**
1. Добавь в `IMessageSource<out T>` метод `void Push(T message)`. Какая ошибка компиляции? Объясни, какую «дыру» она закрывает (приведи пример со `SmsMessage`).
2. Почему `QueueMessageSource<T>` нельзя объявить как `class QueueMessageSource<out T>`?
3. Почему тесты проверяют вариантность через приведение к `object`, а не обычным присваиванием?
