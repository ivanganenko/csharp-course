# Модуль 16. EF Core и SQL

> **Зачем:** почти любое бэкенд-приложение хранит данные в реляционной БД, а в .NET стандарт доступа к ней — Entity Framework Core. На собеседовании Junior спрашивают базовый SQL (JOIN, GROUP BY, индексы, транзакции) и EF Core: DbContext, миграции, tracking, N+1, IQueryable vs IEnumerable. **Время:** ~10–12 ч. **Нужно:** модули 11 (LINQ), 13 (async), 15 (DI, Web API).

## Что ты будешь уметь

- Писать базовые SQL-запросы: SELECT, WHERE, JOIN, GROUP BY/HAVING, ORDER BY; объяснять индексы, транзакции и нормализацию.
- Описывать модель EF Core: сущности, связи 1-N и N-N, конфигурация через conventions и Fluent API, уникальные индексы.
- Понимать жизненный цикл `DbContext`, change tracker, `SaveChanges`, разницу tracking/`AsNoTracking`.
- Загружать связанные данные (`Include`/`ThenInclude`) и находить проблему N+1 по SQL-логам.
- Строить запросы на `IQueryable` (фильтры, пагинация) так, чтобы работа шла в БД, а не в памяти.
- Объяснять миграции и когда использовать транзакции.

---

## 1. SQL-минимум

Реляционная БД хранит данные в **таблицах** (строки и столбцы). У каждой строки — **первичный ключ** (primary key, PK), уникальный идентификатор. Связи между таблицами — через **внешний ключ** (foreign key, FK): столбец `Books.AuthorId` ссылается на `Authors.Id`.

```sql
-- Выборка с фильтром и сортировкой
SELECT Title, PublishedYear FROM Books WHERE PublishedYear >= 2010 ORDER BY Title;

-- JOIN: книги вместе с именем автора
SELECT b.Title, a.Name
FROM Books b
INNER JOIN Authors a ON a.Id = b.AuthorId;

-- LEFT JOIN: все авторы, даже без книг (у них b.* будут NULL)
SELECT a.Name, COUNT(b.Id) AS BookCount
FROM Authors a
LEFT JOIN Books b ON b.AuthorId = a.Id
GROUP BY a.Id, a.Name
HAVING COUNT(b.Id) >= 1          -- HAVING фильтрует ГРУППЫ, WHERE — строки до группировки
ORDER BY BookCount DESC;
```

| Конструкция | Что делает |
|---|---|
| `INNER JOIN` | только пары строк, у которых условие совпало |
| `LEFT JOIN` | все строки левой таблицы + совпавшие из правой (иначе NULL) |
| `GROUP BY` | сворачивает строки в группы; в SELECT — ключи группы и агрегаты (`COUNT`, `SUM`, `AVG`, `MIN`, `MAX`) |
| `WHERE` vs `HAVING` | WHERE — до группировки, HAVING — по результатам агрегатов |
| `NULL` | «нет значения»; сравнение `= NULL` всегда не истинно — нужно `IS NULL` |

**Индекс** (index) — дополнительная структура (обычно B-дерево), которая позволяет находить строки по столбцу без полного просмотра таблицы: поиск за O(log n) вместо O(n). Цена — место на диске и замедление INSERT/UPDATE (индекс тоже надо обновлять). **Уникальный индекс** ещё и гарантирует уникальность значения.

**Транзакция** — группа операций, выполняемая по принципу «всё или ничего». Свойства **ACID**: Atomicity (атомарность), Consistency (согласованность — ограничения не нарушаются), Isolation (изоляция параллельных транзакций), Durability (после COMMIT данные не потеряются).

```sql
BEGIN TRANSACTION;
UPDATE Accounts SET Balance = Balance - 100 WHERE Id = 1;
UPDATE Accounts SET Balance = Balance + 100 WHERE Id = 2;
COMMIT;   -- или ROLLBACK при ошибке: деньги не «исчезнут» на полпути
```

**Нормализация** — устранение дублирования: каждый факт хранится в одном месте. Имя автора — в `Authors`, а в `Books` только `AuthorId`. Переименовали автора — одно UPDATE вместо сотни. Иногда сознательно денормализуют ради скорости чтения (отчёты, кэши).

