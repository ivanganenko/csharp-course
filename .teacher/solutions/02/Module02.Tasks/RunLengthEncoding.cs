using System.Text;

namespace Module02.Tasks;

public static class RunLengthEncoding
{
    public static string Encode(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
        {
            return "";
        }

        // StringBuilder: результат собирается по кусочку в цикле — именно для этого он и нужен.
        // Ёмкость (capacity) заранее не угадать точно, но text.Length — разумная оценка.
        var sb = new StringBuilder(text.Length);

        var current = text[0];
        var count = 0;

        foreach (var c in text)
        {
            if (char.IsAsciiDigit(c))
            {
                // Цифры во входе сделали бы формат неоднозначным: "a12" — это 'a'×12 или 'a','1','2'?
                throw new ArgumentException("Текст не должен содержать цифр.", nameof(text));
            }

            if (c == current)
            {
                count++;
                continue;
            }

            sb.Append(current).Append(count);
            current = c;
            count = 1;
        }

        // Последняя серия не «закрывается» внутри цикла — классическая ошибка забыть её дописать.
        sb.Append(current).Append(count);

        return sb.ToString();
    }

    public static string Decode(string encoded)
    {
        ArgumentNullException.ThrowIfNull(encoded);

        var sb = new StringBuilder();
        var i = 0;

        while (i < encoded.Length)
        {
            var symbol = encoded[i];
            if (char.IsAsciiDigit(symbol))
            {
                throw new FormatException($"Ожидался символ, а встречена цифра в позиции {i}.");
            }

            i++;

            // Читаем многозначное число вручную: count = count * 10 + цифра.
            // c - '0' — char это число (UTF-16 код), и коды '0'..'9' идут подряд.
            var count = 0;
            var digitsRead = 0;
            while (i < encoded.Length && char.IsAsciiDigit(encoded[i]))
            {
                count = checked(count * 10 + (encoded[i] - '0')); // checked: "a99999999999" → OverflowException, а не мусор
                digitsRead++;
                i++;
            }

            if (digitsRead == 0 || count == 0)
            {
                throw new FormatException($"После символа '{symbol}' ожидалось число ≥ 1.");
            }

            sb.Append(symbol, count); // Append(char, repeatCount) — без цикла и без промежуточных строк
        }

        return sb.ToString();
    }
}
