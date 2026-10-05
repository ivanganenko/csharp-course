# Модуль 04 — задачи

> Код пишется в `starter/Module04.Tasks/`. Тесты: `cd modules/04-oop-classes/starter && dotnet test`.
>
> Сигнатуры `public`-членов (имена, параметры, типы) не меняй — по ним работают тесты. Внутри класса можно добавлять любые `private`-поля и методы, а выражения `=> throw new NotImplementedException()` заменять на auto-properties (`{ get; private set; }` и т.п.).

## Задача 1. Банковский счёт с инвариантами  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** реализуй класс `BankAccount`: владелец, баланс, пополнение, снятие, перевод на другой счёт и история операций. Точные правила — в XML-комментариях в файле. Главное:
- баланс **никогда** не бывает отрицательным;
- при любой ошибке (исключении) состояние счёта (и второго счёта при переводе) не меняется;
- историю нельзя изменить снаружи — даже приведением типа `(IList<Transaction>)account.History`.

**Зачем:** это классическая задача на инкапсуляцию, её дают и на live coding. Учишься выбирать между `{ get; }`, `{ get; private set; }`, полем `private readonly`, и защищать внутренние коллекции.
**Файл:** `Module04.Tasks/BankAccount.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~BankAccountTests"`
**Подумай / ответь письменно:**
1. Почему `Balance` — свойство с `private set`, а не публичное поле? Что сломается, если сделать `public set`?
2. Почему в `TransferTo` важен порядок `Withdraw` → `Deposit`, а не наоборот?

## Задача 2. Температура с валидацией  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `Temperature`: хранится одно значение в °C (private backing field), `Fahrenheit` — вычисляемое свойство с `get` и `set`, `Kelvin` — только `get`. Валидация (не ниже абсолютного нуля, не `NaN`) — **в одном месте**. Плюс фабрики `FromFahrenheit`/`FromKelvin` и `ToString()` в формате `"21.5 °C"` независимо от языка Windows.

**Зачем:** свойство с логикой в setter'е, computed properties, static factory methods, `CultureInfo.InvariantCulture`. На собеседовании: «зачем свойства, если есть поля?» — вот ответ в коде.
**Файл:** `Module04.Tasks/Temperature.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~TemperatureTests"`
**Подумай / ответь письменно:**
1. Почему мы храним только °C, а не три поля (°C, °F, K)?
2. Почему `double.NaN < -273.15` — `false`, и чем это опасно для валидации?

## Задача 3. Карточка книги: `required`, `init`, вычисляемые свойства  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `Book`. Объект создаётся только через object initializer:
```csharp
var book = new Book { Title = "The Silent Compiler", Author = "A. Petrova", Year = 2021, Isbn = "978-5-00000-000-1" };
```
`Title`/`Author` — `required`, все свойства — `init` (после создания не меняются). В init-аксессорах — валидация и нормализация (Trim, очистка ISBN). `Description` — вычисляемое.

**Зачем:** современный C#-стиль создания объектов без конструктора на 6 параметров; понимание разницы `set`/`init`/get-only.
**Файл:** `Module04.Tasks/Book.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~BookTests"`
**Мини-эксперимент:** в любом тесте убери `Title = ...` из initializer'а и собери проект — прочитай ошибку CS9035. Затем попробуй после создания написать `book.Year = 2000;` — какая ошибка? Верни всё обратно.
**Подумай / ответь письменно:**
1. Чем `required` лучше проверки «Title не задан → исключение» в runtime?
2. Почему для хранения нужны `private readonly`-поля, а не auto-property `{ get; init; }`?

## Задача 4. Заявки поддержки: static-счётчик  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуй `SupportTicket`: каждая новая заявка получает `Id` на 1 больше предыдущей; `TotalCreated` — сколько заявок создано; `Number` — `"TCK-0007"`. Невалидный заголовок **не должен** увеличивать счётчик.