> **Почему так?** Реляционная модель и SQL — декларативны: ты описываешь, *что* нужно, а оптимизатор БД решает, *как* (какие индексы, порядок JOIN). Поэтому важно отдавать работу базе: она умеет фильтровать, группировать и сортировать на порядки эффективнее, чем код, загрузивший всё в память. Индексы — компромисс «быстрое чтение vs медленная запись и место».

> 💼 **На собеседовании:** «INNER JOIN vs LEFT JOIN?», «WHERE vs HAVING?», «Что такое индекс и почему не повесить индекс на все столбцы?», «Что такое транзакция, ACID?» — ответы выше. Частая задача: «выведи авторов и количество их книг, включая авторов без книг» → LEFT JOIN + GROUP BY + COUNT(b.Id) (не COUNT(*)!).

---

## 2. ORM и EF Core: зачем

**ORM** (Object-Relational Mapper) — библиотека, которая отображает таблицы на классы и переводит LINQ-запросы в SQL. **EF Core** — ORM от Microsoft.

```csharp
var books = await db.Books
    .Where(b => b.PublishedYear >= 2010)
    .OrderBy(b => b.Title)
    .ToListAsync();
// SQL: SELECT ... FROM "Books" AS "b" WHERE "b"."PublishedYear" >= 2010 ORDER BY "b"."Title"
```

| | EF Core | Dapper | ADO.NET (`SqlCommand`) |
|---|---|---|---|
| Что пишешь | LINQ, классы | SQL руками, маппинг автоматически | SQL + ручной маппинг |
| Change tracking, миграции | да | нет | нет |
| Контроль над SQL | меньше | полный | полный |
| Скорость разработки | высокая | средняя | низкая |

> **Почему так?** ORM убирает рутину (маппинг строк в объекты, генерация INSERT/UPDATE, схема из кода) и даёт типобезопасность: опечатка в имени свойства — ошибка компиляции, а не SQL-ошибка в рантайме. Цена — абстракция может породить неэффективный SQL (N+1, лишние колонки), поэтому SQL нужно уметь читать. Многие проекты сочетают EF Core (CRUD) и Dapper (тяжёлые отчёты).

---

## 3. DbContext, DbSet, сущности

```csharp
public class Book
{
    public int Id { get; set; }                  // PK по соглашению (Id или BookId)
    public string Title { get; set; } = "";
    public int AuthorId { get; set; }            // FK по соглашению
    public Author Author { get; set; } = null!;  // навигационное свойство (reference navigation)
    public List<Loan> Loans { get; set; } = new(); // collection navigation
}

public class LibraryDbContext(DbContextOptions<LibraryDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
}

// Регистрация в ASP.NET Core (Scoped по умолчанию):
builder.Services.AddDbContext<LibraryDbContext>(o => o.UseSqlite("Data Source=library.db"));
```

**DbContext** — это **unit of work** («единица работы»): он помнит, какие объекты загружены и что в них изменилось (**change tracker**), и одной командой `SaveChanges` отправляет все изменения в БД (в транзакции). **DbSet<T>** — точка входа для запросов к таблице.

Жизненный цикл: контекст **короткоживущий** — один на HTTP-запрос (Scoped) или на одну бизнес-операцию. Он **не потокобезопасен**: нельзя запускать два запроса параллельно на одном контексте.

> **Почему так?** Короткая жизнь контекста = маленький change tracker (быстро), свежие данные и нет гонок. Долгоживущий контекст копит тысячи отслеживаемых объектов, отдаёт устаревшие данные из кэша идентичности и ломается при параллельном использовании — поэтому Singleton `DbContext` — грубая ошибка (captive dependency из модуля 15).

> 💼 **На собеседовании:** «Что такое DbContext? Какой у него lifetime?» — unit of work + change tracker + точка доступа к DbSet; Scoped, не потокобезопасен. «Что делает SaveChanges?» — находит изменения в tracked-сущностях, генерирует INSERT/UPDATE/DELETE и выполняет их в одной транзакции.

---

## 4. Конфигурация модели: conventions, атрибуты, Fluent API

EF Core строит модель тремя способами (по возрастанию приоритета):

