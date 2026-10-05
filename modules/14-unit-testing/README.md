# Модуль 14. Unit-тестирование

> **Зачем:** в командах код без тестов на code review обычно не принимают, а первая задача джуна часто звучит как «допиши тесты» или «почини упавший тест». На собеседовании спрашивают: «зачем тесты», «unit vs integration», «что такое mock и stub», «как протестировать код с `DateTime.Now`», «что такое AAA и TDD», а иногда просят написать тест прямо на интервью. **Время:** ~8 ч. **Нужно:** модули 04–05 (классы, интерфейсы), 07 (исключения), 13 (async/await).

## Что ты будешь уметь

- Объяснять, зачем нужны тесты, что такое пирамида тестов и чем unit-тест отличается от интеграционного.
- Писать тесты на xUnit: `[Fact]`, `[Theory]`, `[InlineData]`, `[MemberData]`, `Assert.*`, проверка исключений и async-кода.
- Структурировать тест по AAA и называть его так, чтобы по имени было понятно, что сломалось.
- Делать код тестируемым через dependency injection и подменять зависимости: stub, mock, fake, NSubstitute.
- Работать по циклу TDD red → green → refactor.

> **Особенность модуля:** здесь **ты пишешь тесты**. Production-код в `starter/Module14.Tasks` частично готов (его нужно покрыть тестами — и найти в нём баг), частично не реализован (его ты напишешь по TDD).

---

## 1. Зачем тесты и пирамида тестов

Автоматический тест — код, который запускает твой код и проверяет результат. Что он даёт:

- **Защита от регрессий** (regression — поломка того, что раньше работало). Поменял метод — за секунды узнал, что ничего не сломал.
- **Смелость рефакторить.** Без тестов улучшать код страшно — поэтому его и не улучшают.
- **Живая документация.** `CancelOrderAsync_WindowExpired_ThrowsAndDoesNotRefund` объясняет правило лучше комментария, и, в отличие от комментария, не устаревает молча.
- **Давление на дизайн.** Если код сложно протестировать — у него, скорее всего, плохой дизайн: скрытые зависимости, слишком много обязанностей.

**Пирамида тестов** (test pyramid):

```
        /  E2E  \          мало: медленные, хрупкие, но проверяют систему целиком (UI, браузер)
       /---------\
      /integration\        больше: реальная БД, HTTP, файловая система, несколько компонентов
     /-------------\
    /     unit      \      много: быстрые (миллисекунды), изолированные, проверяют одну единицу логики
```

| | Unit | Integration | E2E |
|---|------|-------------|-----|
| Что проверяет | метод/класс в изоляции | взаимодействие с реальной инфраструктурой | сценарий пользователя целиком |
| Зависимости | подменены (test doubles) | настоящие (БД, HTTP) | всё настоящее |
| Скорость | мс | секунды | секунды–минуты |
| Почему падает | ошибка в логике | логика, конфигурация, схема БД, сеть | что угодно |

> **Почему так?** Чем выше уровень, тем больше уверенности «работает по-настоящему», но тем дороже тест: дольше выполняется, сложнее поддерживать, по падению труднее понять причину. Поэтому основную массу логики проверяют дешёвыми unit-тестами, а интеграционными — «швы» с инфраструктурой (в модулях 15–16 ты напишешь и такие: `WebApplicationFactory`, SQLite in-memory).

> 💼 **На собеседовании:** «Unit-тест проверяет единицу логики в изоляции — зависимости подменены, тест быстрый и детерминированный. Интеграционный проверяет взаимодействие с реальными компонентами: БД, HTTP. По пирамиде unit-тестов больше всего, E2E — меньше всего».

---

## 2. xUnit: Fact, Theory, Assert

В .NET три популярных фреймворка: **xUnit** (самый распространённый в новых проектах, им тестируют сам .NET), **NUnit**, **MSTest**. Идеи у всех одинаковые.