**Зачем:** static-поля и свойства, `const` vs `static readonly`, порядок «проверь → измени». Бонус: потокобезопасный инкремент.
**Файл:** `Module04.Tasks/SupportTicket.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SupportTicketTests"`
**Подумай / ответь письменно:**
1. Почему `MaxTitleLength` — `const`, а `Prefix` — `static readonly`? Что будет, если другая сборка использует `const`, а ты поменяешь значение?
2. Что может пойти не так с `_lastId++`, если заявки создаются из нескольких потоков одновременно? (Подсказка: `Interlocked`.)

## Задача 5. Extension methods для строк  ·  ⏱ ~30 мин  ·  🟢 базовая

**Что сделать:** реализуй в `StringExtensions` четыре метода расширения: `IsNullOrBlank`, `Truncate`, `WordCount`, `Mask` (маскирование номера карты: `"************5678"`).

**Зачем:** extension methods — основа LINQ, ими постоянно пользуются в реальном коде; на собеседовании спрашивают, как они устроены и почему их можно вызвать на `null`.
**Файл:** `Module04.Tasks/StringExtensions.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~StringExtensionsTests"`
**Подумай / ответь письменно:**
1. Почему `((string?)null).IsNullOrBlank()` не бросает `NullReferenceException`, а `((string?)null).Trim()` — бросает?
2. Что произойдёт, если в `string` когда-нибудь добавят свой метод `Truncate(int)`?

## Задача 6. Исследование: модификаторы доступа, `partial`, nested types  ·  ⏱ ~30 мин  ·  🧪 исследование

**Что сделать:** без тестов. Создай во временной папке (вне курса) решение из двух проектов:
```bash
dotnet new classlib -n Lib
dotnet new console -n App
dotnet add App reference Lib
```
1. В `Lib` создай `public class Base` с членами всех шести уровней доступа (`public`, `private`, `protected`, `internal`, `protected internal`, `private protected`).
2. Попробуй обратиться к каждому члену: (а) из другого класса в `Lib`; (б) из наследника в `Lib`; (в) из класса в `App`; (г) из наследника в `App`. Заполни таблицу «доступно / ошибка компиляции».
3. Добавь в `Lib` `public const int Version = 1;` и `public static readonly int ReadonlyVersion = 1;`, выведи оба в `App`. Поменяй оба на `2`, пересобери **только** `Lib` (`dotnet build Lib`), скопируй новую `Lib.dll` в папку вывода `App` и запусти `App.dll` без пересборки. Что вывелось и почему?
4. Разбей любой класс на два файла через `partial`. Сделай private nested class и попробуй обратиться к нему снаружи.

**Зачем:** вопрос про `protected internal` vs `private protected` и про ловушку `const` — любимые «проверки на глубину».
**Подумай / ответь письменно:** объясни таблицу из п.2 и результат п.3 своими словами — так, как ответил бы на собеседовании.

## Задача 7. Money — value object  ·  ⏱ ~60 мин  ·  🔴 сложная  ·  ⭐ со звёздочкой

**Что сделать:** реализуй неизменяемый `Money` (сумма `decimal` + код валюты): нормализация и валидация в конструкторе, округление «от нуля», `Add`/`Subtract`/`Multiply` возвращают **новый** объект, нельзя смешивать валюты, `Allocate(n)` делит сумму на `n` частей **без потери центов** (100 / 3 → 33.34 + 33.33 + 33.33), `ToString()` → `"5.00 EUR"`.

**Зачем:** value object — частая тема на собеседованиях и live coding («реализуйте класс Money»). Ловушки: `double` для денег, банковское округление, потеря центов при делении, смешивание валют, мутабельность. В модуле 06 мы превратим его в `readonly record struct` с операторами `+`, `-`, `==`.
**Файл:** `Module04.Tasks/Money.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~MoneyTests"`
**Подумай / ответь письменно:**
1. Почему `Money` — immutable? Приведи пример бага, который возможен с изменяемым `Money`.
2. Что вернёт `new Money(1, "USD") == new Money(1, "USD")` и почему? Как это исправить (заглянем в модуль 06)?
3. Почему `Allocate` считает в целых центах, а не делит `Amount / parts`?