1. **Conventions** (соглашения): `Id` — ключ, `AuthorId` + `Author` — FK, `string` без `?` при включённых nullable reference types — NOT NULL, `List<Book>` — связь 1-N, коллекции навигаций с обеих сторон — N-N.
2. **Data annotations** (атрибуты): `[Required]`, `[MaxLength(200)]`, `[Index(nameof(Isbn), IsUnique = true)]`.
3. **Fluent API** в `OnModelCreating` — самый мощный способ:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Book>(book =>
    {
        book.Property(b => b.Title).IsRequired().HasMaxLength(200);
        book.HasIndex(b => b.Isbn).IsUnique();
        book.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        book.HasMany(b => b.Genres).WithMany(g => g.Books);  // N-N, таблица-связка создаётся автоматически
        book.ToTable(t => t.HasCheckConstraint("CK_Books_CopiesTotal", "\"CopiesTotal\" >= 0"));
    });
}
```

**Связи:** 1-1 (`HasOne().WithOne()`), 1-N (`HasOne().WithMany()`), N-N (`HasMany().WithMany()` — EF Core 5+ создаёт join-таблицу сам; если в связке нужны свои поля, например дата, — делают явную сущность-связку).

**DeleteBehavior:** `Cascade` (удалить зависимые — по умолчанию для обязательных FK), `Restrict`/`NoAction` (запретить удаление, пока есть зависимые), `SetNull` (для необязательных FK).

Нарушение уникального индекса, FK или check constraint → `SaveChanges` бросает `DbUpdateException` (внутри — исключение провайдера, например `SqliteException`).

> **Почему так?** Ограничения (unique, FK, check) — последняя линия защиты инвариантов: код можно обойти (другой сервис, ручной SQL, гонка двух запросов), а базу — нет. Проверка «есть ли такой ISBN» в C# не спасёт от двух одновременных запросов — уникальный индекс спасёт. Fluent API держит схему в одном месте и не засоряет сущности атрибутами.

> 💼 **На собеседовании:** «Как настроить связь многие-ко-многим?», «Что будет при удалении автора с книгами?» — зависит от DeleteBehavior; по умолчанию для обязательной связи — каскадное удаление, что часто опасно.

→ **Задача 1** (см. exercises.md)

---

## 5. Миграции

**Миграция** — C#-файл с описанием изменения схемы (создать таблицу, добавить столбец) относительно предыдущего состояния модели. Это «git для схемы БД».

```bash
dotnet tool install --global dotnet-ef               # один раз: CLI для EF Core
dotnet ef migrations add AddBookIsbn                 # сравнить модель со снимком → создать миграцию
dotnet ef database update                            # применить все неприменённые миграции к БД
dotnet ef migrations script --idempotent -o deploy.sql  # SQL-скрипт для продакшена (DBA может проверить)
dotnet ef migrations remove                          # удалить последнюю (ещё не применённую) миграцию
```

(Для команд в проекте нужен пакет `Microsoft.EntityFrameworkCore.Design`.) В БД хранится таблица `__EFMigrationsHistory` — какие миграции уже применены.

**`EnsureCreated()`** создаёт схему по текущей модели **без миграций** — подходит для тестов и прототипов (мы используем его в тестах с SQLite in-memory), но несовместим с миграциями: изменить схему потом нельзя.

> **Почему так?** Схема БД должна версионироваться вместе с кодом: каждый разработчик и каждое окружение (dev, test, prod) воспроизводимо приходит к той же схеме. Сгенерированную миграцию **обязательно читают** перед коммитом: переименование свойства EF может понять как «удалить столбец + создать новый» — с потерей данных.

> 💼 **На собеседовании:** «Что такое миграции и как ты их применяешь в продакшене?» — `migrations add` → ревью → idempotent SQL-скрипт или `database update` в pipeline деплоя; не `Database.Migrate()` при старте нескольких экземпляров приложения одновременно (гонка).

---

## 6. LINQ → SQL: IQueryable vs IEnumerable (снова)

`IQueryable<T>` хранит **expression tree** (дерево выражения — описание запроса как данных), которое провайдер EF переводит в SQL. `IEnumerable<T>` — уже объекты в памяти, LINQ to Objects.

```csharp
IQueryable<Book> q = db.Books.Where(b => b.PublishedYear > 2000);    // SQL ещё не выполнен
var page = await q.OrderBy(b => b.Title).Skip(20).Take(10).ToListAsync(); // один SQL с WHERE, ORDER BY, LIMIT/OFFSET