```csharp
public class PriceCalculatorTests
{
    [Fact] // один сценарий без параметров
    public void CalculateTotal_SingleLine_ReturnsQuantityTimesPrice()
    {
        var lines = new[] { new OrderLine("BOOK-1", 2, 150m) };

        var total = PriceCalculator.CalculateTotal(lines);

        Assert.Equal(300m, total); // сначала ОЖИДАЕМОЕ, потом ФАКТИЧЕСКОЕ
    }

    [Theory] // один тест — много наборов данных
    [InlineData("1,2", 3)]
    [InlineData("", 0)]
    public void Add_ReturnsSum(string input, int expected)
        => Assert.Equal(expected, StringCalculator.Add(input));
}
```

**`[InlineData]` и `decimal`.** Аргументы атрибутов в C# — только константы примитивных типов, строки, `Type`, enum. `decimal` (и `DateTime`, `DateTimeOffset`, свои классы) туда нельзя. Решение — **`[MemberData]`** + **`TheoryData<...>`**:

```csharp
public static TheoryData<int, decimal, decimal> DiscountCases => new()
{
    { 1, 999.99m, 999.99m },
    { 1, 1000m, 900m },
};

[Theory]
[MemberData(nameof(DiscountCases))]
public void CalculateTotal_AppliesDiscount(int qty, decimal price, decimal expected) { /* ... */ }
```

Основные проверки:

```csharp
Assert.Equal(expected, actual);          Assert.NotEqual(a, b);
Assert.True(cond);  Assert.False(cond);  Assert.Null(x);  Assert.NotNull(x);
Assert.Same(a, b);                        // та же ссылка
Assert.Empty(list);  Assert.Contains(item, list);  var single = Assert.Single(list);
Assert.Equal(new[] { 1, 2 }, list);       // коллекции сравниваются поэлементно
Assert.InRange(value, low, high);
Assert.Contains("Total", text);           // подстрока
var ex = Assert.Throws<ArgumentException>(() => Calc(-1));   // ровно этот тип
Assert.ThrowsAny<ArgumentException>(() => Calc(-1));          // этот тип или наследник
await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelAsync(order));
```

Async-тест — просто `public async Task Name()`. **Никогда** `async void` (модуль 13: фреймворк не дождётся теста) и никаких `.Result` в тестах.

> **Почему так?** `Theory` убирает копипасту: правило одно, данных много, и каждый набор показывается в отчёте отдельно. `Assert.Throws<T>` проверяет **точный** тип — это защищает от ситуации, когда код бросает «что-то не то» (например, `NullReferenceException` вместо `ArgumentNullException`), а тест всё равно зелёный.

### Жизненный цикл: конструктор, IDisposable, fixtures

xUnit создаёт **новый экземпляр тест-класса для каждого теста**. Поэтому:
- общий Arrange (создание моков, SUT) — в **конструкторе** и полях;
- очистка — в `Dispose()` (класс реализует `IDisposable`) или `DisposeAsync()` (`IAsyncLifetime`);
- тесты не делят состояние — их можно запускать в любом порядке и параллельно.

Если что-то дорогое нужно **один раз на класс** (поднять тестовую БД) — `IClassFixture<T>`: xUnit создаст `T` один раз и передаст в конструктор. На несколько классов — `ICollectionFixture<T>` + `[Collection("...")]`.

```csharp
public class DatabaseFixture : IDisposable { /* создать БД */ public void Dispose() { /* удалить */ } }

public class RepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    [Fact] public void Save_Works() { /* используем db */ }
}
```

> **Почему так?** Изоляция тестов важнее экономии: тест, который зависит от результата другого теста, «мигает» (flaky test) при изменении порядка. xUnit делает изоляцию поведением по умолчанию, а общие ресурсы — явным решением (fixture).

### Почему в курсе xUnit v2, если есть v3?

xUnit v3 (пакет `xunit.v3`) вышел в 2024–2025: тесты собираются в исполняемый файл, поддерживает Microsoft Testing Platform, есть новые возможности (`TestContext`, явная отмена). Курс использует **v2 API** (пакет `xunit` 2.9.x), потому что: подавляющее большинство существующих проектов, статей и ответов на Stack Overflow — про v2; v2 стабилен и поддерживается; атрибуты и `Assert` почти одинаковы, и переход на v3 для тебя будет минимальным.

→ **Задача 1** (см. exercises.md)

---

## 3. AAA и именование

