# Модуль 05. ООП: наследование, полиморфизм, интерфейсы

> **Зачем:** на этом стоит вся архитектура .NET-приложений: сервисы зависят от интерфейсов (`IOrderRepository`), фреймворк вызывает твои переопределённые методы (`ControllerBase`, `BackgroundService.ExecuteAsync`), тесты подменяют реализации. А на собеседовании это **главная** тема «что выведет код»: `virtual`/`override`/`new`, порядок конструкторов, abstract class vs interface. **Время:** ~7–9 ч. **Нужно:** модуль 04.

## Что ты будешь уметь

- Строить иерархии классов, вызывать `base`, понимать порядок вызова конструкторов.
- Безошибочно предсказывать результат `virtual`/`override`/`new` — и объяснять, почему.
- Использовать `abstract`, `sealed`, интерфейсы, явную реализацию, default interface methods.
- Аргументированно выбирать между abstract class и interface, наследованием и композицией.
- Делать upcast/downcast, пользоваться `is`/`as`/pattern matching.
- Видеть нарушение LSP и заменять switch по типу полиморфизмом.

---

## 1. Наследование и `base`

**Наследование** (inheritance) — класс-наследник (derived class) получает все члены базового класса (base class) и может добавить свои. В C# у класса **ровно один** базовый класс (нет множественного наследования классов), но сколько угодно интерфейсов.

```csharp
public class Animal
{
    public Animal(string name) => Name = name;
    public string Name { get; }
    public string Eat() => $"{Name} is eating";
}

public class Dog : Animal                        // Dog наследует Animal
{
    public Dog(string name) : base(name) { }     // base(...) — вызов конструктора базового класса
    public string Bark() => $"{Name}: Woof";     // Name унаследован
}

var dog = new Dog("Rex");
Console.WriteLine(dog.Eat());   // вывод: Rex is eating
```

- Если у базового класса нет конструктора без параметров, наследник **обязан** вызвать `base(...)` явно.
- `base.Method()` — вызов версии метода из базового класса (часто внутри `override`).
- `private`-члены базового класса наследуются (они есть в объекте), но недоступны наследнику. `protected` — доступны.
- Все классы неявно наследуют `System.Object`.

> **Почему так?** Почему нет множественного наследования классов? Из-за «проблемы ромба» (diamond problem): если `C : A, B`, а в `A` и `B` есть `Foo()` с разной реализацией, какую получит `C`? C++ решает это сложными правилами, а C# (как и Java) выбрал простоту: один базовый класс + интерфейсы. С C# 8 у интерфейсов есть реализации по умолчанию, но и там компилятор требует явно разрешать неоднозначность.

### Порядок инициализации — классика собеседований

При `new Derived()`:
1. инициализаторы полей **Derived**;
2. инициализаторы полей **Base**;
3. тело конструктора **Base**;
4. тело конструктора **Derived**.

```csharp
class Base
{
    public Base() { Console.WriteLine("Base ctor"); Print(); }
    protected virtual void Print() => Console.WriteLine("Base.Print");
}
class Derived : Base
{
    private string _name = "init";               // инициализатор — выполнится ДО Base()
    private string? _city;
    public Derived() { _city = "Paris"; Console.WriteLine("Derived ctor"); }
    protected override void Print() => Console.WriteLine($"Derived.Print {_name} {_city ?? "null"}");
}

new Derived();
// вывод:
// Base ctor
// Derived.Print init null     ← виртуальный вызов из конструктора попал в наследника ДО его конструктора!
// Derived ctor
```

> **Почему так?** Объект создаётся сразу с итоговым типом `Derived` (тип не «дорастает» по ходу конструкторов), поэтому виртуальный вызов в конструкторе `Base` уходит в `Derived.Print`. Инициализаторы полей C# выполняет до вызова `base(...)` именно чтобы поля были хоть как-то готовы к такому вызову; но то, что присваивается в **теле** конструктора наследника, ещё не присвоено. Вывод: **не вызывай виртуальные методы из конструктора** — анализаторы (CA2214) за это ругаются. (В Java, кстати, инициализаторы полей идут после `super()`, и там `_name` был бы `null`.)