IEnumerable<Book> all = db.Books;                                    // ⚠️ тип переменной — IEnumerable!
var bad = all.Where(b => b.PublishedYear > 2000).Take(10).ToList();  // SELECT * FROM Books — вся таблица в память
```

Запрос выполняется на «материализующих» операторах: `ToList(Async)`, `First(OrDefault)(Async)`, `Single`, `Count`, `Any`, `foreach`. Методы, которые EF не умеет перевести (свой C#-метод внутри `Where`), дают исключение «could not be translated» — EF Core 3+ не делает молча client evaluation (кроме финального `Select`).

Приёмы:
- Фильтры добавляй условно к `IQueryable` — все попадут в один `WHERE`.
- **Проекция** (`Select(b => new BookDto(...))`) тянет только нужные колонки.
- `OrderBy` **до** `Skip/Take` — иначе порядок не определён.

> **Почему так?** Деревья выражений позволяют одной и той же LINQ-записи работать и с памятью, и с БД. Но граница «где выполняется код» становится невидимой: случайный `AsEnumerable()` или переменная типа `IEnumerable` переносит фильтрацию в память — тесты на 10 строках проходят, продакшен с миллионом строк падает.

> 💼 **На собеседовании:** «IEnumerable vs IQueryable?» — где выполняется запрос: в памяти vs в источнике данных (SQL); IQueryable строит expression tree; пример с `Take(10)`, который превращается в `LIMIT` только на IQueryable.

→ **Задача 7**

---

## 7. Change tracking и AsNoTracking

Когда EF загружает сущность обычным запросом, он регистрирует её в **change tracker** и запоминает исходные значения. При `SaveChanges` сравнивает текущие и исходные значения и генерирует `UPDATE` только изменённых колонок.

```csharp
var book = await db.Books.FirstAsync(b => b.Id == 1);  // tracked, State = Unchanged
book.Title = "New";                                    // State = Modified (обнаружится при SaveChanges)
await db.SaveChangesAsync();                           // UPDATE "Books" SET "Title" = @p0 WHERE "Id" = @p1

var list = await db.Books.AsNoTracking().ToListAsync(); // НЕ отслеживаются
list[0].Title = "Changed";
await db.SaveChangesAsync();                           // ничего не произойдёт!
```

Состояния сущности (`EntityState`): `Added`, `Unchanged`, `Modified`, `Deleted`, `Detached`. `db.Add(x)` → Added, `db.Remove(x)` → Deleted, `db.Update(x)` / `db.Attach(x)` — подключить «чужой» (detached) объект.

**AsNoTracking** — для запросов «только чтение» (списки, отчёты, GET-эндпоинты): нет затрат на снимки и регистрацию, меньше памяти. Можно сделать поведением по умолчанию: `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)`.

Для массовых изменений без загрузки сущностей (EF Core 7+): `ExecuteUpdateAsync`/`ExecuteDeleteAsync` — один SQL `UPDATE ... WHERE`, change tracker не участвует.

> **Почему так?** Tracking — это удобство (меняешь свойства — EF сам пишет UPDATE) за счёт памяти и CPU. Для чтения оно не нужно. Классическая ошибка: загрузить через AsNoTracking, изменить и удивляться, что SaveChanges ничего не сохранил.

> 💼 **На собеседовании:** «Что такое AsNoTracking и когда его использовать?» — read-only запросы; быстрее; изменения таких объектов не сохраняются без `Attach`/`Update`.

→ **Задача 5**

---

## 8. Загрузка связанных данных и N+1

Навигационные свойства **не загружаются сами** (lazy loading в EF Core выключен по умолчанию). Способы загрузки:

| Способ | Код | Когда |
|---|---|---|
| **Eager loading** | `.Include(b => b.Author).ThenInclude(a => a.Country)` | знаешь заранее, что данные нужны |
| **Explicit loading** | `await db.Entry(book).Collection(b => b.Loans).LoadAsync()` | догрузить по условию |
| **Lazy loading** | пакет Proxies + `virtual` навигации — загрузка при первом обращении | редко; опасно N+1 |
| **Проекция** | `.Select(b => new { b.Title, AuthorName = b.Author.Name })` | лучший вариант для чтения |

**Проблема N+1:** один запрос за списком + по одному запросу на каждый элемент списка.

```csharp
var authors = await db.Authors.ToListAsync();               // 1 запрос
foreach (var a in authors)
{
    var count = await db.Books.CountAsync(b => b.AuthorId == a.Id);  // + N запросов
}
// 1000 авторов → 1001 запрос. Каждый — сетевой round-trip к БД.