**AAA** — Arrange / Act / Assert:
- **Arrange** — подготовить данные и зависимости;
- **Act** — **одно** действие, которое проверяем;
- **Assert** — проверить результат.

Именование: **`Method_Condition_ExpectedResult`** — `PlaceOrderAsync_PaymentDeclined_StatusPaymentFailed_NoEmail`. Когда тест упадёт в CI, по имени сразу ясно, какое правило нарушено.

Хороший unit-тест — **FIRST**: Fast (быстрый), Isolated (независимый), Repeatable (одинаковый результат всегда — никаких `DateTime.Now`, случайных чисел, сети), Self-validating (сам говорит «прошёл/упал»), Timely (пишется вместе с кодом).

Что ещё отличает хороший тест:
- одна причина упасть (одно поведение на тест; несколько `Assert` про одно поведение — нормально);
- нет логики (`if`, циклов) в тесте — иначе тест тоже надо тестировать;
- тестирует **поведение** через публичный API, а не приватные детали реализации;
- граничные значения: 0, пусто, `null`, ровно порог, порог ± 1.

> **Почему так?** Тест читают гораздо чаще, чем пишут, — обычно в момент, когда он упал и надо быстро понять, что случилось. Единая структура и говорящие имена экономят это время. А тесты на детали реализации ломаются при каждом рефакторинге, хотя поведение не менялось, — и команда перестаёт им доверять.

> 💼 **На собеседовании:** «Тест пишу по AAA: подготовка, одно действие, проверка. Имя — метод, условие, ожидаемый результат. Тест должен быть быстрым, изолированным и детерминированным, проверять поведение через публичный API, обязательно — граничные случаи».

---

## 4. Тестируемость и dependency injection

Код **нетестируем**, когда он сам создаёт или напрямую вызывает то, что тест не контролирует:

```csharp
public decimal Apply(decimal price)
{
    var now = DateTime.UtcNow;                // в тесте не выставить «пятницу 18:00»
    new SmtpClient("smtp.company.local").Send(...); // тест будет слать настоящие письма
    var rate = File.ReadAllText("rates.txt");       // зависит от файла на диске
}
```

Решение — **dependency injection** (DI, внедрение зависимостей): класс получает зависимости **снаружи**, через конструктор, в виде **интерфейсов**.

```csharp
public sealed class HappyHourDiscount(IClock clock)
{
    public decimal Apply(decimal price)
    {
        var now = clock.UtcNow;   // production: SystemClock; тест: FakeClock(пятница, 18:00)
        // ...
    }
}
```

В production объекты собирает **DI-контейнер** (модуль 15: `builder.Services.AddSingleton<IClock, SystemClock>()`), в тесте — ты сам, подставляя test doubles.

> 💡 В .NET 8+ есть встроенная абстракция времени **`TimeProvider`** (`TimeProvider.System`, а для тестов — `FakeTimeProvider` из пакета `Microsoft.Extensions.TimeProvider.Testing`). В курсе мы пишем свой `IClock`, чтобы было видно, как это устроено.

> **Почему так?** Зависимость, спрятанная внутри метода, — это «скрытый вход»: результат зависит от того, чего не видно в сигнатуре. Явная зависимость в конструкторе делает её видимой, заменяемой и тестируемой. Это же — буква D в SOLID (Dependency Inversion, модуль 17): зависеть от абстракций, а не от конкретных классов.

→ **Задача 6** (см. exercises.md)

---

## 5. Test doubles: dummy, stub, fake, mock, spy

**Test double** (тестовый дублёр, как каскадёр в кино) — объект, подменяющий реальную зависимость в тесте. Терминология Джерарда Месароша:

| Вид | Что делает | Пример |
|-----|-----------|--------|
| **Dummy** | только чтобы заполнить параметр; не используется | `null!` или пустой объект |
| **Stub** | возвращает заранее заданные ответы | `clock.UtcNow` → «10 марта 12:00» |
| **Fake** | рабочая упрощённая реализация | `InMemoryRepository` на `Dictionary` |
| **Spy** | stub, который ещё и запоминает вызовы | `FakeEmailSender` со списком `Sent` |
| **Mock** | объект, у которого проверяют **взаимодействие**: был ли вызов, с какими аргументами | «`SendAsync` вызван 1 раз с темой "Order confirmed"» |

