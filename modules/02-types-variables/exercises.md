# Модуль 02 — задачи

> Код пишется в `starter/Module02.Tasks/`. Тесты: `cd modules/02-types-variables/starter && dotnet test`.
> Задача 6 — 🧪 исследование без тестов: результат — ваши замеры и письменные ответы.

---

## Задача 1. Безопасный парсинг ввода  ·  ⏱ ~30 мин  ·  🟢 базовая

**Что сделать:** реализуйте `InputParser`: `ParseInt`, `ParseAge`, `ParseAmount` (точка или запятая), `ParseYesNo` (`"да"`/`"yes"`/`"1"`…). Ни один метод не бросает исключений на плохом вводе — возвращает `null` (`int?`, `decimal?`, `bool?`).

**Зачем:** любой реальный ввод (консоль, форма, CSV, query string) может быть мусором. Отрабатываете `TryParse`, `NumberStyles`, `CultureInfo.InvariantCulture` и nullable value types — на собеседовании часто спрашивают «Parse vs TryParse vs Convert».

**Файл:** `Module02.Tasks/InputParser.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~InputParserTests"`

**Подумай / ответь письменно:**
- Почему `TryParse`, а не `Parse` внутри `try/catch`? Назовите минимум две причины.
- Почему `"1,000"` с `NumberStyles.Number` превратилось бы в `1000` (или в `1`, если заменить запятую на точку), и почему мы не разрешаем разделители тысяч?

---

## Задача 2. Переполнение и `checked`  ·  ⏱ ~25 мин  ·  🟢 базовая

**Что сделать:** реализуйте `SafeMath`: `AddChecked` (исключение при переполнении), `TryAdd` (Try-паттерн с `out`), `Midpoint` (середина без переполнения), `ToIntChecked` (long → int с проверкой), `SumAsLong`.

**Зачем:** переполнение — тихий баг: счётчик просмотров стал отрицательным, сумма заказа «обнулилась». `Midpoint` — тот самый баг из бинарного поиска (пригодится в модуле 03), а на интервью любят спросить «что будет при `int.MaxValue + 1`?».

**Файл:** `Module02.Tasks/SafeMath.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~SafeMathTests"`

**Подумай / ответь письменно:**
- Почему C# по умолчанию НЕ проверяет переполнение? В каких проектах вы бы включили `CheckForOverflowUnderflow` для всего проекта?
- Почему `(low + high) / 2` ломается, а `low + (high - low) / 2` — нет?

---

## Задача 3. Денежные расчёты  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуйте `MoneyCalculator`: скидка, НДС, **точное деление суммы на части** (`Split(100, 3)` → `33.34, 33.33, 33.33`, сумма частей строго равна исходной) и сложные проценты с округлением только в конце.

**Зачем:** «разделить счёт», «распределить скидку по позициям», «начислить проценты» — повседневные задачи в финтехе и e-commerce. Проверяется понимание `decimal`, правил округления и того, что копейки не должны появляться и исчезать.

**Файл:** `Module02.Tasks/MoneyCalculator.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~MoneyCalculatorTests"`

**Подумай / ответь письменно:**
- Почему в `Split` нельзя просто сделать `Math.Round(total / parts, 2)` для каждой части?
- Что такое banker's rounding, и почему оно — поведение `Math.Round` по умолчанию?

---

## Задача 4. Строковые утилиты  ·  ⏱ ~40 мин  ·  🟡 средняя

**Что сделать:** реализуйте `StringTools`: `IsPalindrome` (только буквы/цифры, без учёта регистра), `CountWords`, `NormalizeSpaces`, `SameLogin` (сравнение без учёта регистра и пробелов по краям), `MaskCardNumber` (`**** **** **** 3456`).

**Зачем:** строки — самая частая тема live coding (палиндром — в топ-5). Отрабатываете `char`-методы, `Split`/`Join`, `StringComparison` и immutability: попробуйте сделать `IsPalindrome` без создания новых строк.

**Файл:** `Module02.Tasks/StringTools.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~StringToolsTests"`

**Подумай / ответь письменно:**
- Какая сложность по времени и памяти у вашего `IsPalindrome`? Сколько новых строк он создаёт?
- Почему `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` лучше `a.ToLower() == b.ToLower()`?
- Почему в `MaskCardNumber` невозможно «поменять символ в строке на месте»?

---

## Задача 5. Чек через StringBuilder  ·  ⏱ ~30 мин  ·  🟡 средняя