var stats = await db.Authors                                  // ✅ 1 запрос
    .Select(a => new { a.Name, Count = a.Books.Count })
    .ToListAsync();
```

Как найти: включить логирование SQL и посмотреть, сколько команд уходит:

```csharp
options.UseSqlite(connection).LogTo(Console.WriteLine, LogLevel.Information);
// ещё полезно: .EnableSensitiveDataLogging() — показывать значения параметров (только в разработке!)
```

**Cartesian explosion:** `Include` нескольких коллекций в одном запросе даёт произведение строк (10 жанров × 100 выдач = 1000 строк на книгу). Решение — `.AsSplitQuery()` (несколько запросов) или проекция.

> **Почему так?** Каждый запрос к БД — это сетевой round-trip (миллисекунды), и на сотнях элементов N+1 превращает 20-мс эндпоинт в многосекундный. Lazy loading делает N+1 невидимым (обычное обращение к свойству = запрос), поэтому в EF Core его выключили по умолчанию — явный `Include`/проекция лучше видны на ревью.

> 💼 **На собеседовании:** «Что такое проблема N+1 и как её решить в EF Core?» — один из самых частых вопросов. Ответ: определение + пример цикла + решения (Include, проекция, батч-запрос) + как обнаружить (логи SQL, профайлер).

→ **Задача 2**, **Задача 6**

---

## 9. Агрегаты и группировка в EF Core

```csharp
var top = await db.Authors
    .Select(a => new { a.Name, Loans = db.Loans.Count(l => l.Book.AuthorId == a.Id) })
    .Where(x => x.Loans > 0)
    .OrderByDescending(x => x.Loans)
    .Take(3)
    .ToListAsync();

var byYear = await db.Books
    .GroupBy(b => b.PublishedYear)
    .Select(g => new { Year = g.Key, Count = g.Count() })   // GROUP BY "PublishedYear"
    .ToListAsync();
