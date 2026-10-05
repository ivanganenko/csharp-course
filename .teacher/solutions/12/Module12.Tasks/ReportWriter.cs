using System.Text;

namespace Module12.Tasks;

public class ReportWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly FakeNativeHandle _handle;

    // Флаг нужен для идемпотентности: Dispose могут вызвать много раз
    // (using + ручной вызов, финализатор после Dispose(false) и т.д.).
    private bool _disposed;

    public ReportWriter(Stream stream, FakeNativeHandle handle)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(handle);
        _stream = stream;
        _handle = handle;
    }

    // Финализатор — «страховка», если пользователь забыл вызвать Dispose.
    // Почему только Dispose(false): в момент финализации другие managed-объекты (наш Stream)
    // могли быть уже финализированы — трогать их нельзя.
    // В реальном коде вместо своего финализатора лучше обернуть дескриптор в SafeHandle:
    // SafeHandle уже имеет надёжный (critical) финализатор, а нашему классу финализатор не нужен.
    ~ReportWriter()
    {
        Dispose(false);
    }

    public bool IsDisposed => _disposed;

    public void WriteLine(string text)
    {
        // Работа с освобождённым объектом — ошибка вызывающего кода; сообщаем о ней явно,
        // а не падаем где-то в глубине Stream с непонятным исключением.
        ObjectDisposedException.ThrowIf(_disposed, this);

        var bytes = Encoding.UTF8.GetBytes(text + "\n");
        _stream.Write(bytes, 0, bytes.Length);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        // Ресурсы уже освобождены — финализатор больше не нужен.
        // Без SuppressFinalize объект пережил бы лишнюю сборку мусора (попал бы в очередь финализации).
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Managed-ресурсы освобождаем только при явном Dispose().
            _stream.Dispose();
        }

        // Unmanaged-ресурсы освобождаем в обоих случаях: GC о них не знает.
        _handle.Release();

        _disposed = true;
    }
}
