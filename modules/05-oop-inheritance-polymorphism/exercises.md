# Модуль 05 — задачи

> Код пишется в `starter/Module05.Tasks/`. Тесты: `cd modules/05-oop-inheritance-polymorphism/starter && dotnet test`.
>
> Сигнатуры `public`-членов не меняй (кроме задачи 2, где менять модификаторы `virtual`/`override`/`new` — это и есть задача). `=> throw new NotImplementedException()` можно заменять на auto-properties и добавлять private-поля.

## Задача 1. Иерархия фигур  ·  ⏱ ~40 мин  ·  🟢 базовая

**Что сделать:** реализуй `Circle`, `Rectangle`, `Square`, `Triangle` (наследники `abstract class Shape`): конструкторы с валидацией, `Area()`, `Perimeter()`. В базовом классе — невиртуальный `Describe()` (`"Rectangle: area 6.00, perimeter 10.00"`). В `ShapeStats` — `TotalArea` и `Largest`, работающие с любыми фигурами через базовый тип.

**Зачем:** abstract-члены, `override`, полиморфная коллекция `List<Shape>` — «hello world» ООП на собеседовании («спроектируйте иерархию фигур»).
**Файл:** `Module05.Tasks/Shapes.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ShapeTests"`
**Подумай / ответь письменно:**
1. Почему `Shape` — abstract class, а не interface? Что изменилось бы, если бы был `IShape`?
2. `Describe()` не виртуальный, но выводит разные данные для разных фигур. Как так?

## Задача 2. `virtual` / `override` / `new`  ·  ⏱ ~25 мин  ·  🟡 средняя

**Что сделать:** в `Vehicles.cs` наследники «повторяют» методы базы без модификаторов (смотри warning CS0108). Расставь `virtual`/`override`/`new` и напиши тела так, чтобы:
- `Kind()` определялся **реальным типом объекта** (`Vehicle v = new Car(); v.Kind() == "Car"`);
- `Wheels()` определялся **типом переменной** (`Vehicle v = new Car(); v.Wheels() == "unknown"`, но `Car c = ...; c.Wheels() == "4"`).

До запуска тестов **предскажи**, что вернёт `new Car().Describe()`.

**Зачем:** самый частый вопрос «что выведет код» на собеседовании Junior .NET. Нужно не угадать, а объяснить через статический/runtime тип и vtable.
**Файл:** `Module05.Tasks/Vehicles.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~VehicleTests"`
**Подумай / ответь письменно:**
1. Что произойдёт, если в `Car.Wheels()` написать `override` вместо `new`? Какие тесты упадут и почему?
2. Почему C# требует явно писать `virtual`, а в Java все методы виртуальные по умолчанию?

## Задача 3. Головоломки «что выведет этот код?»  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** прочитай 4 головоломки в `Puzzles.cs` (файл не меняй) и запиши в `OutputPredictions.cs` **точные** строки, которые вернёт каждый `Run()`. Правило честности: сначала на бумаге, с объяснением; запускать — только тесты, отладчиком «подсматривать» нельзя. Если тест красный — найди, где ошиблась твоя модель, а не подбирай ответ.

Темы: порядок инициализаторов полей и конструкторов + виртуальный вызов из конструктора; `new` vs `override` в трёх уровнях; overload (компиляция) vs override (runtime); `is`/`as`.

**Зачем:** ровно такие фрагменты дают на собеседованиях. Тренируешь «исполнять код в голове».
**Файлы:** `Module05.Tasks/Puzzles.cs` (читать), `Module05.Tasks/OutputPredictions.cs` (писать)
**Тесты:** `dotnet test --filter "FullyQualifiedName~OutputPredictionTests"`
**Подумай / ответь письменно:** для каждой головоломки — одно предложение «почему именно так». Отдельно: почему в `ConstructorOrderPuzzle` поле `_a` уже присвоено, а `_b` — ещё нет?

## Задача 4. Система уведомлений на интерфейсах  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуй `EmailChannel` и `SmsChannel` (интерфейс `INotificationChannel` готов) и `NotificationService`, который рассылает сообщение через все подходящие каналы, **не зная** их конкретных типов. Email использует default interface method `Format` (`"[Email] Hello"`), а SMS заменяет его через **explicit interface implementation** (без префикса, обрезка до 70 символов).