```

Правило: агрегаты (`Count`, `Sum`, `Max`) считай в запросе, а не `ToList()` + LINQ в памяти. Сортировку после проекции делай по анонимному типу; проекцию в record с конструктором — последним шагом (EF не умеет «заглянуть» в параметры конструктора для последующего `OrderBy`).

> **Почему так?** Загрузить 100 000 выдач, чтобы посчитать одно число, — это гигабайты трафика вместо одной строки результата. БД считает агрегаты рядом с данными и использует индексы.

→ **Задача 3**

---

## 10. Транзакции и конкурентный доступ

`SaveChanges` **уже** выполняется в транзакции: все INSERT/UPDATE/DELETE одного вызова — атомарны. Явная транзакция нужна, когда операция состоит из **нескольких** шагов: прочитать → проверить → записать, или несколько `SaveChanges`:

```csharp
await using var tx = await db.Database.BeginTransactionAsync();
var active = await db.Loans.CountAsync(l => l.BookId == bookId && l.ReturnedOn == null);
if (active >= book.CopiesTotal) throw new LibraryRuleException("Нет свободных экземпляров");
db.Loans.Add(new Loan { ... });
await db.SaveChangesAsync();
await tx.CommitAsync();   // без Commit → при Dispose транзакция откатится
```

**Уровни изоляции** (isolation levels): Read Uncommitted, Read Committed (по умолчанию в SQL Server/PostgreSQL), Repeatable Read, Serializable. Чем выше — тем меньше аномалий (грязное чтение, неповторяемое чтение, фантомы), но больше блокировок. На Read Committed два параллельных «проверил → записал» могут оба пройти проверку — для этого используют Serializable, блокировку строки или **optimistic concurrency**:

```csharp
[Timestamp] public byte[] RowVersion { get; set; } = [];  // SQL Server rowversion — concurrency token
// UPDATE ... WHERE Id = @id AND RowVersion = @original → 0 строк → DbUpdateConcurrencyException
```

> **Почему так?** Транзакция гарантирует, что бизнес-инвариант («выдано не больше, чем экземпляров») не нарушится частично выполненной операцией. Optimistic concurrency не держит блокировки, а обнаруживает конфликт при записи — хорошо работает, когда конфликты редки (типичный веб).

> 💼 **На собеседовании:** «Нужна ли транзакция вокруг одного SaveChanges?» — нет, он уже атомарен. «Как обработать одновременное редактирование одной записи двумя пользователями?» — optimistic concurrency (rowversion/concurrency token) → `DbUpdateConcurrencyException` → показать пользователю конфликт или повторить.

→ **Задача 4**

---

## 11. Тестирование с SQLite in-memory

```csharp
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();                                   // база живёт, пока открыто соединение
var options = new DbContextOptionsBuilder<LibraryDbContext>().UseSqlite(connection).Options;
using (var db = new LibraryDbContext(options)) db.Database.EnsureCreated();
```

**EF InMemory-провайдер** (`UseInMemoryDatabase`) — не реляционная БД: не проверяет уникальные индексы, FK, транзакции, не выполняет SQL. Microsoft не рекомендует его для тестов. SQLite in-memory — настоящая реляционная БД, но с отличиями от SQL Server/PostgreSQL (типы, `lower()` только для ASCII, нет `DateTimeOffset`-сравнений). Самый честный вариант — та же СУБД, что в продакшене, в Docker (Testcontainers).

> **Почему так?** Тест должен падать там же, где упадёт продакшен. Если тестовая «БД» не знает про уникальные индексы, тест на дубликат ISBN пройдёт, а продакшен — нет.

---

## 12. Dapper (упоминание)

**Dapper** — micro-ORM: пишешь SQL сам, Dapper только маппит результат на объекты.

```csharp
var books = await connection.QueryAsync<BookDto>(
    "SELECT Id, Title FROM Books WHERE PublishedYear >= @Year", new { Year = 2010 });
