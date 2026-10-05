# Модуль 12 — задачи

> Код пишется в `starter/Module12.Tasks/`. Тесты: `cd modules/12-memory-gc/starter && dotnet test`.

## Задача 1. Отчёт с дескриптором: полный Dispose pattern  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** в классе `ReportWriter` реализуй полный Dispose pattern. Класс владеет `Stream` (managed-ресурс) и `FakeNativeHandle` (имитация дескриптора ОС — unmanaged-ресурс).
- `Dispose()` → `Dispose(true)` + `GC.SuppressFinalize(this)`;
- `protected virtual void Dispose(bool disposing)`: при `true` закрыть поток и освободить handle, при `false` — только handle; повторные вызовы ничего не делают;
- **добавь финализатор** `~ReportWriter()` (в starter его нет специально — подумай, почему финализатор, бросающий исключение, опасен);
- `WriteLine` пишет текст + `\n` в UTF-8, после `Dispose` бросает `ObjectDisposedException`.

**Зачем:** «расскажите про Dispose pattern» и «зачем `GC.SuppressFinalize`» — одни из самых частых вопросов по памяти на собеседовании Junior.
**Файл:** `Module12.Tasks/ReportWriter.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ReportWriterTests"`
**Подумай / ответь письменно:**
1. Почему при `disposing == false` нельзя вызывать `_stream.Dispose()`?
2. Если бы вместо `FakeNativeHandle` был `SafeFileHandle`, нужен ли был бы финализатор в `ReportWriter`? Почему?

## Задача 2. Биржевой виджет без утечки памяти  ·  ⏱ ~30 мин  ·  🟢 базовая

**Что сделать:** реализуй `StockTicker` (издатель: `SubscriberCount`, `Publish`) и `PriceWidget` (подписчик). Виджет подписывается на `PriceChanged` в конструкторе, запоминает последнюю цену **своего** тикера и **отписывается в `Dispose`**. Повторный `Dispose` безопасен.

**Зачем:** утечка через события — классический ответ на вопрос «бывают ли утечки памяти в .NET?». Тест `WidgetWithoutDispose_StaysSubscribed_ThisIsTheLeak` показывает саму утечку: «забытый» виджет всё ещё в списке подписчиков, а значит, жив.
**Файл:** `Module12.Tasks/StockTicker.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~PriceWidgetTests"`
**Подумай / ответь письменно:**
1. Кто на кого ссылается при подписке: издатель на подписчика или наоборот? Что из этого следует, если издатель — singleton?
2. Почему `ticker.PriceChanged -= (s, e) => OnPrice(e);` не отпишет обработчик, добавленный такой же лямбдой?

## Задача 3. Парсер без Substring: Span&lt;T&gt;  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуй методы `SpanParser`: `SumCsv` («10, 20,30» → 60), `CountWords`, `TryParseKeyValue` («host = example.local» → key/value), `GetLogLevel` («[ERROR] ...» → «ERROR»). **Правило:** внутри методов никаких `Substring`, `Split`, `ToString()`, `new string` — только срезы `ReadOnlySpan<char>`, `IndexOf`, `Trim`, `int.Parse(span, NumberStyles.Integer, CultureInfo.InvariantCulture)`.

**Зачем:** `Span<T>` всё чаще спрашивают даже у джунов («что это, зачем, почему нельзя в поле класса»), а парсинг логов/CSV — типичная задача, где лишние строки создают нагрузку на GC.
**Файл:** `Module12.Tasks/SpanParser.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SpanParserTests"`
**Подумай / ответь письменно:**
1. Сколько объектов в куче создаст `"10,20,30".Split(',')` и сколько — твоя реализация `SumCsv`?
2. Почему `Span<T>` нельзя сделать полем класса, а `Memory<T>` — можно?

## Задача 4. Контрольная сумма потока с ArrayPool  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `StreamChecksum.SumBytes(stream, pool, bufferSize)` — сумма всех байтов потока. Буфер бери через `pool.Rent`, возвращай через `pool.Return` **в `finally`**. Учти: пул может выдать массив длиннее запрошенного и с мусором (тестовый пул так и делает).

