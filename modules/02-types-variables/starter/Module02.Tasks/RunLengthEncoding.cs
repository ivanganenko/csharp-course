namespace Module02.Tasks;

/// <summary>
/// Задача 02.7 ⭐ — Run-Length Encoding (сжатие повторов) — классика live coding.
/// Тесты: dotnet test --filter "FullyQualifiedName~RunLengthEncodingTests"
/// </summary>
public static class RunLengthEncoding
{
    /// <summary>
    /// Кодирует строку: каждая серия одинаковых подряд идущих символов → символ + длина серии.
    /// "aaabccdddd" → "a3b1c2d4", "aaaaaaaaaaaa" → "a12", "aAa" → "a1A1a1" (регистр важен), "" → "".
    /// Входная строка не должна содержать цифр (иначе декодирование неоднозначно) → ArgumentException.
    /// text == null → ArgumentNullException.
    /// Для эффективности используйте StringBuilder.
    /// </summary>
    public static string Encode(string text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Обратное преобразование: "a3b1c2d4" → "aaabccdddd", "x12" → двенадцать 'x', "" → "".
    /// Каждый символ-не-цифра обязан иметь после себя число ≥ 1 (может быть многозначным).
    /// Некорректный формат ("3a", "ab3", "a", "a0") → FormatException. encoded == null → ArgumentNullException.
    /// </summary>
    public static string Decode(string encoded)
    {
        throw new NotImplementedException();
    }
}