**Что сделать:** реализуйте `ReceiptBuilder.BuildReceipt(number, date, lines)` — текст чека строго заданного формата: заголовок с датой `yyyy-MM-dd`, строки `Tea x2 = 5.00`, итог `Total: 8.50`, разделитель строк `'\n'`, числа с точкой независимо от культуры. Тип `ReceiptLine` уже готов.

**Зачем:** генерация отчётов, писем, CSV, логов — постоянная задача. Учит `StringBuilder`, форматированию `decimal` и `DateOnly` с культурой и тому, что `AppendLine`/`Environment.NewLine` зависят от ОС.

**Файл:** `Module02.Tasks/ReceiptBuilder.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~ReceiptBuilderTests"`

**Подумай / ответь письменно:**
- Сколько строк создалось бы при `receipt += ...` для чека из 1000 позиций? Какая сложность?
- Почему тест падал бы на Windows при использовании `AppendLine`, а на Linux — нет?

---

## Задача 6. Исследование: value vs reference и boxing  ·  ⏱ ~40 мин  ·  🧪 исследование

**Что сделать:** создайте отдельное консольное приложение (`dotnet new console -n BoxingLab` вне папки курса).

**Часть A — предскажите, потом запустите.** Для каждого фрагмента **сначала запишите ожидаемый вывод**, потом проверьте:
```csharp
int a = 5; int b = a; b++;
Console.WriteLine($"{a} {b}");

int[] x = [1, 2]; int[] y = x; y[0] = 100;
Console.WriteLine(x[0]);

int n = 42; object o = n; n = 7;
Console.WriteLine(o);

object boxed = 42;
try { long l = (long)boxed; Console.WriteLine(l); }
catch (InvalidCastException) { Console.WriteLine("InvalidCastException"); }

string s1 = "abc"; string s2 = s1; s2 += "d";
Console.WriteLine(s1);
```

**Часть B — замер аллокаций.** Сравните выделенную память при добавлении 1 000 000 чисел в `ArrayList` (`using System.Collections;`) и в `List<int>`:
```csharp
var before = GC.GetAllocatedBytesForCurrentThread();
// ... заполнение коллекции ...
var after = GC.GetAllocatedBytesForCurrentThread();
Console.WriteLine($"Выделено: {(after - before) / 1024 / 1024} МБ");
```
Запустите в Release (`dotnet run -c Release`). Во сколько раз отличаются цифры? Откуда разница?

**Часть C — IL.** На https://sharplab.io найдите инструкции `box` и `unbox.any` в коде из части A. Проверьте: есть ли `box` в `Console.WriteLine($"{n}")` для `int n`? А в `string.Format("{0}", n)`?

**Зачем:** «объясните boxing» и «что выведет код» про value/reference — обязательная часть любого Junior-интервью. Цифры, полученные своими руками, запоминаются лучше определений.

**Подумай / ответь письменно:**
- Почему изменение `n` после `object o = n` не меняет `o`?
- Почему `ArrayList` тратит больше памяти, чем `List<int>`? Сколько объектов в heap создаётся на каждый `Add`?
- Почему `s1` не изменилась, хотя `string` — reference type?

---

## Задача 7. Run-Length Encoding  ·  ⏱ ~45 мин  ·  🔴 сложная  ·  ⭐ со звёздочкой

**Что сделать:** реализуйте `RunLengthEncoding.Encode("aaabccdddd")` → `"a3b1c2d4"` и `Decode` обратно, включая многозначные счётчики (`"x12"`), валидацию (цифры во входе `Encode` → `ArgumentException`, неверный формат `Decode` → `FormatException`).

**Зачем:** классическая задача live coding на работу со строками и `char`: обработка серий, «последний элемент после цикла», ручной разбор чисел (`c - '0'`), `StringBuilder`, граничные случаи. Интервьюер будет смотреть на edge cases и сложность.

**Файл:** `Module02.Tasks/RunLengthEncoding.cs`
**Тесты:** `dotnet test --filter "FullyQualifiedName~RunLengthEncodingTests"`

**Подумай / ответь письменно:**
- Какая сложность `Encode` и `Decode`? Почему с `+=` строк она была бы хуже?
- Почему `'7' - '0'` равно `7`? Что такое `char` «на самом деле»?
- Почему во входе `Encode` запрещены цифры? Как изменить формат, чтобы они стали допустимы?
