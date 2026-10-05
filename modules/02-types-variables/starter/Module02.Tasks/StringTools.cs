namespace Module02.Tasks;

/// <summary>
/// Задача 02.4 — работа со строками и char.
/// Тесты: dotnet test --filter "FullyQualifiedName~StringToolsTests"
/// </summary>
public static class StringTools
{
    /// <summary>
    /// Палиндром ли текст. Учитываются только буквы и цифры (char.IsLetterOrDigit), регистр не важен:
    /// "А роза упала на лапу Азора" → true, "Was it a car or a cat I saw?" → true, "hello" → false.
    /// Пустая строка и строка без букв/цифр → true. text == null → ArgumentNullException.
    /// Подсказка: можно обойтись без создания новых строк — двумя индексами с краёв.
    /// </summary>
    public static bool IsPalindrome(string text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Количество слов. Слова разделены любыми пробельными символами (пробел, \t, \n), в любом количестве.
    /// null, пустая строка, строка из пробелов → 0.
    /// </summary>
    public static int CountWords(string? text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Нормализация пробелов: убрать пробельные символы по краям, а любую последовательность
    /// пробельных символов внутри заменить одним пробелом: "  Hello \t\n  world " → "Hello world".
    /// null → "".
    /// </summary>
    public static string NormalizeSpaces(string? text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Один и тот же ли это логин: сравнение без учёта регистра (ordinal, не зависит от культуры),
    /// пробелы по краям игнорируются: ("Admin", " admin ") → true.
    /// Два null → true; null и не-null → false.
    /// </summary>
    public static bool SameLogin(string? a, string? b)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Маскирует номер карты: все ЦИФРЫ, кроме последних четырёх, заменяются на '*',
    /// остальные символы (пробелы, дефисы) остаются на месте:
    /// "1234 5678 9012 3456" → "**** **** **** 3456", "1234-5678" → "****-5678".
    /// Если цифр 4 или меньше — строка возвращается без изменений. cardNumber == null → ArgumentNullException.
    /// </summary>
    public static string MaskCardNumber(string cardNumber)
    {
        throw new NotImplementedException();
    }
}
