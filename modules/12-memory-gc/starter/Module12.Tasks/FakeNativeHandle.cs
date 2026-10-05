namespace Module12.Tasks;

/// <summary>
/// Готовый вспомогательный класс (его менять не нужно).
/// Имитирует unmanaged-ресурс — например, дескриптор (handle) файла или сокета операционной системы.
/// Garbage collector ничего не знает о таких ресурсах, поэтому их нужно освобождать явно.
/// Тесты смотрят на <see cref="ReleaseCount"/>, чтобы убедиться, что ресурс освобождён ровно один раз.
/// </summary>
public sealed class FakeNativeHandle
{
    private int _releaseCount;

    /// <summary>Сколько раз вызывали <see cref="Release"/>. Правильный код вызывает его ровно 1 раз.</summary>
    public int ReleaseCount => Volatile.Read(ref _releaseCount);

    /// <summary>true, если ресурс уже освобождён.</summary>
    public bool IsReleased => ReleaseCount > 0;

    /// <summary>
    /// «Закрыть дескриптор». Interlocked — потому что финализатор работает в отдельном потоке.
    /// </summary>
    public void Release() => Interlocked.Increment(ref _releaseCount);
}
