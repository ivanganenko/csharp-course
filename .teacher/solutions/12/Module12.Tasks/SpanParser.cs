using System.Globalization;

namespace Module12.Tasks;

public static class SpanParser
{
    public static int SumCsv(ReadOnlySpan<char> line)
    {
        // IsWhiteSpace для пустого span тоже возвращает true — покрываем оба случая.
        if (line.IsWhiteSpace())
        {
            return 0;
        }

        var sum = 0;
        while (true)
        {
            var comma = line.IndexOf(',');
            // Slice/диапазон не копирует символы — это «окно» на ту же строку.
            // Substring создал бы новую строку в куче на каждый элемент.
            var part = comma < 0 ? line : line[..comma];

            // Перегрузка int.Parse(ReadOnlySpan<char>, ...) разбирает число прямо из окна, без аллокаций.
            // InvariantCulture — чтобы результат не зависел от настроек ОС пользователя.
            // checked — переполнение суммы должно быть ошибкой, а не тихим «перескоком» в минус.
            sum = checked(sum + int.Parse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture));

            if (comma < 0)
            {
                return sum;
            }

            line = line[(comma + 1)..];
        }
    }

    public static int CountWords(ReadOnlySpan<char> text)
    {
        var count = 0;
        var inWord = false;

        // Один проход O(n) без аллокаций: считаем переходы «пробел → не пробел».
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }

    public static bool TryParseKeyValue(ReadOnlySpan<char> input, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        // Try-pattern: out-параметры должны получить значение на любом пути выполнения.
        key = default;
        value = default;

        var separator = input.IndexOf('=');
        if (separator < 0)
        {
            return false;
        }

        var candidateKey = input[..separator].Trim();
        if (candidateKey.IsEmpty)
        {
            return false;
        }

        key = candidateKey;
        value = input[(separator + 1)..].Trim();
        return true;
    }

    public static ReadOnlySpan<char> GetLogLevel(ReadOnlySpan<char> logLine)
    {
        if (logLine.IsEmpty || logLine[0] != '[')
        {
            return ReadOnlySpan<char>.Empty;
        }

        var close = logLine.IndexOf(']');
        if (close < 0)
        {
            return ReadOnlySpan<char>.Empty;
        }

        // Возвращаем окно внутри исходной строки — вызывающий сам решит, нужна ли ему string (ToString()).
        return logLine[1..close];
    }
}
