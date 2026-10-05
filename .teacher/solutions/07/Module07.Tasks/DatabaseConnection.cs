namespace Module07.Tasks;

public sealed class ConnectionTracker
{
    private readonly List<string> _events = new();

    public int OpenConnections { get; private set; }

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

// sealed + только управляемые ресурсы → достаточно простого Dispose без финализатора
// и без Dispose(bool) (полный Dispose pattern — в модуле 12).
public sealed class DatabaseConnection : IDisposable
{
    private readonly ConnectionTracker _tracker;
    private bool _disposed;

    public DatabaseConnection(string name, ConnectionTracker tracker)
    {
        // Валидация ДО захвата ресурса: если бросим после OnOpened, соединение «утечёт» —
        // объект не создан, и Dispose вызвать будет не у кого.
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(tracker);

        Name = name;
        _tracker = tracker;
        _tracker.OnOpened(name);
    }

    public string Name { get; }

    public bool IsDisposed => _disposed;

    public void Execute(string command)
    {
        // Использование освобождённого ресурса — ошибка программиста; стандартный тип — ObjectDisposedException.
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        if (command.Contains("FAIL", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Command failed: {command}");
        }

        _tracker.OnExecuted(Name, command);
    }

    // Dispose обязан быть идемпотентным: его могут вызвать и using, и код вручную,
    // и повторный вызов не должен ни падать, ни освобождать ресурс дважды.
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _tracker.OnClosed(Name);
    }
}

public static class ReportService
{
    public static int RunCommands(ConnectionTracker tracker, string connectionName, IEnumerable<string> commands)
    {
        // Проверяем аргументы до открытия соединения — не тратим ресурс на заведомо невалидный вызов.
        ArgumentNullException.ThrowIfNull(commands);

        // using declaration: Dispose вызовется в конце метода — компилятор генерирует try/finally,
        // поэтому соединение закроется и при return, и при исключении.
        using var connection = new DatabaseConnection(connectionName, tracker);

        int count = 0;
        foreach (var command in commands)
        {
            connection.Execute(command);
            count++;
        }

        return count;
    }

    public static void Transfer(ConnectionTracker tracker, string command)
    {
        // Несколько using освобождаются в обратном порядке объявления (как вложенные try/finally):
        // target зависит от «окружения», созданного раньше, поэтому закрывается первым.
        using var source = new DatabaseConnection("source", tracker);
        using var target = new DatabaseConnection("target", tracker);

        source.Execute(command);
        target.Execute(command);
    }
}