Ключевая разница **stub vs mock**: stub — источник **входных** данных для теста (проверяем результат SUT), mock — объект, по которому проверяем **исходящее взаимодействие** (проверяем, что SUT вызвал зависимость правильно).

Самописный fake:

```csharp
public sealed class FakeEmailSender : IEmailSender
{
    public List<(string To, string Subject)> Sent { get; } = [];
    public Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        Sent.Add((to, subject));
        return Task.CompletedTask;
    }
}
```

> **Почему так?** Разные дублёры решают разные задачи. Проверка состояния (stub/fake) обычно устойчивее: тест не знает, *как* SUT достиг результата. Проверка взаимодействия (mock) нужна, когда результат — и есть вызов: письмо отправлено, деньги списаны, сообщение ушло в очередь.

→ **Задача 3** (см. exercises.md)

---

## 6. NSubstitute

Писать fake на каждый интерфейс утомительно. **Mocking-библиотеки** создают дублёров на лету. Популярные: **NSubstitute** (используем в курсе — лаконичный синтаксис), **Moq** (очень распространён в существующих проектах), FakeItEasy.

```csharp
using NSubstitute;

var gateway = Substitute.For<IPaymentGateway>();   // создать дублёра интерфейса
var email = Substitute.For<IEmailSender>();
var clock = Substitute.For<IClock>();
clock.UtcNow.Returns(new DateTimeOffset(2026, 3, 10, 12, 0, 0, TimeSpan.Zero)); // stub свойства

// stub метода: при таких аргументах вернуть такой результат
gateway.ChargeAsync(Arg.Any<string>(), Arg.Is(300m), Arg.Any<CancellationToken>())
       .Returns(Task.FromResult(PaymentResult.Success("tx-1")));

// ошибка зависимости
gateway.RefundAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
       .Returns(Task.FromException<bool>(new HttpRequestException("timeout")));

var sut = new OrderService(gateway, email, clock);
await sut.PlaceOrderAsync("buyer@example.com", lines, CancellationToken.None);

// mock: проверка взаимодействий
await email.Received(1).SendAsync(Arg.Is("buyer@example.com"), Arg.Is("Order confirmed"),
                                  Arg.Any<string>(), Arg.Any<CancellationToken>());
await email.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
await gateway.Received().ChargeAsync(Arg.Any<string>(), Arg.Is<decimal>(a => a > 0), Arg.Any<CancellationToken>());
```

Шпаргалка по аргументам: `Arg.Any<T>()` — любой; `Arg.Is(value)` — равный значению; `Arg.Is<T>(x => условие)` — по предикату. Правило: **если используешь `Arg.*` хотя бы для одного аргумента — используй для всех** (иначе NSubstitute может не понять, какой матчер к какому аргументу относится).

Не настроенный метод возвращает «пустое» значение: `0`, `false`, пустую строку/коллекцию, для `Task<T>` — завершённую задачу с таким значением.

### Что мокать, а что нет

- ✅ Мокать: **внешние границы** — сеть, почта, платёжный шлюз, часы, очереди, файловая система.
- ❌ Не мокать: свои **value objects** и чистую логику (`PriceCalculator`, `OrderLine`) — используй настоящие.
- ❌ Не мокать то, чем не владеешь, «по форме» (`HttpClient`, `DbContext`) — лучше оберни в свой интерфейс или проверь интеграционным тестом.
- ⚠ Избыточная проверка вызовов (`Received` на каждый чих) делает тесты хрупкими: они ломаются при любом рефакторинге.

> **Почему так?** Мок — это утверждение «так SUT разговаривает с миром». Чем больше таких утверждений про внутреннюю кухню, тем сильнее тест привязан к реализации. Мокаем то, что действительно является «внешним миром», а внутреннюю логику проверяем по результату.

> 💼 **На собеседовании:** «Stub подставляет данные, mock проверяет взаимодействие. Мокаю внешние зависимости — сеть, БД, почту, время. Пользуюсь NSubstitute или Moq: `Substitute.For<T>()`, `.Returns(...)`, `.Received(1)`. Чистую логику и value objects не мокаю».

