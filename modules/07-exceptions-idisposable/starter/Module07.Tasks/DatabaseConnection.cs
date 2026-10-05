namespace Module07.Tasks;

/// <summary>
/// Журнал «открытых соединений» — помогает тестам проверить, что ресурсы освобождаются.
/// Уже готов — менять не нужно.
/// </summary>
public sealed class ConnectionTracker
{
    private readonly List<string> _events = new();

    /// <summary>Сколько соединений сейчас открыто.</summary>
    public int OpenConnections { get; private set; }

    /// <summary>События по порядку: "open:name", "exec:name:command", "close:name".</summary>
    public IReadOnlyList<string> Events => _events.AsReadOnly();

    public void OnOpened(string name)
    {
        OpenConnections++;
        _events.Add($"open:{name}");
    }

    public void OnExecuted(string name, string command) => _events.Add($"exec:{name}:{command}");

    public void OnClosed(string name)
    {
        OpenConnections--;
        _events.Add($"close:{name}");
    }
}

/// <summary>
/// Задача 07.4 — «соединение с БД» (фейковое), которое обязательно нужно закрыть.
/// </summary>
public sealed class DatabaseConnection : IDisposable
{
    /// <summary>
    /// Открывает соединение: tracker.OnOpened(name).
    /// name null/пусто/пробелы → ArgumentException (или наследник); tracker == null → ArgumentNullException.
    /// </summary>
    public DatabaseConnection(string name, ConnectionTracker tracker)
    {
        throw new NotImplementedException();
    }

    public string Name => throw new NotImplementedException();

    public bool IsDisposed => throw new NotImplementedException();

    /// <summary>
    /// Выполняет команду: tracker.OnExecuted(Name, command).
    /// После Dispose → ObjectDisposedException.
    /// command null/пусто/пробелы → ArgumentException (или наследник).
    /// Для имитации сбоя: если command содержит "FAIL" (с учётом регистра) → InvalidOperationException,
    /// и OnExecuted не вызывается.
    /// </summary>
    public void Execute(string command)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Закрывает соединение: tracker.OnClosed(Name). Идемпотентен — повторный вызов ничего не делает
    /// (OnClosed вызывается ровно один раз).
    /// </summary>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

/// <summary>Код, который пользуется соединениями. Соединения обязаны закрываться даже при исключениях.</summary>
public static class ReportService
{
    /// <summary>
    /// Открывает соединение connectionName, выполняет все команды по порядку и возвращает их количество.
    /// Если команда падает — исключение пробрасывается наружу, но соединение всё равно закрыто.
    /// commands == null → ArgumentNullException (до открытия соединения).
    /// </summary>
    public static int RunCommands(ConnectionTracker tracker, string connectionName, IEnumerable<string> commands)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Открывает соединение "source", затем "target"; выполняет command сначала на source, потом на target.
    /// Оба соединения закрываются в ОБРАТНОМ порядке открытия (target, затем source) — и при успехе, и при ошибке.
    /// Ожидаемые события при успехе:
    /// open:source, open:target, exec:source:cmd, exec:target:cmd, close:target, close:source.
    /// </summary>
    public static void Transfer(ConnectionTracker tracker, string command)
    {
        throw new NotImplementedException();
    }
}
