using System.Text;
using Module12.Tasks;

namespace Module12.Tests;

public class ReportWriterTests
{
    // Наследник нужен, чтобы детерминированно вызвать путь финализатора (Dispose(false))
    // без реального GC: ждать сборщик мусора в тестах — ненадёжно.
    private sealed class FinalizerProbe(Stream stream, FakeNativeHandle handle) : ReportWriter(stream, handle)
    {
        public void RunFinalizerPath() => Dispose(false);
    }

    [Fact]
    public void WriteLine_WritesUtf8TextWithNewLine()
    {
        var stream = new MemoryStream();
        using var writer = new ReportWriter(stream, new FakeNativeHandle());

        writer.WriteLine("Итого: 42");

        Assert.Equal("Итого: 42\n", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void Dispose_ReleasesHandleAndDisposesStream()
    {
        var stream = new MemoryStream();
        var handle = new FakeNativeHandle();
        var writer = new ReportWriter(stream, handle);

        writer.Dispose();

        Assert.True(writer.IsDisposed);
        Assert.Equal(1, handle.ReleaseCount);
        Assert.False(stream.CanWrite); // MemoryStream после Dispose закрыт
    }

    [Fact]
    public void Dispose_CalledTwice_ReleasesHandleOnlyOnce()
    {
        var handle = new FakeNativeHandle();
        var writer = new ReportWriter(new MemoryStream(), handle);

        writer.Dispose();
        writer.Dispose();

        Assert.Equal(1, handle.ReleaseCount);
    }

    [Fact]
    public void IsDisposed_BeforeDispose_IsFalse()
    {
        using var writer = new ReportWriter(new MemoryStream(), new FakeNativeHandle());

        Assert.False(writer.IsDisposed);
    }

    [Fact]
    public void WriteLine_AfterDispose_ThrowsObjectDisposedException()
    {
        var writer = new ReportWriter(new MemoryStream(), new FakeNativeHandle());
        writer.Dispose();

        Assert.Throws<ObjectDisposedException>(() => writer.WriteLine("late"));
    }

    [Fact]
    public void UsingBlock_ExceptionInside_StillReleasesHandle()
    {
        var handle = new FakeNativeHandle();

        Assert.Throws<InvalidOperationException>(() => WriteReportAndFail(handle));

        Assert.Equal(1, handle.ReleaseCount);
    }

    private static void WriteReportAndFail(FakeNativeHandle handle)
    {
        using var writer = new ReportWriter(new MemoryStream(), handle);
        writer.WriteLine("start");
        throw new InvalidOperationException("Ошибка при формировании отчёта");
    }

    [Fact]
    public void DisposeFalse_FinalizerPath_ReleasesOnlyUnmanagedResource()
    {
        var stream = new MemoryStream();
        var handle = new FakeNativeHandle();
        var probe = new FinalizerProbe(stream, handle);

        probe.RunFinalizerPath();

        Assert.Equal(1, handle.ReleaseCount);
        // В финализаторе нельзя трогать другие managed-объекты — поток не закрыт.
        Assert.True(stream.CanWrite);
    }

    [Fact]
    public void DisposeFalseThenDispose_ReleasesHandleOnlyOnce()
    {
        var handle = new FakeNativeHandle();
        var probe = new FinalizerProbe(new MemoryStream(), handle);

        probe.RunFinalizerPath();
        probe.Dispose();

        Assert.Equal(1, handle.ReleaseCount);
    }
}