→ **Задача 4**, **Задача 5** (см. exercises.md)

---

## 7. TDD: red → green → refactor

**TDD** (test-driven development, разработка через тестирование) — тест пишется **до** кода:

1. 🔴 **Red** — напиши маленький тест на следующее правило. Запусти — он должен **упасть** (и упасть по правильной причине).
2. 🟢 **Green** — напиши **минимальный** код, чтобы тест прошёл. Даже «глупый».
3. 🔵 **Refactor** — улучши код и тесты, не меняя поведения; все тесты зелёные.

Пример на ShippingCalculator:
- 🔴 `Calculate(100, 1, Standard) == 300` → 🟢 `return 300m;`
- 🔴 `Calculate(3000, 1, Standard) == 0` → 🟢 `return orderTotal >= 3000 ? 0 : 300;`
- 🔴 `Calculate(100, 5, Express) == 600` → 🟢 добавляем `switch` по типу
- 🔵 выносим магические числа в константы…

> **Почему так?** Тест, написанный первым, гарантирует, что он **может** упасть (тест, который никогда не был красным, может ничего не проверять). Маленькие шаги дают быструю обратную связь, а код получается минимальным — ровно под требования (YAGNI). Минус — дисциплина и время на старте; на практике многие пишут тесты сразу после кода, но TDD полезен для сложной логики и багфиксов («сначала тест, воспроизводящий баг»).

> 💼 **На собеседовании:** «TDD — цикл red-green-refactor: падающий тест, минимальный код для прохождения, рефакторинг. Плюсы — тесты точно проверяют поведение, дизайн получается тестируемым, есть страховка для рефакторинга. Для багфикса всегда начинаю с теста, который воспроизводит баг».

→ **Задача 2**, **Задача 7 ⭐** (см. exercises.md)

---

## 8. Code coverage — обзорно

**Code coverage** (покрытие кода) — доля строк/ветвлений, выполненных тестами.

- В шаблоне `dotnet new xunit` уже есть пакет `coverlet.collector`: `dotnet test --collect:"XPlat Code Coverage"` → отчёт `coverage.cobertura.xml`; HTML — через инструмент `reportgenerator`.
- Альтернативы: `dotnet-coverage` (инструмент Microsoft), встроенное покрытие в Rider (dotCover) и Visual Studio Enterprise.

Покрытие показывает, **что точно не протестировано**, но не доказывает, что протестировано **хорошо**: можно выполнить строку и ничего не проверить. 100% — не цель; разумный ориентир для бизнес-логики — 70–90%, а важнее — покрытие граничных случаев и ветвлений.

> **Почему так?** Метрика, ставшая целью, перестаёт быть хорошей метрикой (закон Гудхарта): требование «100% покрытия» рождает тесты без Assert. Покрытие полезно как фонарик — найти непроверенные ветки, — а не как оценка качества.

---

## Типичные ошибки

1. **Тест без Assert** или с `Assert.True(true)` — «зелёный», но ничего не проверяет.
2. **Перепутаны expected и actual** в `Assert.Equal` — сообщение об ошибке вводит в заблуждение.
3. **Зависимость от времени, случайности, сети, порядка тестов** — flaky tests.
4. **`async void` тесты** и `.Result` внутри тестов.
5. **Логика в тестах** (`if`, циклы, вычисление ожидаемого значения той же формулой, что в коде).
6. **Тест на всё сразу** — 15 Assert про разные правила; при падении непонятно, что сломалось.
7. **Мокают всё подряд**, включая value objects и сам SUT; проверяют каждый вызов — тесты ломаются от рефакторинга.
8. **Нет граничных случаев**: только «счастливый путь» со значением из середины диапазона.
9. **Изменение теста, чтобы он стал зелёным**, вместо исправления кода.
10. **`decimal` в `[InlineData]`** — не компилируется; используй `TheoryData`/`MemberData`.
11. **Смешивание `Arg.Any` и обычных значений** в одном вызове NSubstitute.

## Шпаргалка