**Зачем:** правильная работа с пулом — `try/finally`, «используй только прочитанное», не трогай массив после `Return` — это то, что проверяют на code review в высоконагруженных сервисах.
**Файл:** `Module12.Tasks/StreamChecksum.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~StreamChecksumTests"`
**Подумай / ответь письменно:**
1. Почему при буфере в 100 000 байт пул особенно выгоден (вспомни про LOH)?
2. Что случится, если вернуть один и тот же массив в пул дважды?

## Задача 5. Исследование: сколько стоит конкатенация  ·  ⏱ ~30 мин  ·  🧪 исследование

**Что сделать:** создай отдельное консольное приложение **вне папки starter** (`dotnet new console -o AllocLab`), вставь код и запусти в **Release** (`dotnet run -c Release`):

```csharp
using System.Text;

const int N = 20_000;

Measure("string +=", () =>
{
    var s = "";
    for (var i = 0; i < N; i++) s += "x";
    return s.Length;
});

Measure("StringBuilder", () =>
{
    var sb = new StringBuilder();
    for (var i = 0; i < N; i++) sb.Append('x');
    return sb.ToString().Length;
});

Measure("new string('x', N)", () => new string('x', N).Length);

static void Measure(string name, Func<int> action)
{
    action(); // прогрев: первый вызов включает JIT-компиляцию
    var gen0 = GC.CollectionCount(0);
    var gen2 = GC.CollectionCount(2);
    var bytes = GC.GetTotalAllocatedBytes(precise: true);

    var length = action();

    Console.WriteLine($"{name,-20} len={length} allocated={(GC.GetTotalAllocatedBytes(true) - bytes) / 1024,8} KB " +
                      $"gen0={GC.CollectionCount(0) - gen0} gen2={GC.CollectionCount(2) - gen2}");
}
```

Затем:
1. Запиши результаты в таблицу. Во сколько раз отличаются аллокации?
2. Увеличь `N` до 50 000. Как выросли аллокации у `+=` и у `StringBuilder` — линейно или быстрее? Появились ли сборки Gen 2 (подсказка: строки длиной > ~42 500 символов попадают в LOH)?
3. Добавь вариант `new StringBuilder(N)` (с начальной ёмкостью). Что изменилось и почему?

**Зачем:** «почему конкатенация в цикле — плохо» спрашивают постоянно; ответ с цифрами, которые ты сам получил, звучит намного убедительнее.
**Тесты:** нет — покажи агенту таблицу и выводы при `/check 12.5`.
**Подумай / ответь письменно:** почему у `+=` рост аллокаций квадратичный, а у `StringBuilder` — примерно линейный?

## Задача 6. ⭐ Пул объектов  ·  ⏱ ~45 мин  ·  🔴 сложная

**Что сделать:** реализуй потокобезопасный `SimpleObjectPool<T>`:
- конструктор `(Func<T> factory, int maxRetained, Action<T>? reset = null)` с проверкой аргументов;
- `Rent()` — отдаёт последний возвращённый объект (LIFO) или создаёт новый через `factory`;
- `Return(item)` — вызывает `reset`, кладёт в пул, но не больше `maxRetained` (лишние отбрасывает);
- `AvailableCount`, `CreatedCount`;
- один объект никогда не выдаётся двум потокам одновременно (тест гоняет `Parallel.For` на 10 000 итераций).

**Зачем:** типичное live coding задание «напиши пул/кэш» проверяет сразу generics с constraint, потокобезопасность и понимание, зачем вообще переиспользовать объекты (меньше работы GC).
**Файл:** `Module12.Tasks/SimpleObjectPool.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SimpleObjectPoolTests"`
**Подумай / ответь письменно:**
1. Зачем ограничение `maxRetained`? Что будет без него в долгоживущем сервисе?
2. Почему `factory()` лучше вызывать вне `lock`?
