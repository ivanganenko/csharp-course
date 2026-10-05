using System.Text;

namespace Module12.Tasks;

/// <summary>
/// Задача 12.1 — полный Dispose pattern.
/// Класс владеет двумя ресурсами:
///  - managed: <see cref="Stream"/> (сам реализует IDisposable);
///  - «unmanaged»: <see cref="FakeNativeHandle"/> (имитация дескриптора ОС).
///
/// Реализуй классический Dispose pattern для НЕ sealed класса:
///  - public void Dispose() → Dispose(true) + GC.SuppressFinalize(this);
///  - protected virtual void Dispose(bool disposing);
///  - финализатор ~ReportWriter() → Dispose(false) — ДОБАВЬ ЕГО САМ
///    (в starter его нет специально: финализатор, бросающий NotImplementedException,
///    уронил бы весь процесс тестов).
/// </summary>
public class ReportWriter : IDisposable
{
    private readonly Stream _stream;
    private readonly FakeNativeHandle _handle;

    /// <summary>Конструктор уже готов.</summary>
    public ReportWriter(Stream stream, FakeNativeHandle handle)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(handle);
        _stream = stream;
        _handle = handle;
    }

    /// <summary>true после первого вызова Dispose() или Dispose(false).</summary>
    public bool IsDisposed => throw new NotImplementedException();

    /// <summary>
    /// Записывает в поток строку <paramref name="text"/> и символ '\n' в кодировке UTF-8
    /// (используй <see cref="Encoding.UTF8"/>).
    /// После Dispose бросает <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void WriteLine(string text)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Освобождает все ресурсы. Повторный вызов безопасен и ничего не делает
    /// (handle освобождается ровно один раз).
    /// </summary>
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// disposing == true — вызов из Dispose(): освободить managed (Stream) и unmanaged (handle) ресурсы.
    /// disposing == false — вызов из финализатора: освободить ТОЛЬКО unmanaged (handle),
    /// к Stream не прикасаться.
    /// Повторный вызов (в любой комбинации) ничего не делает.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        throw new NotImplementedException();
    }
}