```csharp
// xUnit
[Fact] public void M_Condition_Expected() { /* Arrange */ /* Act */ /* Assert */ }
[Theory] [InlineData(1, 2)] public void M(int a, int b) { }
public static TheoryData<decimal, decimal> Cases => new() { { 1m, 2m } };
[Theory] [MemberData(nameof(Cases))] public void M(decimal a, decimal b) { }
public async Task M_Async() { await Assert.ThrowsAsync<X>(() => sut.DoAsync()); }
var ex = Assert.Throws<ArgumentException>(() => Do()); Assert.Equal("msg", ex.Message);
public class Tests : IClassFixture<Fixture>, IDisposable { public Tests(Fixture f) {} public void Dispose() {} }

// NSubstitute
var dep = Substitute.For<IDep>();
dep.Prop.Returns(value);
dep.MethodAsync(Arg.Any<string>(), Arg.Is(5m)).Returns(Task.FromResult(result));
dep.MethodAsync(default!, default).ReturnsForAnyArgs(Task.FromResult(result));
await dep.Received(1).MethodAsync(Arg.Is("x"), Arg.Any<decimal>());
await dep.DidNotReceiveWithAnyArgs().MethodAsync(default!, default);

// запуск
dotnet test
dotnet test --filter "FullyQualifiedName~OrderServiceCancelTests"
dotnet test --collect:"XPlat Code Coverage"   // при наличии coverlet.collector
```

## Глоссарий

| Термин | Что значит |
|--------|-----------|
| Unit test | тест одной единицы логики в изоляции от инфраструктуры |
| Integration test | тест взаимодействия с реальными компонентами (БД, HTTP) |
| E2E test | сквозной тест системы целиком глазами пользователя |
| Test pyramid | много unit, меньше integration, мало E2E |
| SUT | system under test — тестируемый объект |
| AAA | Arrange / Act / Assert — структура теста |
| Fact / Theory | тест без параметров / параметризованный тест в xUnit |
| Fixture | общий контекст для тестов (`IClassFixture`) |
| Test double | дублёр зависимости в тесте |
| Stub | дублёр, возвращающий заданные данные |
| Mock | дублёр, по которому проверяют взаимодействие |
| Fake | упрощённая рабочая реализация (in-memory) |
| Spy | дублёр, запоминающий вызовы |
| Dependency injection | передача зависимостей снаружи (обычно через конструктор) |
| TDD | разработка через тестирование: red → green → refactor |
| Regression | поломка ранее работавшего поведения |
| Flaky test | «мигающий» тест: то проходит, то нет без изменений кода |
| Code coverage | доля кода, выполненная тестами |

## Вопросы на собеседовании

<details><summary><b>Зачем писать unit-тесты?</b></summary>

Они защищают от регрессий, позволяют безопасно рефакторить, документируют поведение и дают быструю обратную связь — за секунды, без запуска всего приложения. Плюс тестируемость заставляет делать хороший дизайн: явные зависимости, маленькие классы. Тесты окупаются тем быстрее, чем дольше живёт проект и чем больше в нём людей.
</details>

<details><summary><b>Чем unit-тест отличается от интеграционного?</b></summary>

Unit-тест проверяет единицу логики в изоляции: внешние зависимости подменены, тест быстрый (миллисекунды) и детерминированный. Интеграционный проверяет работу с реальными компонентами — БД, HTTP, файловой системой, несколькими слоями вместе; он медленнее и может падать из-за инфраструктуры. По пирамиде unit-тестов больше всего.
</details>

<details><summary><b>Что такое AAA?</b></summary>

Arrange — подготовка данных и зависимостей, Act — одно действие над SUT, Assert — проверка результата. Такая структура делает тесты единообразными и читаемыми, а правило «одно действие» помогает тесту проверять одно поведение.
</details>

<details><summary><b>Fact vs Theory в xUnit?</b></summary>

`[Fact]` — тест без параметров, один сценарий. `[Theory]` — параметризованный тест, который запускается для каждого набора данных из `[InlineData]`, `[MemberData]` или `[ClassData]`. `InlineData` принимает только константы, поэтому для `decimal`, дат и объектов используют `MemberData` с `TheoryData<...>`.
</details>

<details><summary><b>Чем mock отличается от stub?</b></summary>

