namespace Module04.Tasks;

public sealed class SupportTicket
{
    // const: значение известно при компиляции и копируется в каждое место использования
    // (даже в другие сборки). Подходит для «вечных» значений вроде лимитов формата.
    public const int MaxTitleLength = 100;

    // static readonly: вычисляется в runtime один раз. Если поменять значение и пересобрать
    // только эту сборку, зависимые сборки увидят новое значение (с const — нет).
    public static readonly string Prefix = "TCK-";

    // static-поле принадлежит ТИПУ, а не экземпляру: одно на всё приложение.
    private static int _lastId;

    public SupportTicket(string title)
    {
        // Сначала валидация, потом изменение static-состояния. Если поменять порядок,
        // неудачная попытка создания «съест» номер, и в нумерации появятся дыры.
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        var trimmed = title.Trim();
        if (trimmed.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"Title must not be longer than {MaxTitleLength} characters.", nameof(title));
        }

        Title = trimmed;

        // Interlocked.Increment — атомарная операция: _lastId++ из двух потоков одновременно
        // может выдать двум заявкам один и тот же Id (read-modify-write гонка, подробно в модуле 13).
        Id = Interlocked.Increment(ref _lastId);
    }

    // Volatile.Read — чтобы гарантированно увидеть последнее записанное значение из любого потока.
    public static int TotalCreated => Volatile.Read(ref _lastId);

    public int Id { get; }

    public string Title { get; }

    // "D4" — формат целого с ведущими нулями. Для int он не зависит от культуры.
    public string Number => $"{Prefix}{Id:D4}";
}
