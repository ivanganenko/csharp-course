using System.Globalization;

namespace Module03.Tasks;

public static class DurationParser
{
    private const int MinutesPerHour = 60;

    public static bool TryParse(string? input, out int totalMinutes)
    {
        // out-параметр обязан получить значение на каждом пути выхода — присваиваем сразу,
        // тогда все `return false` ниже автоматически оставляют 0 (контракт как у int.TryParse).
        totalMinutes = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var text = input.Trim().ToLowerInvariant();
        var hours = 0;
        var minutes = 0;

        var hIndex = text.IndexOf('h');
        var hasHours = hIndex >= 0;
        if (hasHours)
        {
            // Range-синтаксис [..hIndex] — подстрока до 'h' (C# 8).
            if (!TryParseDigits(text[..hIndex], out hours))
            {
                return false;
            }

            text = text[(hIndex + 1)..];
        }

        if (text.Length > 0)
        {
            if (!text.EndsWith('m') || !TryParseDigits(text[..^1], out minutes))
            {
                return false;
            }

            if (hasHours && minutes >= MinutesPerHour)
            {
                return false;
            }
        }
        else if (!hasHours)
        {
            return false; // сюда не попасть при непустом вводе, но явная ветка делает контракт очевидным
        }

        // Считаем в long, чтобы "35791395h" не переполнило int молча.
        var total = (long)hours * MinutesPerHour + minutes;
        if (total > int.MaxValue)
        {
            return false;
        }

        totalMinutes = (int)total;
        return true;
    }

    // NumberStyles.None — только цифры: без знака, пробелов и разделителей.
    // Так "-1", " 30" и "1.5" отбрасываются самим парсером, без ручных проверок.
    // Пустая строка тоже даёт false — это закрывает случаи "h" и "m".
    private static bool TryParseDigits(string text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
}
