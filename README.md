# C# и .NET: от нуля до Junior-собеседования

Практический курс по C# с агентом-преподавателем на базе Claude Code. Много задач с автоматическими тестами, самая важная теория и вопросы, которые реально задают на собеседованиях Junior .NET Developer.

**Для кого:** вы немного программировали (переменные, циклы, функции), но ещё не работали разработчиком.
**Формат:** 17 модулей. В каждом — теория порциями, 5–8 задач с тестами, вопросы с собеседований, mock interview.
**Язык:** русский, термины — на английском (как в документации и на собеседованиях).
**Платформа:** .NET 10 (LTS), C# 14.

## Программа

| # | Модуль | Ключевые вопросы интервью |
|---|--------|---------------------------|
| 01 | Платформа .NET и инструменты | CLR, IL, JIT, BCL, SDK vs Runtime |
| 02 | Типы данных и переменные | value vs reference type, boxing/unboxing, immutable string |
| 03 | Управляющие конструкции и методы | pattern matching, `ref`/`out`/`in`, перегрузка |
| 04 | ООП: классы и инкапсуляция | поля vs свойства, `static`, конструкторы, модификаторы доступа |
| 05 | ООП: наследование, полиморфизм, интерфейсы | `virtual`/`override`/`new`, abstract class vs interface |
| 06 | struct, record и равенство | `Equals`/`GetHashCode`/`==`, immutability |
| 07 | Исключения и IDisposable | `throw` vs `throw ex`, `finally`, `using` |
| 08 | Generics | constraints, covariance/contravariance |
| 09 | Коллекции и сложность | как устроен `Dictionary`, Big-O |
| 10 | Делегаты, события, лямбды | `Func`/`Action`, closures, event vs delegate |
| 11 | LINQ | deferred execution, `IEnumerable` vs `IQueryable` |
| 12 | Память и Garbage Collector | stack/heap, поколения, LOH, финализаторы |
| 13 | Асинхронность и многопоточность | `Task` vs `Thread`, `async`/`await`, deadlock, `lock` |
| 14 | Unit-тестирование | AAA, mocks, xUnit |
| 15 | ASP.NET Core Web API | DI lifetimes, middleware, REST |
| 16 | EF Core и SQL | миграции, N+1, tracking, индексы |
| 17 | SOLID, паттерны, финальный проект, mock interview | SOLID, паттерны, «расскажите о проекте» |

## Что нужно установить

1. **.NET 10 SDK** — https://dotnet.microsoft.com/download. Проверка: `dotnet --version` (должно начинаться с `10.`).
2. **IDE** (любая):
   - **JetBrains Rider** — бесплатен для некоммерческого использования, отличные подсказки;
   - **Visual Studio 2026 Community** (Windows);
   - **VS Code** + расширение C# Dev Kit.
3. **Git**.
4. **Claude Code** — https://code.claude.com/docs. Windows (PowerShell): `irm https://claude.ai/install.ps1 | iex`.

## Как начать

```bash
git clone <url-этого-репозитория> csharp-course
cd csharp-course
claude
```

В сессии напишите `/lesson` — агент познакомится с вами, проверит окружение и начнёт первый модуль.

## Как устроены задачи

У каждого модуля есть папка `starter/` с решением из двух проектов:

```
modules/05-oop-inheritance/starter/
  Module05.slnx
  Module05.Tasks/      ← здесь вы пишете код (методы сейчас бросают NotImplementedException)
  Module05.Tests/      ← тесты, которые проверяют вашу реализацию (не меняйте их)
```

Цикл работы:

```bash
cd modules/05-oop-inheritance/starter
dotnet test                                          # все тесты модуля
dotnet test --filter "FullyQualifiedName~ShapeTests" # тесты одной задачи
```

Красные тесты → пишете код → зелёные тесты → `/check` → code review от агента.

**Почему через тесты?** Тест — это точная спецификация: нет споров «правильно или нет», а вы сразу привыкаете к workflow реальной команды, где код без тестов не принимают. Плюс на собеседованиях часто дают задачу «сделай, чтобы тесты прошли».

## Команды агента

| Команда | Что делает |
|---------|-----------|
| `/lesson [NN]` | начать или продолжить урок |
| `/check [MM.N]` | прогнать тесты и сделать code review задачи |
| `/hint` | подсказка без готового решения |
| `/interview [NN \| 01-05 \| all]` | mock interview как на собеседовании |
| `/quiz [NN]` | быстрый тест: «что выведет код», найди ошибку |
| `/code-review <путь>` | ревью любого вашего кода |
| `/explain <тема>` | объяснение понятия с примером и ответом «как на собеседовании» |
| `/progress` | ваш прогресс и слабые темы |

## Структура репозитория

```
CLAUDE.md                 инструкции агента-преподавателя
PROGRESS.md               ваш прогресс (ведёт агент)
Directory.Build.props     общие настройки всех проектов
modules/NN-*/             README.md (теория), exercises.md (задачи), starter/ (код + тесты)
.teacher/                 эталонные решения, критерии ревью, ответы на вопросы интервью (для агента)
tools/verify-solutions.*  проверка, что все эталоны проходят тесты (для автора курса)
.claude/                  skills и настройки агента
```

## Для автора курса: проверка эталонов

```powershell
pwsh tools/verify-solutions.ps1          # все модули
pwsh tools/verify-solutions.ps1 -Module 05
```

Скрипт копирует каждый `starter` в `.verify/`, накладывает эталон из `.teacher/solutions/NN/` (та же структура, что у `starter/`) и запускает `dotnet test`. Все тесты должны пройти.
