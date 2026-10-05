using System.Globalization;

namespace Module07.Tasks;

public sealed class ConfigParseException : Exception
{
    public ConfigParseException(string message, int lineNumber, Exception? innerException = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
    }

    public int LineNumber { get; }
}

public sealed class AppConfig
{
    private readonly Dictionary<string, Entry> _values;

    // Private-конструктор: создать конфиг можно только через Parse — значит, он всегда валиден.
    private AppConfig(Dictionary<string, Entry> values)
    {
        _values = values;
    }

    public int Count => _values.Count;

    public static AppConfig Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        // OrdinalIgnoreCase в самом словаре — не нужно приводить ключи к нижнему регистру вручную.
        var values = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        var lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            var line = lines[i].Trim(); // Trim убирает и '\r' от Windows-переводов строк

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            int separator = line.IndexOf('=');
            if (separator < 0)
            {
                throw new ConfigParseException($"Line {lineNumber}: expected 'key=value'.", lineNumber);
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();

            if (key.Length == 0)
            {
                throw new ConfigParseException($"Line {lineNumber}: key is empty.", lineNumber);
            }

            // TryAdd вместо ContainsKey + Add: один поиск и явная обработка дубликата.
            if (!values.TryAdd(key, new Entry(value, lineNumber)))
            {
                throw new ConfigParseException($"Line {lineNumber}: duplicate key '{key}'.", lineNumber);
            }
        }

        return new AppConfig(values);
    }

    public string GetString(string key) => GetEntry(key).Value;

    public int GetInt(string key)
    {
        var entry = GetEntry(key);
        try
        {
            return int.Parse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            // Фильтр when ловит ТОЛЬКО ожидаемые ошибки разбора. Оборачиваем низкоуровневое исключение
            // в доменное (с номером строки — его знает только конфиг), а исходное сохраняем в InnerException,
            // чтобы не потерять детали для логов.
            throw new ConfigParseException(
                $"Line {entry.Line}: value of '{key}' is not a valid integer.", entry.Line, ex);
        }
    }

    // Try-pattern: для случаев, когда «не число» — нормальная ситуация, а не ошибка.
    // Без исключений — без их стоимости (stack trace) и без try/catch у вызывающего.
    public bool TryGetInt(string key, out int value)
    {
        value = 0;
        return _values.TryGetValue(key, out var entry)
            && int.TryParse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private Entry GetEntry(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_values.TryGetValue(key, out var entry))
        {
            throw new KeyNotFoundException($"Configuration key '{key}' was not found.");
        }

        return entry;
    }

    // Значение + номер строки, чтобы и при чтении (GetInt) сообщать, где ошибка в файле.
    private readonly record struct Entry(string Value, int Line);
}