Stub возвращает заранее заданные данные — он нужен, чтобы SUT мог работать, а проверяем мы результат SUT. Mock — объект, по которому проверяют взаимодействие: что метод вызван, сколько раз и с какими аргументами. Ещё есть fake (упрощённая рабочая реализация, например in-memory репозиторий), spy (запоминает вызовы), dummy (просто заполняет параметр).
</details>

<details><summary><b>Как протестировать код, который использует DateTime.Now?</b></summary>

Убрать прямую зависимость: ввести абстракцию времени (`IClock` или встроенный `TimeProvider` в .NET 8+) и передавать её через конструктор. В production — системная реализация, в тесте — fake с фиксированной датой, которую можно «перематывать». Так тест становится детерминированным и проверяет граничные моменты.

```csharp
public sealed class Discount(TimeProvider time) { /* time.GetUtcNow() */ }
```
</details>

<details><summary><b>Что мокать, а что нет?</b></summary>

Мокать внешние границы: сеть, платёжные шлюзы, почту, очереди, время, файловую систему. Не мокать value objects и чистую логику — их проще использовать настоящими. Избыточная проверка вызовов делает тесты хрупкими: они ломаются при рефакторинге, хотя поведение не изменилось. `DbContext` и `HttpClient` лучше не мокать напрямую, а проверять интеграционными тестами или прятать за своим интерфейсом.
</details>

<details><summary><b>Что такое TDD?</b></summary>

Разработка через тестирование: цикл red (написать падающий тест на следующее требование) → green (минимальный код, чтобы прошёл) → refactor (улучшить, не меняя поведения). Плюсы — тест гарантированно может упасть, код тестируемый и минимальный, всегда есть страховка. Минусы — требует дисциплины, медленнее на старте.
</details>

<details><summary><b>Как xUnit управляет жизненным циклом тест-класса?</b></summary>

Для каждого теста создаётся новый экземпляр класса: конструктор — общий Arrange, `Dispose` — очистка. Поэтому тесты не делят состояние. Для дорогих общих ресурсов есть `IClassFixture<T>` (один экземпляр на класс) и `ICollectionFixture<T>` (на коллекцию классов). Тесты разных классов по умолчанию выполняются параллельно.
</details>

<details><summary><b>Что такое code coverage? Нужно ли 100%?</b></summary>

Процент строк или ветвлений, выполненных тестами (в .NET — coverlet, dotnet-coverage, dotCover). Покрытие показывает непротестированный код, но не качество тестов: строку можно выполнить и ничего не проверить. 100% не цель — важнее покрыть бизнес-логику, ветвления и граничные случаи.
</details>

<details><summary><b>Что такое flaky test и откуда он берётся?</b></summary>

Тест, который то проходит, то падает без изменения кода. Причины: зависимость от времени (`DateTime.Now`, `Task.Delay` с расчётом на скорость), порядка выполнения тестов, общего состояния (static), случайных данных, сети, культуры ОС. Лечится изоляцией и детерминированными test doubles; такие тесты опасны, потому что команда перестаёт доверять красному цвету.
</details>

<details><summary><b>Как протестировать, что метод бросает исключение? А async-метод?</b></summary>

`Assert.Throws<T>(() => sut.Method())` — проверяет точный тип и возвращает исключение, чтобы проверить сообщение или `ParamName`. Для async — `await Assert.ThrowsAsync<T>(() => sut.MethodAsync())`. Для «этот тип или наследник» — `ThrowsAny`/`ThrowsAnyAsync`.
</details>

## Дополнительно

- [Unit testing best practices in .NET](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices)
- [Unit testing C# with xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-csharp-with-xunit)
- [xUnit.net documentation](https://xunit.net/) — включая разделы про shared context (fixtures) и v3
- [NSubstitute documentation](https://nsubstitute.github.io/help/getting-started/)
- [Use code coverage for unit testing](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage)
- [TimeProvider](https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview)
- Vladimir Khorikov, «Unit Testing: Principles, Practices, and Patterns» — лучшая книга про то, что и как тестировать.
- Kent Beck, «Test-Driven Development: By Example».
- Roy Osherove, «The Art of Unit Testing».