**Зачем:** программирование через интерфейсы — основа DI и тестирования (модули 14–15). Тесты подсовывают сервису свой `FakeChannel` — это и есть test double. Плюс default methods и явная реализация — частые «проверки кругозора».
**Файл:** `Module05.Tasks/Notifications.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~NotificationTests"`
**Подумай / ответь письменно:**
1. Почему внутри `EmailChannel.Send` нельзя просто написать `Format(message)`?
2. Как добавить канал Telegram? Какие файлы придётся изменить, а какие — нет?

## Задача 5. Способы оплаты: Template Method  ·  ⏱ ~45 мин  ·  🟡 средняя

**Что сделать:** в `abstract class PaymentMethod` реализуй `CanPay` (базовое условие) и `Pay` — общий алгоритм: валидация → `CanPay` → комиссия → `Charge` → результат. Реализуй `CardPayment` (2%, минимум 0.30), `WalletPayment` (баланс, без комиссии), `BankTransferPayment` (фикс. 1.00, минимум 10). `Checkout.PayWithFirstAvailable` выбирает первый способ, который может оплатить.

**Зачем:** abstract + virtual + `base.CanPay(...)` + `protected abstract` в одной задаче; паттерн Template Method встречается во фреймворке повсюду (`BackgroundService`, `Stream`).
**Файл:** `Module05.Tasks/Payments.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~PaymentTests"`
**Подумай / ответь письменно:**
1. Почему `Pay` не `virtual`, а `Charge` — `protected`?
2. Почему наследники вызывают `base.CanPay(amount)`, а не повторяют проверку `amount > 0`?

## Задача 6. Исследование: LSP и Rectangle/Square  ·  ⏱ ~25 мин  ·  🧪 исследование

**Что сделать:** без тестов, во временном консольном проекте (`dotnet new console -n LspDemo` вне папки курса):
1. Напиши `Rectangle` с **изменяемыми** `virtual int Width/Height { get; set; }` и `Area`, и `Square : Rectangle`, где сеттеры меняют обе стороны.
2. Напиши метод `static void Stretch(Rectangle r) { r.Width = 5; r.Height = 4; Console.WriteLine(r.Area); }` и вызови его для обоих. Что вывелось?
3. Предложи два способа исправить дизайн. Сравни с тем, как сделано в задаче 1 (почему там проблемы нет?).
4. Бонус: найди в BCL пример «нарушения» LSP — подсказка: `ReadOnlyCollection<T>` реализует `IList<T>`, вызови `Add` через `IList<T>`.

**Зачем:** LSP — самый «философский» из SOLID, его любят спрашивать с этим примером.
**Подумай / ответь письменно:** сформулируй LSP своими словами в одно предложение и объясни, почему «квадрат — это прямоугольник» верно в математике, но не в коде.

## Задача 7. Рефакторинг: switch по типу → полиморфизм  ·  ⏱ ~45 мин  ·  🔴 сложная  ·  ⭐ со звёздочкой

**Что сделать:** в `Pricing.cs` есть работающий `LegacyPriceCalculator` со `switch` по `CustomerType`. Реализуй новую модель: наследники `Customer` (`RegularCustomer`, `SilverCustomer`, `GoldCustomer`, `EmployeeCustomer`) сами знают свою скидку, а `PriceCalculator.FinalPrice` не содержит ни одного `switch`/`if` по типу. Тесты сравнивают результаты с legacy-версией и проверяют, что **новый тип клиента** (объявленный прямо в тестах) работает без изменения калькулятора.

**Зачем:** «вот код со switch по типу — как улучшить?» — типичная задача на live coding и code review. Это Open/Closed principle на практике.
**Файл:** `Module05.Tasks/Pricing.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~PriceCalculatorTests"`
**Подумай / ответь письменно:**
1. Сколько мест в коде нужно поменять, чтобы добавить тип клиента `Platinum`, — до и после рефакторинга?
2. Когда switch по типу всё-таки уместен? (Подсказка: switch expression по закрытому набору вариантов, pattern matching по данным, которые ты не контролируешь.)
3. Зачем калькулятор проверяет `DiscountPercent` на диапазон 0–100, если все наши классы возвращают корректные значения?
