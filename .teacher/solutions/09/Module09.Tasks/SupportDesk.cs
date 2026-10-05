namespace Module09.Tasks;

public class SupportDesk
{
    // PriorityQueue (binary heap): Enqueue/Dequeue за O(log n), минимальный приоритет — первым.
    // Важно: PriorityQueue НЕ стабилен — при равных приоритетах порядок не гарантирован.
    // Поэтому приоритет — кортеж (priority, sequence): кортежи сравниваются лексикографически,
    // и при равном priority побеждает меньший порядковый номер, то есть более раннее обращение (FIFO).
    private readonly PriorityQueue<string, (int Priority, long Sequence)> _queue = new();

    // HashSet — для Contains за O(1) и проверки дубликатов. В PriorityQueue поиск был бы O(n).
    private readonly HashSet<string> _pending = new(StringComparer.Ordinal);

    // List — история только дописывается в конец (O(1) амортизированно) и читается по порядку.
    private readonly List<string> _history = new();

    private long _nextSequence;

    public int Count => _pending.Count;

    // Наружу — read-only представление: снаружи нельзя «подправить» историю.
    public IReadOnlyList<string> History => _history;

    public void Submit(string ticketId, int priority)
    {
        // Для null бросит ArgumentNullException (наследник ArgumentException), для "" и "  " — ArgumentException.
        ArgumentException.ThrowIfNullOrWhiteSpace(ticketId);

        // Сначала HashSet: если id дублируется, очередь остаётся нетронутой (состояние консистентно).
        if (!_pending.Add(ticketId))
        {
            throw new InvalidOperationException($"Ticket '{ticketId}' is already pending.");
        }

        _queue.Enqueue(ticketId, (priority, _nextSequence++));
    }

    public string? TakeNext()
    {
        if (!_queue.TryDequeue(out string? ticketId, out _))
        {
            return null;
        }

        _pending.Remove(ticketId);
        _history.Add(ticketId);
        return ticketId;
    }

    public bool Contains(string ticketId) => _pending.Contains(ticketId);
}
