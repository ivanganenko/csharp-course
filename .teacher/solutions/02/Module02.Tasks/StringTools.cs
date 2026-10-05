namespace Module02.Tasks;

public static class StringTools
{
    public static bool IsPalindrome(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // Два индекса навстречу друг другу: O(n) по времени и O(1) по памяти.
        // Вариант «отфильтровать в новую строку, развернуть и сравнить» проще, но создаёт
        // 2–3 новые строки — строки immutable, каждая операция = новая аллокация.
        var left = 0;
        var right = text.Length - 1;

        while (left < right)
        {
            if (!char.IsLetterOrDigit(text[left]))
            {
                left++;
                continue;
            }

            if (!char.IsLetterOrDigit(text[right]))
            {
                right--;
                continue;
            }

            // ToLowerInvariant — сравнение не зависит от культуры машины.
            if (char.ToLowerInvariant(text[left]) != char.ToLowerInvariant(text[right]))
            {
                return false;
            }

            left++;
            right--;
        }

        return true;
    }

    public static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        // separator == null означает «любой whitespace» (пробел, \t, \n…).
        // Приведение (char[]?) нужно, чтобы компилятор выбрал перегрузку Split(char[], options),
        // иначе вызов Split(null, ...) неоднозначен (есть и Split(string?, options)).
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static string NormalizeSpaces(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        // Split убирает все пробельные символы, Join склеивает слова одним пробелом.
        // Альтернатива — Regex.Replace(text.Trim(), @"\s+", " "): короче, но медленнее
        // и требует знания регулярных выражений от читателя.
        var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words);
    }

    public static bool SameLogin(string? a, string? b)
    {
        if (a is null || b is null)
        {
            // Два null равны, null и строка — нет. ReferenceEquals(null, null) == true.
            return a is null && b is null;
        }

        // StringComparison.OrdinalIgnoreCase: побайтовое сравнение без учёта регистра,
        // НЕ зависит от культуры и быстрее культурного. Для идентификаторов (логины, ключи,
        // пути, имена файлов) — всегда Ordinal/OrdinalIgnoreCase.
        // a.ToLower() == b.ToLower() хуже: две лишние строки и зависимость от культуры.
        return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static string MaskCardNumber(string cardNumber)
    {
        ArgumentNullException.ThrowIfNull(cardNumber);

        var digitCount = 0;
        foreach (var c in cardNumber)
        {
            if (char.IsAsciiDigit(c))
            {
                digitCount++;
            }
        }

        var digitsToMask = digitCount - 4;
        if (digitsToMask <= 0)
        {
            return cardNumber;
        }

        // Строку изменить нельзя (immutable), поэтому копируем символы в изменяемый массив,
        // правим его и создаём ОДНУ новую строку в конце.
        var chars = cardNumber.ToCharArray();
        for (var i = 0; i < chars.Length && digitsToMask > 0; i++)
        {
            if (char.IsAsciiDigit(chars[i]))
            {
                chars[i] = '*';
                digitsToMask--;
            }
        }

        return new string(chars);
    }
}
