# Модуль 16 — задачи

> Код пишется в `starter/Module16.Tasks/`. Тесты: `cd modules/16-efcore-sql/starter && dotnet test`.

**Проект «Библиотека».** Готово: сущности `Author`, `Book`, `Genre`, `Member`, `Loan` (`Entities/Entities.cs`), `LibraryDbContext` с DbSet-ами, модели результатов и исключения (`Services/Models.cs`). `LibraryService` разбит на partial-файлы — по одному на задачу. Тесты используют SQLite in-memory (`Module16.Tests/TestDatabase.cs`) и общий набор данных `LibraryData` — прочитай его комментарий, там вся «библиотека» в одной таблице.

Совет: для отладки создай контекст с логом — `_database.CreateContext(Console.WriteLine)` — и смотри, какой SQL генерирует твой код.

---

## Задача 1. Конфигурация модели (Fluent API)  ·  ⏱ ~35 мин  ·  🟢 базовая

**Что сделать:** в `LibraryDbContext.OnModelCreating` настрой длины строк, уникальные индексы (`Book.Isbn`, `Genre.Name`, `Member.Email`), check constraint на `CopiesTotal >= 0` и `DeleteBehavior.Restrict` для связей Book→Author, Loan→Book, Loan→Member. Подробности — в XML-комментарии.
**Зачем:** модель данных — основа приложения; ограничения в БД — последняя линия защиты инвариантов. «Как сделать поле уникальным?» и «что будет при удалении родителя?» — частые вопросы.
**Файл:** `Module16.Tasks/Data/LibraryDbContext.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ModelConfigurationTests"`
**Подумай / ответь письменно:**
1. Почему уникальность ISBN проверяется индексом в БД, а не только `if (await db.Books.AnyAsync(b => b.Isbn == isbn))` в коде?
2. Какой `DeleteBehavior` EF выбрал бы по умолчанию для `Book → Author` и чем это опасно?

## Задача 2. Include / ThenInclude  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `GetBookWithDetailsAsync`, `GetAuthorWithBooksAsync`, `GetActiveLoansForMemberAsync` в `LibraryService.Queries.cs`.
**Зачем:** без явной загрузки навигационные свойства пусты — новички получают `NullReferenceException` и «чинят» его lazy loading'ом, создавая N+1.
**Файл:** `Module16.Tasks/Services/LibraryService.Queries.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~QueryWithIncludeTests"`
**Подумай / ответь письменно:**
1. Почему тесты создают для запроса НОВЫЙ контекст, а не используют тот, которым заполняли базу? Что такое relationship fixup?
2. Посмотри SQL для `GetActiveLoansForMemberAsync` (через лог): сколько JOIN и почему?

## Задача 3. Агрегаты в базе  ·  ⏱ ~35 мин  ·  🟡 средняя

**Что сделать:** `GetTopAuthorsAsync(top)` и `GetOverdueReportAsync(today)` в `LibraryService.Statistics.cs`. Каждый метод — **один** SQL-запрос, считающий агрегаты в БД.
**Зачем:** отчёты и статистика — повседневная задача бэкенда; на собеседовании любят «посчитай количество X для каждого Y» — и в SQL, и в LINQ.
**Файл:** `Module16.Tasks/Services/LibraryService.Statistics.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~StatisticsTests"`
**Подумай / ответь письменно:**
1. Напиши эквивалентный SQL для `GetOverdueReportAsync` (с JOIN/GROUP BY/HAVING).
2. Почему `today` передаётся параметром, а не берётся `DateOnly.FromDateTime(DateTime.Today)` внутри метода?

## Задача 4. Выдача книги в транзакции  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** `LendBookAsync` (5 проверок в заданном порядке, транзакция) и `ReturnBookAsync` в `LibraryService.Lending.cs`.
**Зачем:** бизнес-операция с инвариантами — классика собеседования и реальной работы («перевод денег», «бронирование места», «выдача книги»).
**Файл:** `Module16.Tasks/Services/LibraryService.Lending.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~LendingTests"`
**Подумай / ответь письменно:**
1. Зачем транзакция, если `SaveChanges` сам по себе атомарен? Что именно она защищает?
2. Два библиотекаря одновременно выдают последний экземпляр. Всегда ли транзакция с уровнем Read Committed (SQL Server) это предотвратит? Какие есть варианты?

## Задача 5. Tracking vs AsNoTracking  ·  ⏱ ~20 мин  ·  🟢 базовая

**Что сделать:** `GetCatalogAsync` (read-only, `AsNoTracking`) и `RenameBookAsync` (с валидацией, сохраняет изменения) в `LibraryService.Catalog.cs`.
**Зачем:** «Что такое AsNoTracking?» — частый вопрос, а «почему SaveChanges ничего не сохранил?» — частый баг.
**Файл:** `Module16.Tasks/Services/LibraryService.Catalog.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~TrackingTests"`
**Подумай / ответь письменно:**
1. Что произойдёт, если в `RenameBookAsync` взять книгу из `GetCatalogAsync()`, поменять `Title` и вызвать `SaveChangesAsync`? Почему?
2. Как переименовать книгу одним SQL-запросом без загрузки сущности (EF Core 7+)?

## Задача 6. Найди и убей N+1  ·  ⏱ ~30 мин  ·  🧪 исследование

**Что сделать:**
1. Прочитай готовый `BuildAuthorReportNaiveAsync`. Напиши временный тест (или используй `NaiveReport_ExecutesOneQueryPerAuthor`), создав контекст через `_database.CreateContext(Console.WriteLine)` или `LogTo(Console.WriteLine, LogLevel.Information)`, и посмотри в выводе теста, какие SQL уходят в базу. Сколько их при 4 авторах? При 1000?
2. Реализуй `BuildAuthorReportAsync` — тот же результат за ≤ 2 SQL-команды (тест считает команды через `LogTo` с фильтром `RelationalEventId.CommandExecuted`).
**Зачем:** N+1 — вопрос из топ-3 по EF Core; важно не только назвать, но и уметь найти по логам.
**Файл:** `Module16.Tasks/Services/LibraryService.Reports.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~NPlusOneTests"`
**Подумай / ответь письменно:**
1. Сравни варианты решения: `Include(a => a.Books)` vs проекция `Select(a => new { a.Name, Titles = a.Books.Select(b => b.Title).ToList() })`. Какие колонки уходят в каждом случае?
2. Почему lazy loading делает N+1 «невидимым»?

## Задача 7. Поиск с фильтрами и пагинацией на IQueryable  ·  ⏱ ~50 мин  ·  ⭐ со звёздочкой

**Что сделать:** `SearchBooksAsync(criteria)` в `LibraryService.Search.cs`: условные фильтры на `IQueryable<Book>`, `TotalCount`, сортировка, `Skip/Take`, проекция в `BookListItem` с вычисляемым `AvailableCopies`. Посмотри итоговый SQL в логе — фильтры должны оказаться в `WHERE`, пагинация — в `LIMIT/OFFSET`.
**Зачем:** реальный эндпоинт «список с фильтрами» встречается в каждом проекте; на live coding часто просят «добавь фильтр, не загружая всю таблицу».
**Файл:** `Module16.Tasks/Services/LibraryService.Search.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~BookSearchTests"`
**Подумай / ответь письменно:**
1. Что изменится, если начать с `IEnumerable<Book> query = _db.Books;`? Тесты пройдут? Что будет в продакшене на миллионе книг?
2. Почему `OrderBy` обязателен перед `Skip/Take`? Чем плох `Skip(100000)` и что такое keyset-пагинация?
3. Почему поиск по названию без учёта регистра работает только для латиницы в SQLite?