> 💼 **На собеседовании:** «Сначала инициализаторы полей от наследника к базе, потом тела конструкторов от базы к наследнику. Виртуальный метод из конструктора базового класса вызовет override наследника, у которого конструктор ещё не отработал — поэтому так делать нельзя».

→ **Задача 3** (головоломки) в [exercises.md](exercises.md).

---

## 2. `virtual`, `override`, `new` — полиморфизм

**Полиморфизм** (polymorphism, «много форм») — один и тот же вызов ведёт себя по-разному в зависимости от **реального типа объекта**.

```csharp
public class Shape
{
    public virtual string Draw() => "shape";        // virtual: можно переопределить
}
public class Circle : Shape
{
    public override string Draw() => "circle";      // override: переопределяем
}
public class Square : Shape
{
    public new string Draw() => "square";           // new: СКРЫВАЕМ (method hiding)
}

Shape a = new Circle();
Shape b = new Square();
Console.WriteLine(a.Draw());               // вывод: circle  ← по типу ОБЪЕКТА
Console.WriteLine(b.Draw());               // вывод: shape   ← new не участвует в полиморфизме
Console.WriteLine(((Square)b).Draw());     // вывод: square  ← по типу ПЕРЕМЕННОЙ
```

Два вида «типа» у выражения:
- **compile-time type** (статический тип) — тип переменной: `Shape a`;
- **runtime type** (реальный тип) — тип объекта: `Circle`.

| | Чем определяется вызов | Механизм |
|---|---|---|
| обычный (не virtual) метод | статическим типом | early binding, при компиляции |
| `virtual` + `override` | реальным типом объекта | late binding, через vtable |
| `new` | статическим типом | это **другой** метод с тем же именем |
| перегрузка (overload) | статическими типами аргументов | при компиляции |

> **Почему так?** Почему методы в C# **не виртуальные по умолчанию** (в отличие от Java)? Решение Андерса Хейлсберга: (1) **версионирование** — если базовый класс библиотеки в новой версии добавит метод с тем же именем, что у твоего наследника, твой метод не станет «случайно» его переопределять — компилятор предупредит (CS0108) и ты явно выберешь `override` или `new`; (2) **производительность** — невиртуальный вызов можно заинлайнить; (3) **проектирование** — точки расширения класса должны быть осознанными. `new` существует именно для сценария «базовый класс добавил член, а у меня уже был такой» — в новом коде он почти не нужен.

Как работает `virtual` внутри: у каждого типа есть **таблица виртуальных методов** (vtable, method table). Объект хранит указатель на таблицу своего реального типа; вызов виртуального метода — «возьми из таблицы объекта слот №N». `override` **заменяет** слот, `new` создаёт **новый** слот, а старый остаётся.

`sealed override` — запрещает дальнейшее переопределение в наследниках.

> 💼 **На собеседовании:** «`override` заменяет реализацию виртуального метода — вызов через ссылку на базовый тип пойдёт в наследника. `new` скрывает метод — это новый метод с тем же именем, вызов определяется типом переменной. Методы по умолчанию не виртуальные — это сделано ради версионирования и производительности».

→ **Задача 2** (Vehicle) в [exercises.md](exercises.md).

---

## 3. Abstract-классы и `sealed`

**Abstract class** — класс, экземпляр которого создать нельзя; он задаёт «каркас» для наследников. Может содержать **abstract-члены** (без реализации — наследник обязан реализовать) и обычные члены с реализацией, поля, конструкторы.

