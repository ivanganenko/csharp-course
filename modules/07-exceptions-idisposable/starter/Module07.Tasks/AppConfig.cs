namespace Module07.Tasks;

/// <summary>
/// Задача 07.2 — ошибка в конфигурации: хранит номер строки (с 1).
/// Готово — менять не нужно (но разберись, зачем здесь innerException).
/// </summary>
public sealed class ConfigParseException : Exception
{
    public ConfigParseException(string message, int lineNumber, Exception? innerException = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
    }

    public int LineNumber { get; }
}

/// <summary>
/// Задача 07.2 — простой парсер конфигурации формата "key=value" (по одной паре на строку).
/// Правила формата:
/// - строки разделяются '\n'; '\r' на конце строки и пробелы по краям игнорируются;
/// - пустые строки и строки, начинающиеся с '#', пропускаются (комментарии);
/// - строка без '=' → ConfigParseException с LineNumber этой строки;
/// - ключ — текст до ПЕРВОГО '=', значение — после (значение может содержать '='); оба обрезаются Trim;
/// - пустой ключ ("=value") → ConfigParseException;
/// - повторный ключ → ConfigParseException с номером строки повтора;
/// - ключи сравниваются без учёта регистра ("Port" и "PORT" — один ключ).
/// </summary>
public sealed class AppConfig
{
    /// <summary>Количество ключей.</summary>
    public int Count => throw new NotImplementedException();

    /// <summary>Разбор текста. text == null → ArgumentNullException.</summary>
    public static AppConfig Parse(string text)
    {
        throw new NotImplementedException();
    }

    /// <summary>Значение ключа. Нет ключа → KeyNotFoundException. key == null → ArgumentNullException.</summary>
    public string GetString(string key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Значение как int (InvariantCulture). Нет ключа → KeyNotFoundException.
    /// Значение не число или не помещается в int → ConfigParseException:
    /// LineNumber — строка, где был ключ; InnerException — исходное FormatException или OverflowException.
    /// Подсказка: catch (Exception ex) when (ex is FormatException or OverflowException).
    /// </summary>
    public int GetInt(string key)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Try-pattern: без исключений. true + значение, если ключ есть и это int; иначе false и value = 0.
    /// </summary>
    public bool TryGetInt(string key, out int value)
    {
        throw new NotImplementedException();
    }
}