```

Используют для сложных отчётов и горячих путей, где важен контроль над SQL. Параметры (`@Year`) — обязательно: конкатенация строк в SQL = **SQL injection**. EF Core параметризует запросы автоматически (в том числе `FromSql($"... {value}")` — интерполяция там безопасна, а вот `FromSqlRaw` с конкатенацией — нет).

> **Почему так?** У Dapper нет change tracker и генерации SQL — поэтому он почти так же быстр, как «голый» ADO.NET, но и всю работу по написанию SQL и следованию схеме делаешь ты. Параметры — не только защита от injection: БД кэширует план запроса для одинакового текста SQL, а вклеенные значения делают каждый текст уникальным.

---

## Типичные ошибки

| Ошибка | Последствие | Как правильно |
|---|---|---|
| Запрос в цикле (N+1) | сотни round-trip'ов, медленный API | Include / проекция / один запрос с `Contains` |
| `ToList()` до `Where`/`Take` или переменная `IEnumerable` | вся таблица в память | держать `IQueryable` до материализации |
| `Skip/Take` без `OrderBy` | страницы «перемешиваются», дубли | `OrderBy(...).ThenBy(Id)` перед пагинацией |
| Изменение сущности из `AsNoTracking` + `SaveChanges` | изменения молча не сохраняются | tracking-запрос или `Update/Attach` |
| `DbContext` как Singleton / общий между потоками | исключения, устаревшие данные | Scoped / короткая жизнь |
| Проверка уникальности только в C# | дубликаты при параллельных запросах | уникальный индекс + обработка `DbUpdateException` |
| Каскадное удаление по умолчанию | удалил автора — пропала история выдач | осознанный `DeleteBehavior` |
| Миграция не прочитана | потеря данных при «переименовании» | ревью миграции, при необходимости `RenameColumn` |
| Конкатенация строк в SQL | SQL injection | параметры |
| `DateTime.Now` внутри запроса/сервиса | нетестируемо | дата параметром или `TimeProvider` |

## Шпаргалка

```csharp
// Чтение
await db.Books.AsNoTracking().Where(b => b.Year > 2000).OrderBy(b => b.Title).ToListAsync();
await db.Books.Include(b => b.Author).ThenInclude(a => a.Country).FirstOrDefaultAsync(b => b.Id == id);
await db.Books.Select(b => new BookDto(b.Id, b.Title, b.Author.Name)).ToListAsync();   // проекция
await db.Books.CountAsync(b => b.AuthorId == id);  await db.Books.AnyAsync(...);
// Пагинация
query.OrderBy(b => b.Title).ThenBy(b => b.Id).Skip((page - 1) * size).Take(size);
// Запись
db.Books.Add(book); book.Title = "x"; db.Books.Remove(book); await db.SaveChangesAsync();
await db.Books.Where(b => b.Year < 1900).ExecuteDeleteAsync();             // EF Core 7+
// Транзакция
await using var tx = await db.Database.BeginTransactionAsync(); ...; await tx.CommitAsync();
// Логи SQL
options.UseSqlite(conn).LogTo(Console.WriteLine, LogLevel.Information);
```

```bash
dotnet ef migrations add <Name>    dotnet ef database update    dotnet ef migrations script --idempotent
```

## Глоссарий

| Термин | Что значит |
|---|---|
| Primary key / Foreign key | уникальный идентификатор строки / ссылка на PK другой таблицы |
| Index | структура для быстрого поиска по столбцам; unique index — гарантия уникальности |
| Transaction, ACID | «всё или ничего»; атомарность, согласованность, изоляция, долговечность |
| Normalization | хранение каждого факта в одном месте, без дублирования |
| ORM | отображение таблиц на объекты и LINQ на SQL |
| DbContext | unit of work: change tracker + DbSet-ы + SaveChanges |
| Change tracker | механизм EF, отслеживающий состояние загруженных сущностей |
| Navigation property | свойство-ссылка на связанную сущность/коллекцию |
| Eager / explicit / lazy loading | загрузка связей заранее (Include) / по требованию явно / при обращении |
| N+1 problem | 1 запрос за списком + N запросов за деталями каждого элемента |
| Projection | `Select` в DTO/анонимный тип — только нужные колонки |
| Migration | версионируемое изменение схемы БД |
| Expression tree | представление лямбды как данных, которое провайдер переводит в SQL |
| Optimistic concurrency | обнаружение конфликта при записи по concurrency token (rowversion) |

## Вопросы на собеседовании

<details><summary><b>Что такое DbContext и какой у него lifetime?</b></summary>

DbContext — реализация паттерна unit of work: содержит DbSet-ы, change tracker и `SaveChanges`, который одной транзакцией сохраняет все изменения. Он короткоживущий и не потокобезопасный, поэтому в ASP.NET Core регистрируется как Scoped (один на HTTP-запрос). Singleton-контекст — ошибка: гонки, устаревшие данные, растущий change tracker.
</details>

<details><summary><b>Что такое проблема N+1 и как её решить?</b></summary>

Это когда загружаем список одним запросом, а потом для каждого элемента делаем ещё по запросу (часто незаметно — через lazy loading или запрос в цикле). 1000 элементов — 1001 round-trip к БД. Решения: eager loading (`Include`), проекция (`Select` с подзапросами — один SQL), загрузка всех деталей одним запросом через `Where(x => ids.Contains(x.Id))`. Обнаруживают по логам SQL (`LogTo`) или профайлером.
</details>

<details><summary><b>IEnumerable vs IQueryable — в чём разница для EF Core?</b></summary>

`IQueryable` хранит expression tree, которое провайдер переводит в SQL, — фильтры, сортировка, `Take` выполняются в БД. `IEnumerable` работает с объектами в памяти. Если вызвать `Where` на `IEnumerable` (например, после `ToList()` или через переменную типа `IEnumerable<T>`), сначала загрузится вся таблица, а фильтрация пройдёт в памяти.
</details>

<details><summary><b>Что делает AsNoTracking и когда его использовать?</b></summary>

Отключает регистрацию сущностей в change tracker: нет снимков исходных значений, быстрее и экономнее по памяти. Используют для read-only запросов (списки, отчёты, GET). Изменения таких объектов `SaveChanges` не увидит — нужно `Update`/`Attach` или загрузить заново с tracking.
</details>

<details><summary><b>Что такое миграции? Чем `EnsureCreated` отличается от `Migrate`?</b></summary>

Миграции — версионируемые изменения схемы, генерируются `dotnet ef migrations add` по разнице модели и снимка, применяются `dotnet ef database update` или SQL-скриптом; применённые записываются в `__EFMigrationsHistory`. `EnsureCreated` создаёт схему по текущей модели без миграций (для тестов/прототипов) и не умеет её потом менять; `Migrate()` применяет миграции. Сгенерированную миграцию нужно читать перед коммитом.
</details>

<details><summary><b>Какие способы загрузки связанных данных есть в EF Core?</b></summary>

Eager loading — `Include/ThenInclude` (данные в том же запросе), explicit loading — `Entry(x).Collection(...).LoadAsync()`, lazy loading — через прокси и `virtual`-навигации, загрузка при обращении (по умолчанию выключено, риск N+1). Для чтения часто лучше проекция в DTO — только нужные колонки. При нескольких `Include` коллекций возможен cartesian explosion → `AsSplitQuery`.
</details>

<details><summary><b>Нужна ли транзакция вокруг SaveChanges? Когда нужна явная?</b></summary>

Один `SaveChanges` уже атомарен — все его команды выполняются в транзакции. Явная транзакция (`BeginTransactionAsync`) нужна, когда операция включает несколько `SaveChanges` или «прочитать → проверить → записать», и инвариант должен держаться для всей последовательности. Для конкурентного доступа дополнительно думают об уровне изоляции или optimistic concurrency.
</details>

<details><summary><b>Что такое индекс? Почему не создать индексы на все столбцы?</b></summary>

Индекс — отдельная структура (обычно B-дерево), позволяющая находить строки по значению столбца за O(log n) без полного сканирования. Индексы ускоряют чтение (WHERE, JOIN, ORDER BY), но замедляют запись (каждый INSERT/UPDATE обновляет все индексы) и занимают место. Поэтому индексируют столбцы из частых фильтров и FK; уникальный индекс ещё и гарантирует уникальность.
</details>

<details><summary><b>INNER JOIN vs LEFT JOIN, WHERE vs HAVING?</b></summary>

INNER JOIN возвращает только строки с совпадением в обеих таблицах; LEFT JOIN — все строки левой таблицы, а несовпавшие поля правой — NULL (например, авторы без книг). WHERE фильтрует строки до группировки, HAVING — группы после GROUP BY по значениям агрегатов.
</details>

<details><summary><b>Как обработать одновременное изменение записи двумя пользователями?</b></summary>

Optimistic concurrency: добавить concurrency token (`[Timestamp]`/rowversion или `IsConcurrencyToken`). EF добавит в UPDATE условие на исходное значение токена; если запись успели изменить, обновится 0 строк и будет `DbUpdateConcurrencyException`. Дальше — сообщить пользователю о конфликте или перечитать и повторить. Pessimistic (блокировки) — реже, когда конфликты часты.
</details>

<details><summary><b>Как EF Core защищает от SQL injection?</b></summary>

LINQ-запросы всегда параметризуются: значения передаются как параметры, а не вклеиваются в текст SQL. `FromSql($"...{x}")` тоже превращает интерполяцию в параметры. Опасно — `FromSqlRaw`/`ExecuteSqlRaw` с конкатенацией пользовательского ввода.
</details>

## Дополнительно

- [EF Core documentation](https://learn.microsoft.com/ef/core/) — начни с «Getting Started» и «Creating a Model».
- [Loading related data](https://learn.microsoft.com/ef/core/querying/related-data/) и [Single vs split queries](https://learn.microsoft.com/ef/core/querying/single-split-queries).
- [Tracking vs No-Tracking Queries](https://learn.microsoft.com/ef/core/querying/tracking).
- [Migrations overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations/).
- [Testing against your production database system](https://learn.microsoft.com/ef/core/testing/) — почему не InMemory-провайдер.
- [Efficient querying](https://learn.microsoft.com/ef/core/performance/efficient-querying).
- Книга: Jon P Smith, «Entity Framework Core in Action».
- SQL-тренажёры: https://sqlbolt.com, https://www.sql-ex.ru.
