namespace Module01.Tasks;

public static class Greeter
{
    public static string Greet(string? name)
    {
        // string.IsNullOrWhiteSpace покрывает сразу null, "" и "   " —
        // это идиоматичнее, чем три отдельные проверки.
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Hello, World!";
        }

        // Строки в C# immutable: Trim() возвращает НОВУЮ строку, исходная не меняется.
        return $"Hello, {name.Trim()}!";
    }

    public static string GreetingForHour(int hour)
    {
        // Сначала валидация (guard clause) — так основная логика не загромождена проверками.
        ArgumentOutOfRangeException.ThrowIfNegative(hour);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hour, 23);

        // switch expression + relational patterns (C# 9+): компактно и без "дыр" в диапазонах.
        return hour switch
        {
            >= 5 and <= 11 => "Good morning",
            >= 12 and <= 17 => "Good afternoon",
            >= 18 and <= 22 => "Good evening",
            _ => "Good night",
        };
    }
}
