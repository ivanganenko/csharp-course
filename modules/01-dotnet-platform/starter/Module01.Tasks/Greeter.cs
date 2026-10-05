namespace Module01.Tasks;

/// <summary>
/// Задача 01.3 — первая «настоящая» задача: реализуйте методы так, чтобы прошли тесты в GreeterTests.
/// Запуск тестов: dotnet test (из папки starter).
/// </summary>
public static class Greeter
{
    /// <summary>
    /// Возвращает приветствие "Hello, {name}!".
    /// Если name равно null, пустое или состоит только из пробелов — "Hello, World!".
    /// Пробелы по краям имени нужно убрать: "  Anna " → "Hello, Anna!".
    /// </summary>
    public static string Greet(string? name)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Приветствие в зависимости от часа суток (0–23):
    /// 5–11 → "Good morning", 12–17 → "Good afternoon", 18–22 → "Good evening", иначе → "Good night".
    /// Для часа вне диапазона 0–23 бросьте ArgumentOutOfRangeException.
    /// </summary>
    public static string GreetingForHour(int hour)
    {
        throw new NotImplementedException();
    }
}