```csharp
public abstract class Report
{
    // Template Method: общий алгоритм зафиксирован, шаги — у наследников
    public string Build() => $"{Header()}\n{Body()}\n-- generated";
    protected virtual string Header() => "REPORT";   // можно переопределить
    protected abstract string Body();                 // нужно переопределить
}

public sealed class SalesReport : Report
{
    protected override string Body() => "Sales: 42";
}

Console.WriteLine(new SalesReport().Build());
// вывод:
// REPORT
// Sales: 42
// -- generated
// new Report();  // ошибка CS0144: нельзя создать экземпляр абстрактного класса
```

**`sealed`** на классе запрещает наследование (`string`, `record`-ы часто `sealed`). На `override`-методе — запрещает дальнейшее переопределение.

> **Почему так?** Abstract class — способ сказать «это понятие, а не готовая вещь» (не бывает «просто фигуры» — бывают круг и квадрат), и дать наследникам общую реализацию без копипасты. `sealed` по умолчанию для классов, не спроектированных для наследования, — хорошая практика: наследование — сильная связь, и автор базового класса должен гарантировать, что переопределение не сломает инварианты. Плюс JIT может девиртуализировать вызовы у `sealed`-типов (небольшой выигрыш в скорости).

→ **Задача 1** (фигуры) и **Задача 5** (платежи — Template Method) в [exercises.md](exercises.md).

---

## 4. Интерфейсы

**Interface** — контракт: набор членов, которые класс обязуется реализовать. Описывает «что умеет», а не «что такое».

```csharp
public interface IMessageSender
{
    string Channel { get; }
    void Send(string to, string text);
}

public sealed class EmailSender : IMessageSender
{
    public string Channel => "email";
    public void Send(string to, string text) => Console.WriteLine($"mail to {to}: {text}");
}

IMessageSender sender = new EmailSender();   // работаем через абстракцию
sender.Send("anna@example.com", "Hi");       // вывод: mail to anna@example.com: Hi
```

