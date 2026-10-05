# Модуль 01. Платформа .NET и инструменты

> **Зачем:** почти каждое собеседование на Junior .NET начинается с вопросов «что такое CLR?», «чем SDK отличается от Runtime?», «что происходит, когда вы нажимаете Run?». Это проверка того, что вы понимаете, *на чём* пишете, а не просто знаете синтаксис. В работе это знание нужно каждый день: собрать проект, подключить пакет, разобраться, почему на сервере «не запускается», и отладить код. **Время:** ~4–5 ч. **Нужно:** установленный .NET 10 SDK и любая IDE (Rider, Visual Studio 2026, VS Code + C# Dev Kit).

## Что ты будешь уметь

- Объяснить разницу между .NET, .NET Framework и .NET Standard — и почему их три.
- Рассказать путь кода: C# → IL → JIT → машинный код, и что делает CLR.
- Различать SDK и Runtime, assembly и NuGet-пакет, namespace и assembly.
- Создавать, собирать, запускать и тестировать проекты через `dotnet` CLI, читать `.csproj` и `.slnx`.
- Писать консольные программы: top-level statements, `Main`, `args`, ввод/вывод.
- Ставить breakpoints, ходить по коду отладчиком и смотреть IL на SharpLab.

---

## 1. Что такое .NET и почему «дотнетов» несколько

**.NET** — это платформа (platform — среда выполнения + библиотеки + инструменты) для запуска программ на C#, F# и VB.NET. Исторически было несколько реализаций:

| Название | Годы | Где работает | Статус |
|----------|------|--------------|--------|
| **.NET Framework** 1.0–4.8.1 | 2002–… | только Windows | поддерживается, но новых возможностей нет |
| **.NET Core** 1.0–3.1 | 2016–2019 | Windows, Linux, macOS | переименован в .NET |
| **.NET 5, 6, 7, 8, 9, 10** | 2020–… | кроссплатформенный | **современный .NET**, всё новое — здесь |
| **Mono / Xamarin** | 2004–… | мобильные, Unity | влились в .NET (MAUI) |

**.NET Standard** — это не реализация, а **спецификация** (specification — список API, которые обязана иметь платформа). Библиотека, собранная под `netstandard2.0`, работает и на .NET Framework 4.6.1+, и на современном .NET. Сегодня новые библиотеки обычно таргетят сразу `net10.0` (а `netstandard2.0` — только если нужна поддержка старого .NET Framework).

Версии выходят раз в год в ноябре. Чётные (8, 10) — **LTS** (Long-Term Support, 3 года поддержки), нечётные (9, 11) — **STS** (Standard-Term Support, 2 года). Курс использует .NET 10 (LTS) и C# 14.

> **Почему так?** .NET Framework был намертво привязан к Windows (части BCL вызывали Windows API напрямую), и его обновление ломало чужие программы на той же машине — он устанавливался «один на систему». Microsoft выбрала переписать платформу с нуля (.NET Core): кроссплатформенность, open source, установка рядом с приложением и высокая производительность. Цена — несовместимость части старых API (WebForms, WCF-сервер, AppDomains). А .NET Standard появился как «мост», чтобы одна библиотека работала на обеих платформах в переходный период.

> 💼 **На собеседовании:** «.NET Framework — старая Windows-only платформа, развивается только исправлениями. .NET (бывший .NET Core) — современная кроссплатформенная open source реализация, все новые проекты — на ней. .NET Standard — спецификация общего набора API, чтобы библиотеки работали на обеих».

---

## 2. SDK vs Runtime

- **Runtime** (среда выполнения) — то, что нужно, чтобы **запустить** готовое приложение: CLR + базовые библиотеки. Бывают варианты: .NET Runtime (консоль), ASP.NET Core Runtime (веб), Desktop Runtime (WPF/WinForms).
- **SDK** (Software Development Kit) — то, что нужно, чтобы **разработать**: компилятор Roslyn, MSBuild, `dotnet` CLI с командами `new/build/test/publish`, шаблоны проектов. SDK **включает** Runtime.

```bash
dotnet --version        # версия SDK, например 10.0.100
dotnet --list-sdks      # все установленные SDK
dotnet --list-runtimes  # все установленные runtime
dotnet --info           # всё сразу + ОС и архитектура
```

На машине разработчика стоит SDK. На сервере обычно только Runtime — или вообще ничего, если приложение опубликовано как **self-contained** (runtime упакован внутрь приложения).

> **Почему так?** Разделение экономит место и уменьшает поверхность атаки на продакшене: серверу не нужен компилятор. А self-contained публикация решает проблему «на сервере не та версия .NET»: приложение приносит свой runtime. Компромисс — размер (десятки мегабайт вместо сотен килобайт).

> 💼 **На собеседовании:** «Runtime — чтобы запускать, SDK — чтобы разрабатывать и собирать; SDK содержит runtime. На проде достаточно runtime или self-contained публикации».

---

## 3. Как выполняется код: C# → IL → машинный код

```
  Program.cs ──(Roslyn, csc)──▶ MyApp.dll ──(CLR: JIT)──▶ машинный код x64/ARM64
   C# код           компиляция     IL + metadata        во время выполнения
```

1. **Компилятор Roslyn** превращает C# не в машинный код, а в **IL** (Intermediate Language, он же CIL/MSIL — промежуточный язык, «ассемблер виртуальной машины»).
2. IL вместе с **metadata** (метаданные — описание всех типов, методов, ссылок) кладётся в **assembly** (сборка — файл `.dll` или `.exe`).
3. При запуске **CLR** (Common Language Runtime — виртуальная машина .NET) загружает сборку, и **JIT-компилятор** (Just-In-Time) переводит IL каждого метода в машинный код **при первом вызове** этого метода. Дальше вызывается уже готовый машинный код.

```csharp
static int Add(int a, int b) => a + b;
```

```
// IL этого метода (посмотреть можно на sharplab.io):
ldarg.0     // положить на стек вычислений аргумент a
ldarg.1     // положить b
add         // сложить два верхних значения
ret         // вернуть результат
```

**Что ещё делает CLR**, кроме JIT:
- **Garbage Collector** (GC, сборщик мусора) — автоматически освобождает память (модуль 12);
- **type safety** — проверка, что вы не обращаетесь с объектом как с типом, которым он не является;
- обработка исключений, управление потоками, загрузка сборок, безопасность.

Код, который выполняется под управлением CLR, называют **managed code** (управляемый код). Код на C/C++, который работает напрямую с ОС, — **unmanaged code**.

**Tiered compilation** (многоуровневая компиляция): сначала JIT компилирует метод быстро и без оптимизаций (Tier 0), а если метод вызывается часто — перекомпилирует его с полными оптимизациями (Tier 1), используя статистику реального выполнения.

**AOT** (Ahead-Of-Time) — альтернатива JIT: компиляция в машинный код **до** запуска.
- **ReadyToRun** — сборка содержит и IL, и заранее скомпилированный код: быстрее старт, JIT всё ещё доступен.
- **Native AOT** — один нативный исполняемый файл без JIT: мгновенный старт, малый размер, но ограничения (нет генерации кода в runtime, ограниченная reflection).

> **Почему так?** Зачем промежуточный IL, если можно сразу в машинный код? (1) **Один .dll работает на любой ОС и процессоре** — машинный код создаётся уже на месте. (2) **Много языков** (C#, F#, VB) компилируются в один IL и свободно используют библиотеки друг друга — общая система типов CTS (Common Type System). (3) **JIT знает конкретный процессор** и может использовать его инструкции (AVX и т.д.) и статистику выполнения. Цена — время на JIT при старте; поэтому и появились ReadyToRun и Native AOT для сценариев, где важен холодный старт (serverless, CLI-утилиты).

> 💼 **На собеседовании:** «Компилятор C# создаёт IL и метаданные в сборке. При запуске CLR загружает сборку, JIT компилирует IL в машинный код при первом вызове метода. CLR также отвечает за GC, type safety, исключения. Есть AOT-компиляция — ReadyToRun и Native AOT — для быстрого старта».

→ **Задача 6** (исследование IL на SharpLab, см. exercises.md)

---

## 4. BCL, assembly, namespace, NuGet

- **BCL** (Base Class Library) — стандартная библиотека: `System.String`, `List<T>`, `Console`, `File`, `HttpClient`, `Task`… Часто говорят шире — **FCL** (Framework Class Library), но на собеседовании BCL достаточно.
- **Assembly** (сборка) — единица развёртывания и версионирования: `.dll`/`.exe` с IL, метаданными и **manifest** (имя, версия, зависимости).
- **Namespace** (пространство имён) — логическая группировка типов, чтобы не было конфликтов имён: `System.Text.StringBuilder`. **Один namespace может быть размазан по нескольким сборкам, а одна сборка — содержать много namespace.** Это разные оси: namespace — для имён, assembly — для файлов.
- **NuGet** — менеджер пакетов .NET. **Пакет** (`.nupkg`) — zip-архив со сборками под разные target frameworks + описание зависимостей. Источник по умолчанию — nuget.org.

```bash
dotnet add package Newtonsoft.Json      # добавит <PackageReference> в .csproj
dotnet list package                     # какие пакеты подключены
dotnet list package --outdated          # есть ли обновления
```

> **Почему так?** Пакет, а не просто «скачай dll»: NuGet решает **транзитивные зависимости** (пакет A тянет B версии ≥ 2.0), выбирает сборку под нужный target framework и даёт воспроизводимую сборку на любой машине — пакеты восстанавливаются (`dotnet restore`) по описанию в `.csproj`, поэтому их не хранят в git.

---

## 5. `dotnet` CLI, `.csproj` и `.slnx`

Основные команды (работают одинаково на Windows, Linux и macOS):

| Команда | Что делает |
|---------|-----------|
| `dotnet new console -n MyApp` | создать проект из шаблона (`console`, `classlib`, `xunit`, `webapi`…) |
| `dotnet new sln -n MySolution` | создать solution |
| `dotnet sln add MyApp/MyApp.csproj` | добавить проект в solution |
| `dotnet add reference ../Lib/Lib.csproj` | ссылка на другой проект |
| `dotnet add package <Id>` | подключить NuGet-пакет |
| `dotnet restore` | скачать пакеты (вызывается автоматически при build) |
| `dotnet build` | собрать (результат в `bin/Debug/net10.0/`) |
| `dotnet run` | собрать и запустить |
| `dotnet test` | собрать и запустить тесты |
| `dotnet publish -c Release` | подготовить к развёртыванию |

**`.csproj`** — файл проекта в формате MSBuild (XML). Современный SDK-style проект очень короткий:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>                 <!-- Exe — приложение; без этого — библиотека -->
    <TargetFramework>net10.0</TargetFramework>   <!-- под какую платформу собираем -->
    <Nullable>enable</Nullable>                  <!-- nullable reference types (модуль 02) -->
    <ImplicitUsings>enable</ImplicitUsings>      <!-- автоматические using System; и др. -->
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
    <ProjectReference Include="..\MyLib\MyLib.csproj" />
  </ItemGroup>
</Project>
```

Заметьте: **список `.cs` файлов не указан** — SDK-style проект включает все `.cs` в папке автоматически. `Sdk="Microsoft.NET.Sdk.Web"` — для веб-проектов (модуль 15).

В этом курсе `TargetFramework`, `Nullable` и `ImplicitUsings` вынесены в **`Directory.Build.props`** в корне — MSBuild сам подхватывает этот файл для всех проектов ниже по дереву папок.

**Solution** (решение) — контейнер для нескольких проектов, который открывает IDE. Старый формат `.sln` — нечитаемый текст с GUID-ами; новый **`.slnx`** — простой XML (в .NET 10 SDK он стал форматом по умолчанию):

```xml
<Solution>
  <Project Path="Module01.Tasks/Module01.Tasks.csproj" />
  <Project Path="Module01.Tests/Module01.Tests.csproj" />
</Solution>
```

Папки **`bin/`** (результат сборки) и **`obj/`** (промежуточные файлы, кэш restore) — генерируются, в git их не кладут (см. `.gitignore`). Если сборка «сошла с ума» — удалите их и соберите заново.

> **Почему так?** CLI — единый интерфейс для IDE, CI-серверов (GitHub Actions, Azure DevOps) и людей: то, что работает в терминале, работает и на сборочном сервере. Короткий SDK-style `.csproj` (с 2017 года) пришёл на смену старому, где каждый файл перечислялся вручную, — это постоянно вызывало конфликты слияния в git.

> 💼 **На собеседовании** могут спросить: «Как подключить библиотеку?» → `dotnet add package` / NuGet Package Manager в IDE, в `.csproj` появится `PackageReference`. «Чем solution отличается от project?» → project собирается в одну сборку; solution — группа проектов для удобной работы, сам он ничего не собирает «в файл».

---

## 6. Структура программы: top-level statements и `Main`

Шаблон `dotnet new console` создаёт `Program.cs` из одной строки:

```csharp
Console.WriteLine("Hello, World!");   // вывод: Hello, World!
```

Это **top-level statements** (инструкции верхнего уровня, C# 9+). Компилятор сам оборачивает их в класс и метод `Main`. Классический эквивалент:

```csharp
namespace MyApp;

internal static class Program
{
    // Точка входа (entry point): CLR вызывает Main при старте процесса.
    private static int Main(string[] args)
    {
        Console.WriteLine($"Аргументов: {args.Length}");   // dotnet run -- a b → вывод: Аргументов: 2
        return 0;   // код возврата: 0 — успех, не 0 — ошибка (его видят скрипты и CI)
    }
}
```

- `Main` может быть `void` или `int`, с `string[] args` или без, и `async Task`/`async Task<int>` (модуль 13).
- В top-level statements `args` доступен как «магическая» переменная, а `return 1;` задаёт код возврата.
- Top-level statements допустимы **только в одном файле** проекта.

**Namespaces и using:**

```csharp
using System.Text;                     // теперь можно писать StringBuilder вместо System.Text.StringBuilder
using Json = System.Text.Json.JsonSerializer;   // alias (псевдоним)

namespace Shop.Orders;                 // file-scoped namespace (C# 10): на весь файл, без лишних { }

public class Order { }
```

- `global using System.Text;` в одном файле — действует на весь проект.
- `ImplicitUsings` добавляет набор global using автоматически: `System`, `System.IO`, `System.Linq`, `System.Collections.Generic`, `System.Threading.Tasks` и др. А вот `System.Text` и `System.Globalization` — **нет**, их подключают вручную.

> **Почему так?** Top-level statements убрали церемонию «класс + static Main» для маленьких программ и обучения, но в IL ничего не поменялось — это синтаксический сахар (syntactic sugar — удобная запись, которую компилятор разворачивает в обычный код). File-scoped namespace экономит уровень отступа во *всех* файлах проекта — а 99% файлов содержат один namespace.

> 💼 **На собеседовании:** «Что такое точка входа?» → метод `Main`; при top-level statements компилятор генерирует его сам. «Можно ли иметь два `Main`?» → можно, но тогда нужно указать в `.csproj` `<StartupObject>`, иначе ошибка компиляции.

---

## 7. Консольный ввод и вывод

```csharp
Console.Write("Как вас зовут? ");          // без перевода строки
string? name = Console.ReadLine();         // string? — может вернуть null (конец ввода, Ctrl+Z)
Console.WriteLine($"Привет, {name}!");     // интерполяция строк ($"...") — модуль 02

Console.Write("Сколько вам лет? ");
if (int.TryParse(Console.ReadLine(), out var age))   // безопасный парсинг: не бросает исключение
{
    Console.WriteLine($"Через 10 лет вам будет {age + 10}");
}
else
{
    Console.Error.WriteLine("Это не число");          // ошибки — в stderr, а не stdout
}
```

- `Console.ReadLine()` возвращает `string?`: при перенаправленном вводе (`app < input.txt`) или Ctrl+Z/Ctrl+D строка кончается и приходит `null`.
- `int.Parse("abc")` бросит `FormatException`, а `int.TryParse` вернёт `false` — для пользовательского ввода используйте `TryParse` (подробно в модуле 02).
- Кириллица в старой консоли Windows может отображаться «кракозябрами» — помогает `Console.OutputEncoding = System.Text.Encoding.UTF8;` (в Windows Terminal обычно всё в порядке).

> **Почему так?** `ReadLine` возвращает `string?`, а не `string`, потому что «ввод закончился» — нормальная ситуация, и компилятор с включённым `Nullable` заставит вас её обработать. Разделение stdout/stderr позволяет перенаправить результат программы в файл (`app > out.txt`), а ошибки всё равно увидеть в терминале.

**Отделяйте логику от ввода-вывода.** Метод `TipCalculator.CalculateTip(bill, percent)` легко протестировать; метод, который сам читает `Console.ReadLine()` и печатает результат, — почти невозможно. Поэтому во всех задачах курса логика лежит в отдельных классах, а `Program.cs` только читает ввод, вызывает их и печатает.

→ **Задача 1** (консольное приложение с нуля) · **Задача 2** (FizzBuzz) · **Задача 3** (Greeter) · **Задача 4** (чаевые) · **Задача 5** (конвертер)

---

## 8. Отладка: breakpoints, пошаговое выполнение и SharpLab

**Debugger** (отладчик) позволяет остановить программу и посмотреть её состояние:

| Действие | Visual Studio / Rider | VS Code |
|----------|----------------------|---------|
| Поставить breakpoint (точку останова) | F9 / Ctrl+F8 | F9 |
| Запустить с отладкой | F5 | F5 |
| Step Over — выполнить строку целиком | F10 / F8 | F10 |
| Step Into — зайти внутрь метода | F11 / F7 | F11 |
| Step Out — выйти из метода | Shift+F11 / Shift+F8 | Shift+F11 |
| Continue — до следующего breakpoint | F5 / F9 | F5 |

Полезное: окна **Locals/Variables** (локальные переменные), **Watch** (свои выражения), **Call Stack** (стек вызовов — кто кого вызвал), **conditional breakpoint** (останов только если `i == 42`) и отладка **тестов** (Debug Test в IDE).

Сборки бывают **Debug** (по умолчанию: без оптимизаций, удобно отлаживать) и **Release** (`dotnet build -c Release`: оптимизации, так собирают для продакшена). В Release часть переменных может быть «optimized away» — отладчик их не покажет.

**SharpLab** (https://sharplab.io) — онлайн-инструмент: пишете C# слева, справа видите IL, «развёрнутый» компилятором C# (во что превратились top-level statements, `foreach`, интерполяция) или даже машинный код после JIT.

> **Почему так?** `Console.WriteLine` в каждой строке — медленный способ искать баги: нужно угадать, где печатать, и потом это убирать. Отладчик показывает **всё** состояние в момент остановки, включая стек вызовов. А SharpLab — лучший способ ответить себе на вопрос «во что это компилируется?» вместо заучивания: вы *видите* `box` при боксинге или state machine у `async`.

→ **Задача 6** (исследование: отладчик + IL)

---

## Типичные ошибки

1. **Путать SDK и Runtime**: «Установил runtime, а `dotnet build` не работает» — для сборки нужен SDK.
2. **Путать namespace и assembly**: «Добавил `using Newtonsoft.Json;`, а оно не компилируется» — `using` только сокращает имена; саму сборку нужно подключить пакетом/ссылкой.
3. **Коммитить `bin/` и `obj/`** — это результат сборки, он генерируется. В git — только исходники.
4. **Логика внутри `Program.cs` вперемешку с `Console`** — невозможно протестировать. Выносите логику в методы/классы.
5. **`int.Parse` на пользовательском вводе** — падение на первом же «abc». Используйте `TryParse`.
6. **Форматировать числа без культуры**: `12.5.ToString()` на русской Windows даст `"12,5"`. Для машинного вывода — `CultureInfo.InvariantCulture`.
7. **Целочисленное деление**: `9 / 5` равно `1`, а не `1.8`. Нужно `9.0 / 5`.
8. **Говорить «C# компилируется в машинный код»** — нет, в IL; в машинный код его переводит JIT (или AOT).

## Шпаргалка

```bash
dotnet --info                          # что установлено
dotnet new console -n App && cd App    # новый проект
dotnet run -- arg1 arg2                # запуск с аргументами (после --)
dotnet build -c Release                # релизная сборка
dotnet test --filter "FullyQualifiedName~GreeterTests"   # тесты одного класса
dotnet add package <Id>                # NuGet-пакет
dotnet new sln && dotnet sln add App/App.csproj
```

```csharp
// Program.cs (top-level statements)
using System.Globalization;
Console.Write("Сумма: ");
if (!decimal.TryParse(Console.ReadLine(), NumberStyles.Number, CultureInfo.InvariantCulture, out var bill))
{
    Console.Error.WriteLine("Не число");
    return 1;                           // код ошибки
}
Console.WriteLine($"Чаевые 10%: {bill * 0.1m}");
return 0;
```

Путь кода: **C# → (Roslyn) → IL + metadata в assembly → (CLR: JIT при первом вызове метода) → машинный код**.

## Глоссарий

| Термин | Что значит |
|--------|-----------|
| .NET | современная кроссплатформенная платформа (бывший .NET Core) |
| .NET Framework | старая Windows-only версия платформы (до 4.8.1) |
| .NET Standard | спецификация общего набора API для библиотек |
| SDK | набор для разработки: компилятор, MSBuild, CLI, шаблоны (+ runtime) |
| Runtime | всё необходимое для запуска приложения (CLR + библиотеки) |
| CLR | Common Language Runtime — виртуальная машина .NET: JIT, GC, type safety, исключения |
| IL / CIL / MSIL | промежуточный язык, в который компилируется C# |
| JIT | Just-In-Time компилятор: IL → машинный код при первом вызове метода |
| AOT | Ahead-Of-Time: компиляция в машинный код заранее (ReadyToRun, Native AOT) |
| Tiered compilation | быстрая неоптимизированная компиляция, затем перекомпиляция «горячих» методов |
| BCL | Base Class Library — стандартная библиотека классов |
| Assembly | сборка: `.dll`/`.exe` с IL, метаданными и manifest |
| Metadata | описание типов и членов внутри сборки (на нём работает reflection) |
| Managed code | код, выполняемый под управлением CLR |
| NuGet | менеджер пакетов .NET |
| MSBuild | система сборки, читающая `.csproj` |
| Roslyn | компилятор C# (и VB) с открытым API |
| Top-level statements | код программы без явного класса и `Main` |
| Entry point | точка входа — метод `Main` |
| Breakpoint | точка останова в отладчике |
| Self-contained | публикация вместе с runtime |
| CTS | Common Type System — общая система типов для всех .NET-языков |

## Вопросы на собеседовании

<details><summary><b>Что такое CLR и за что он отвечает?</b></summary>

CLR (Common Language Runtime) — среда выполнения .NET, «виртуальная машина». Она загружает сборки, JIT-компилирует IL в машинный код, управляет памятью через Garbage Collector, обеспечивает type safety, обработку исключений и работу с потоками. Код, работающий под CLR, называется managed code.
</details>

<details><summary><b>Что происходит между написанием кода на C# и его выполнением процессором?</b></summary>

Компилятор Roslyn превращает C# в IL и метаданные и записывает их в сборку (.dll). При запуске CLR загружает сборку; когда метод вызывается впервые, JIT компилирует его IL в машинный код под конкретный процессор и кэширует результат. Благодаря tiered compilation часто вызываемые методы позже перекомпилируются с оптимизациями. Альтернатива — AOT (ReadyToRun, Native AOT), где машинный код создаётся заранее.
</details>

<details><summary><b>Зачем нужен промежуточный язык IL, почему не компилировать сразу в машинный код?</b></summary>

Переносимость: одна сборка работает на Windows/Linux/macOS и x64/ARM, машинный код создаётся на месте. Языковая независимость: C#, F#, VB компилируются в один IL и используют библиотеки друг друга. JIT может оптимизировать под конкретный процессор и по статистике выполнения. Минус — время на JIT при старте, его решают ReadyToRun и Native AOT.
</details>

<details><summary><b>Чем отличаются .NET Framework, .NET Core/.NET и .NET Standard?</b></summary>

.NET Framework — исходная платформа только для Windows, сейчас только поддерживается. .NET (бывший .NET Core) — кроссплатформенная open source реализация, на ней все новые проекты (.NET 5+ — единое название). .NET Standard — не платформа, а спецификация набора API: библиотека под netstandard2.0 работает и там и там. Для новых библиотек обычно таргетят современный .NET напрямую.
</details>

<details><summary><b>Чем SDK отличается от Runtime?</b></summary>

Runtime нужен, чтобы запускать приложения: CLR и библиотеки. SDK — для разработки: компилятор, MSBuild, dotnet CLI, шаблоны; он включает runtime. На сервере ставят runtime или публикуют приложение self-contained, чтобы вообще не зависеть от установленного .NET.
</details>

<details><summary><b>Что такое assembly и чем она отличается от namespace?</b></summary>

Assembly — физическая единица развёртывания: файл .dll/.exe с IL, метаданными и manifest (имя, версия, зависимости). Namespace — логическая группировка имён типов. Они независимы: одна сборка может содержать много namespace, а один namespace (например, `System`) разбросан по многим сборкам. `using` лишь сокращает имена и не подключает сборку.
</details>

<details><summary><b>Что такое JIT и какие у него плюсы и минусы?</b></summary>

JIT (Just-In-Time) компилирует IL метода в машинный код при первом вызове. Плюсы: переносимость IL, оптимизации под конкретный процессор, tiered compilation по реальной статистике. Минусы: задержка при старте (cold start) и расход памяти на компиляцию. Для быстрого старта используют ReadyToRun или Native AOT.
</details>

<details><summary><b>Что такое NuGet и как подключить пакет?</b></summary>

NuGet — менеджер пакетов .NET; пакет — архив со сборками под разные target frameworks и описанием зависимостей. Подключение: `dotnet add package <Id>` или через IDE — в `.csproj` появляется `<PackageReference>`. Пакеты восстанавливаются командой `dotnet restore` (автоматически при сборке), поэтому в git их не храним.
</details>

<details><summary><b>Что такое top-level statements? Куда делся Main?</b></summary>

Это возможность C# 9 писать код программы прямо в файле, без класса и `Main`. Компилятор сам генерирует класс `Program` и метод-точку входа; `args` доступны как переменная, `return` задаёт код возврата. Это только синтаксический сахар — в IL всё как раньше. Top-level statements могут быть только в одном файле проекта.
</details>

<details><summary><b>Что такое managed и unmanaged code?</b></summary>

Managed code выполняется под управлением CLR: память освобождает GC, проверяется type safety, работают исключения .NET. Unmanaged code (C/C++, системные API) работает напрямую с ОС, и память там освобождается вручную. .NET умеет вызывать unmanaged код (P/Invoke), а неуправляемые ресурсы (файлы, сокеты) нужно освобождать явно — об этом `IDisposable` в модуле 07.
</details>

<details><summary><b>Чем Debug-сборка отличается от Release?</b></summary>

Debug — без оптимизаций компилятора, с полной отладочной информацией: удобно ставить breakpoints и смотреть переменные. Release — с оптимизациями (и в C#-компиляторе, и в JIT), так собирают для продакшена; отладка затруднена, часть переменных «optimized away». Замеры производительности имеют смысл только в Release.
</details>

<details><summary><b>Что такое .csproj и что в нём обычно лежит?</b></summary>

Файл проекта MSBuild в XML. В нём: SDK проекта (`Microsoft.NET.Sdk`, `.Web`), `TargetFramework`, тип выхода (`OutputType`), настройки вроде `Nullable`, ссылки на пакеты (`PackageReference`) и проекты (`ProjectReference`). В SDK-style проектах `.cs` файлы включаются автоматически, поэтому файл короткий. Общие настройки нескольких проектов выносят в `Directory.Build.props`.
</details>

## Дополнительно

- [Обзор .NET](https://learn.microsoft.com/dotnet/core/introduction) и [Что такое managed code](https://learn.microsoft.com/dotnet/standard/managed-code)
- [Common Language Runtime (CLR)](https://learn.microsoft.com/dotnet/standard/clr)
- [Обзор `dotnet` CLI](https://learn.microsoft.com/dotnet/core/tools/)
- [Top-level statements](https://learn.microsoft.com/dotnet/csharp/fundamentals/program-structure/top-level-statements)
- [.NET Standard](https://learn.microsoft.com/dotnet/standard/net-standard)
- [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)
- [Отладка в Visual Studio для начинающих](https://learn.microsoft.com/visualstudio/debugger/debugger-feature-tour)
- SharpLab: https://sharplab.io
- Джеффри Рихтер, «CLR via C#» — главы 1–2 (как CLR загружает и выполняет сборки). Книга про .NET Framework, но модель CLR/IL/JIT не изменилась.
