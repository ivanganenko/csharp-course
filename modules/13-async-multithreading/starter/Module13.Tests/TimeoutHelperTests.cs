using Module13.Tasks;

namespace Module13.Tests;

public class TimeoutHelperTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan Long = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task WithTimeoutAsync_OperationCompletes_ReturnsResult()
    {
        var result = await AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync(_ => Task.FromResult(42), Long));

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task WithTimeoutAsync_AsyncOperationCompletes_ReturnsResult()
    {
        var result = await AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync(async ct =>
            {
                await Task.Yield();
                return "report.pdf";
            }, Long));

        Assert.Equal("report.pdf", result);
    }

    [Fact]
    public async Task WithTimeoutAsync_OperationIgnoresTokenAndNeverEnds_ThrowsTimeoutException()
    {
        var neverCompletes = new TaskCompletionSource<int>();

        await Assert.ThrowsAsync<TimeoutException>(() => AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync(_ => neverCompletes.Task, Short)));
    }

    [Fact]
    public async Task WithTimeoutAsync_Timeout_CancelsTokenPassedToOperation()
    {
        var received = CancellationToken.None;

        await Assert.ThrowsAsync<TimeoutException>(() => AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync(async ct =>
            {
                received = ct;
                await Task.Delay(Timeout.Infinite, ct);
                return 0;
            }, Short)));

        Assert.True(received.IsCancellationRequested);
    }

    [Fact]
    public async Task WithTimeoutAsync_CallerCancels_ThrowsOperationCanceledNotTimeout()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ex = await Assert.ThrowsAnyAsync<Exception>(() => AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync(async ct =>
            {
                await Task.Delay(Timeout.Infinite, ct);
                return 0;
            }, Long, cts.Token)));

        Assert.IsAssignableFrom<OperationCanceledException>(ex);
    }

    [Fact]
    public async Task WithTimeoutAsync_OperationThrows_ExceptionPropagates()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => AsyncGuard.WithTimeout(
            TimeoutHelper.WithTimeoutAsync<int>(
                _ => Task.FromException<int>(new InvalidOperationException("Сервис недоступен")),
                Long)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task WithTimeoutAsync_NonPositiveTimeout_ThrowsArgumentOutOfRange(int milliseconds)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => TimeoutHelper.WithTimeoutAsync(_ => Task.FromResult(1), TimeSpan.FromMilliseconds(milliseconds)));
    }

    [Fact]
    public async Task WithTimeoutAsync_NullOperation_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => TimeoutHelper.WithTimeoutAsync<int>(null!, Long));
    }
}