- Класс может реализовать **много** интерфейсов: `class FileLogger : ILogger, IDisposable`.
- Члены интерфейса по умолчанию `public`. Полей экземпляра нет (только свойства/методы/события/индексаторы; static-члены — с C# 8/11).
- Имена начинаются с `I` — соглашение .NET.

### Явная реализация (explicit interface implementation)

```csharp
public interface IPrinter { string Print(); }
public interface IExporter { string Print(); }

public class Document : IPrinter, IExporter
{
    string IPrinter.Print() => "to paper";    // без модификатора доступа, имя с префиксом интерфейса
    string IExporter.Print() => "to PDF";
}

var doc = new Document();
// doc.Print();                               // ошибка: такого публичного метода нет
Console.WriteLine(((IPrinter)doc).Print());   // вывод: to paper
Console.WriteLine(((IExporter)doc).Print());  // вывод: to PDF
```

Зачем: разрешить конфликт имён из двух интерфейсов; спрятать «служебный» член из публичного API класса (так `List<T>` реализует `ICollection.SyncRoot`).

### Default interface methods (C# 8)

```csharp
public interface ILogger
{
    void Log(string message);
    void LogError(string message) => Log($"ERROR: {message}");   // реализация по умолчанию
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
}

ILogger logger = new ConsoleLogger();
logger.LogError("disk full");        // вывод: ERROR: disk full
// new ConsoleLogger().LogError("x"); // ошибка: метод доступен только через интерфейс
```

> **Почему так?** Default interface methods придуманы для **эволюции API**: добавить метод в публичный интерфейс раньше значило сломать все его реализации у всех пользователей библиотеки. Теперь можно добавить метод с реализацией по умолчанию. Метод не становится членом класса (иначе вернулась бы проблема ромба), поэтому вызывается только через ссылку интерфейса. В прикладном коде их используют редко — это инструмент авторов библиотек.

→ **Задача 4** (уведомления) в [exercises.md](exercises.md).

---

## 5. Abstract class vs interface — вопрос №1 по ООП

| | Abstract class | Interface |
|---|---|---|
| Сколько можно унаследовать | один | сколько угодно |
| Состояние (поля экземпляра) | да | нет |
| Конструкторы | да | нет |
| Реализация методов | да | только default methods (C# 8+) и static |
| Модификаторы доступа членов | любые | по умолчанию public |
| Смысл | «является» (is-a), общая база | «умеет» (can-do), контракт |
| Применение | общий код + шаблон алгоритма | абстракция для DI, тестов, разных реализаций |

```csharp
// Типичное сочетание в реальном коде: интерфейс для потребителей + базовый класс для реализаций
public interface IRepository<T> { T? Find(int id); }
public abstract class RepositoryBase<T> : IRepository<T>
{
    public T? Find(int id) { /* общая логика: логирование, кэш */ return Load(id); }
    protected abstract T? Load(int id);
}
```

> **Почему так?** Интерфейс — минимальная связанность: потребитель знает только контракт и ничего не знает о реализации, поэтому её легко подменить (mock в тестах, другая БД). Abstract class связывает сильнее, но даёт переиспользование кода и контроль над алгоритмом. Правило по умолчанию: **зависимости — через интерфейсы**, abstract class — когда у нескольких реализаций реально есть общий код и состояние. После появления default methods граница размылась, но ключевые отличия — состояние, конструкторы и одиночное наследование — остались.

> 💼 **На собеседовании:** «Abstract class — это частично реализованная база: может иметь состояние, конструкторы, любые модификаторы, но наследовать можно только один. Интерфейс — контракт без состояния, реализовать можно много. Интерфейс выбираю для абстракций и DI, abstract class — когда у реализаций есть общий код. Часто используют оба: интерфейс + базовый класс».

---

## 6. Upcast, downcast, `is`, `as`, `object`

**Upcast** (к базовому типу) — неявный и всегда безопасный. **Downcast** (к наследнику) — явный и может упасть.

```csharp
Animal a = new Dog("Rex");      // upcast — неявно
Dog d = (Dog)a;                 // downcast — ок, объект действительно Dog
Cat c = (Cat)a;                 // InvalidCastException в runtime!

Cat? maybe = a as Cat;          // as: при неудаче → null, без исключения (только для reference/nullable типов)
if (a is Dog dog)               // pattern matching: проверка + приведение за один шаг
    Console.WriteLine(dog.Bark());   // вывод: Rex: Woof
```

Рекомендации: `is Type x` — лучший вариант (проверка и переменная); `as` + проверка на null — если нужна переменная вне `if`; прямой каст — когда ты **уверен** в типе и ошибка должна быть громкой.

> **Почему так?** Частые downcast'ы — признак плохого дизайна: если код постоянно спрашивает «а ты случайно не `Dog`?», значит, поведение нужно вынести в виртуальный метод базового класса. Это и есть «заменить switch по типу полиморфизмом» (задача 7).

### Методы `object`

Все типы наследуют от `System.Object`: `ToString()`, `Equals(object?)`, `GetHashCode()` (virtual), `GetType()` (не virtual, возвращает runtime type), `MemberwiseClone()` (protected, поверхностная копия), `ReferenceEquals` (static). Переопределение `Equals`/`GetHashCode` — тема модуля 06.

---

## 7. Композиция vs наследование, LSP

**Наследование** — отношение «является» (is-a): `Dog` является `Animal`. **Композиция** — «содержит» (has-a): `Car` содержит `Engine`.

```csharp
// Наследование ради переиспользования кода — плохо:
public class Stack<T> : List<T> { }       // у «стека» появились Insert(0, x), RemoveAt(5)... Сломали абстракцию.

// Композиция — хорошо:
public class Stack<T>
{
    private readonly List<T> _items = new();   // используем List, но не являемся им
    public void Push(T item) => _items.Add(item);
    public T Pop() { var x = _items[^1]; _items.RemoveAt(_items.Count - 1); return x; }
}
```

**Принцип подстановки Лисков** (Liskov Substitution Principle, LSP, буква L в SOLID): объект наследника должен работать везде, где ожидается базовый тип, **не ломая ожиданий** вызывающего кода.

Классический пример нарушения — Rectangle/Square:

```csharp
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }
    public int Area => Width * Height;
}
public class Square : Rectangle
{
    public override int Width  { set { base.Width = value; base.Height = value; } }
    public override int Height { set { base.Width = value; base.Height = value; } }
}

void Resize(Rectangle r) { r.Width = 5; r.Height = 4; Console.WriteLine(r.Area); }
Resize(new Rectangle());   // вывод: 20
Resize(new Square());      // вывод: 16  ← код, написанный для Rectangle, сломался
```

> **Почему так?** «Квадрат — это прямоугольник» верно в математике, где фигуры неизменяемы. В коде с сеттерами у прямоугольника есть **поведение** «ширина и высота меняются независимо», и квадрат его нарушает. Наследование — это про совместимость **поведения**, а не про таксономию. Решения: сделать фигуры неизменяемыми или не наследовать `Square` от `Rectangle` (общий базовый `Shape`). Общее правило «favor composition over inheritance» (из книги «банды четырёх»): наследование — самая сильная связь между классами, наследник зависит от деталей базы («хрупкий базовый класс»). Композиция гибче: части можно заменить, в том числе в runtime.

→ **Задача 6** (исследование LSP) и **Задача 7 ⭐** (switch → полиморфизм) в [exercises.md](exercises.md).

---

## Типичные ошибки

1. Забыли `virtual` в базовом классе → «переопределение» в наследнике на самом деле скрытие (warning CS0108, который игнорируют).
2. Используют `new` вместо `override` «чтобы убрать warning» — полиморфизм тихо ломается.
3. Вызывают виртуальные методы из конструктора.
4. Наследуются ради переиспользования пары методов (`class Stack : List`) вместо композиции.
5. Цепочки `if (x is A) ... else if (x is B) ...` вместо виртуального метода.
6. Downcast без проверки `(Dog)animal` там, где тип не гарантирован.
7. Интерфейсы «на всякий случай» с одной реализацией и 20 методами — интерфейсы должны быть маленькими (Interface Segregation).
8. Глубокие иерархии (5+ уровней) — трудно понять, где реально реализован метод.
9. Ожидают, что default interface method можно вызвать на переменной класса.
10. Переопределяя метод, не вызывают `base.Method()`, хотя базовая логика нужна (или наоборот — вызывают, когда не нужна).

## Шпаргалка

```csharp
public abstract class Base                    // нельзя new Base()
{
    protected Base(string id) => Id = id;     // конструктор для наследников
    public string Id { get; }
    public abstract double Calc();            // обязан реализовать
    public virtual string Name => "base";     // может переопределить
    public string Info() => $"{Name}:{Calc()}";   // не virtual; вызывает virtual внутри
}

public class Child : Base, IComparable<Child>
{
    public Child() : base("c1") { }           // вызов конструктора базы
    public override double Calc() => 42;
    public override string Name => "child:" + base.Name;
    public int CompareTo(Child? other) => 0;
}

public sealed class Leaf : Child
{
    public new string Info() => "hidden";     // скрытие — работает только через Leaf-переменную
}

Base b = new Leaf();
b.Info();              // "child:base:42"  — Base.Info, Name и Calc виртуально из Child
((Leaf)b).Info();      // "hidden"
if (b is Child c) { }  // pattern matching
var x = b as Leaf;     // null, если не Leaf
```

Порядок `new Derived()`: поля Derived → поля Base → ctor Base → ctor Derived.

## Глоссарий

| Термин | Что значит |
|---|---|
| inheritance | получение членов базового класса наследником |
| base class / derived class | базовый класс / класс-наследник |
| polymorphism | один вызов — разное поведение в зависимости от реального типа |
| `virtual` | метод/свойство, которое можно переопределить |
| `override` | переопределение виртуального члена (замена слота vtable) |
| method hiding (`new`) | новый член с тем же именем, скрывающий член базы |
| vtable | таблица виртуальных методов типа |
| early / late binding | выбор метода при компиляции / в runtime |
| compile-time type / runtime type | тип переменной / тип объекта |
| abstract class | класс, экземпляр которого нельзя создать; может иметь abstract-члены |
| `sealed` | запрет наследования (класс) или дальнейшего переопределения (метод) |
| interface | контракт без состояния; класс может реализовать много интерфейсов |
| explicit interface implementation | реализация члена, доступная только через ссылку интерфейса |
| default interface method | метод интерфейса с реализацией (C# 8) |
| upcast / downcast | приведение к базовому / к производному типу |
| composition | объект содержит другие объекты и делегирует им работу |
| LSP | наследник должен быть заменяем на базовый тип без нарушения поведения |
| Template Method | паттерн: алгоритм в базовом классе, шаги — в наследниках |
| Open/Closed | открыт для расширения, закрыт для изменения |

## Вопросы на собеседовании

<details><summary><b>Чем <code>override</code> отличается от <code>new</code>?</b></summary>

`override` заменяет реализацию виртуального метода в таблице виртуальных методов: вызов через ссылку на базовый тип пойдёт в версию наследника (решается в runtime по типу объекта). `new` создаёт **новый** метод с тем же именем, скрывающий базовый: какой вызвать, решает компилятор по типу переменной. Пример: `Base b = new Derived(); b.M();` — при override вызовется `Derived.M`, при new — `Base.M`. `new` нужен редко — в основном когда в базовом классе появился член с тем же именем, что у тебя уже был.
</details>

<details><summary><b>Что выведет код?</b>

```csharp
class A { public virtual string F() => "A"; }
class B : A { public override string F() => "B"; }
class C : B { public new string F() => "C"; }

A x = new C(); B y = new C(); C z = new C();
Console.WriteLine($"{x.F()} {y.F()} {z.F()}");
```
</summary>

`B B C`. Для `x` и `y` вызов виртуальный: runtime ищет последнее `override` в цепочке для объекта типа `C` — это `B.F`, потому что `C.F` объявлен с `new` и в цепочку не входит. Для `z` статический тип `C`, компилятор выбирает `C.F`.
</details>

<details><summary><b>В каком порядке вызываются конструкторы при наследовании? Можно ли вызывать виртуальный метод из конструктора?</b></summary>

Сначала инициализаторы полей наследника, потом инициализаторы полей базы, затем тело конструктора базы, затем тело конструктора наследника. Виртуальный метод вызвать можно, но опасно: объект уже имеет тип наследника, поэтому вызовется его override, а конструктор наследника ещё не выполнился — поля, присваиваемые в нём, имеют значения по умолчанию. Это источник трудноуловимых `NullReferenceException`; анализатор CA2214 предупреждает об этом.
</details>

<details><summary><b>Abstract class или interface — что и когда?</b></summary>

Abstract class: может иметь состояние, конструкторы, реализованные методы, любые модификаторы доступа; наследовать можно только один. Интерфейс: контракт без состояния, класс реализует сколько угодно; с C# 8 есть default methods. Интерфейс — для абстракций, от которых зависит код (DI, тесты, разные реализации). Abstract class — когда у реализаций есть общий код и состояние, например Template Method. Часто используют вместе: `IRepository` + `RepositoryBase`.
</details>

<details><summary><b>Почему в C# методы не виртуальные по умолчанию?</b></summary>

Версионирование: если базовый класс в новой версии добавит метод с тем же именем, что у наследника, не будет случайного переопределения — компилятор предупредит, и разработчик явно выберет `override` или `new`. Производительность: невиртуальный вызов дешевле и может быть заинлайнен. Дизайн: точки расширения должны быть осознанными — переопределяемый метод становится частью контракта класса, и автор должен это спроектировать.
</details>

<details><summary><b>Что такое полиморфизм? Какие виды бывают?</b></summary>

Возможность работать с объектами разных типов через общий интерфейс, при этом каждый ведёт себя по-своему. Subtype (runtime) polymorphism — `virtual`/`override` и интерфейсы, выбор в runtime по типу объекта. Ad-hoc polymorphism — перегрузка методов и операторов, выбор при компиляции. Parametric polymorphism — generics (`List<T>`, модуль 08). На собеседовании обычно ждут первый вид с примером: `Shape.Area()` у круга и квадрата.
</details>

<details><summary><b>Зачем нужна явная реализация интерфейса?</b></summary>

Чтобы разрешить конфликт, когда два интерфейса имеют член с одинаковой сигнатурой, но разным смыслом; или чтобы спрятать член из публичного API класса (он доступен только через ссылку на интерфейс). Явно реализованный член не имеет модификатора доступа и не может быть `virtual`/`abstract`. Пример из BCL — `List<T>` явно реализует необобщённые `ICollection.SyncRoot`, `IList.Add(object)`.
</details>

<details><summary><b>Что такое default interface methods и как их вызвать?</b></summary>

С C# 8 метод интерфейса может иметь реализацию. Класс может её не переопределять. Метод не становится членом класса, поэтому вызывается только через ссылку на интерфейс: `((ILogger)logger).LogError(...)`. Главная цель — добавлять методы в опубликованные интерфейсы без поломки существующих реализаций.
</details>

<details><summary><b>Чем <code>is</code> отличается от <code>as</code> и от прямого приведения?</b></summary>

`(Dog)x` — бросает `InvalidCastException`, если тип не подходит. `x as Dog` — возвращает `null` (работает только для reference и nullable типов). `x is Dog` — возвращает bool, а `x is Dog d` ещё и объявляет переменную (pattern matching) — самый идиоматичный современный вариант. Все три не вызывают пользовательские операторы преобразования (кроме прямого приведения).
</details>

<details><summary><b>Что такое LSP? Пример нарушения.</b></summary>

Liskov Substitution Principle: объекты наследника должны быть заменяемы на объекты базового типа без нарушения корректности программы. Наследник не должен усиливать предусловия, ослаблять постусловия и ломать инварианты базы. Классика — `Square : Rectangle` с сеттерами: код, который ставит ширину 5 и высоту 4 и ждёт площадь 20, получит 16. Другой пример — наследник, который бросает `NotSupportedException` в методе базового класса.
</details>

<details><summary><b>Композиция или наследование?</b></summary>

По умолчанию — композиция: «содержит» вместо «является». Наследование создаёт сильную связь — наследник зависит от деталей реализации базы (проблема хрупкого базового класса), и его нельзя поменять в runtime. Наследование оправдано при настоящем отношении is-a, когда соблюдается LSP, и когда нужен полиморфизм через базовый тип. Пример плохого наследования — `class Stack : List` (стек получает `Insert` в середину).
</details>

<details><summary><b>Что делает <code>sealed</code>? Зачем запрещать наследование?</b></summary>

`sealed class` нельзя наследовать; `sealed override` запрещает дальнейшее переопределение метода. Запрещают, когда класс не спроектирован для расширения: переопределение может нарушить его инварианты, а поддерживать совместимость с неизвестными наследниками дорого. Бонус — JIT может девиртуализировать вызовы. `string` — sealed; многие гайды советуют делать классы sealed по умолчанию.
</details>

## Дополнительно

- [Наследование (learn.microsoft.com)](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Полиморфизм](https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [Versioning with override and new](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords)
- [Интерфейсы](https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces), [default interface methods](https://learn.microsoft.com/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions)
- [Explicit interface implementation](https://learn.microsoft.com/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- Книги: Рихтер, «CLR via C#» — гл. 6 (виртуальные методы, версионирование), гл. 13 (интерфейсы); Скит, «C# in Depth» — default interface methods; «Design Patterns» (GoF) — Template Method, «favor composition over inheritance».
