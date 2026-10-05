using Module13.Tasks;

namespace Module13.Tests;

public class OrderImporterTests
{
    [Fact]
    public async Task ImportAsync_ImportsAllLinesInOrder_ReturnsCount()
    {
        var imported = new List<string>();

        var count = await AsyncGuard.WithTimeout(OrderImporter.ImportAsync(
            ["A-1", "A-2", "A-3"],
            async (line, ct) =>
            {
                await Task.Yield();
                imported.Add(line);
            },
            CancellationToken.None));

        Assert.Equal(3, count);
        Assert.Equal(new[] { "A-1", "A-2", "A-3" }, imported);
    }

    [Fact]
    public async Task ImportAsync_EmptyInput_ReturnsZero()
    {
        var count = await AsyncGuard.WithTimeout(
            OrderImporter.ImportAsync([], (_, _) => Task.CompletedTask, CancellationToken.None));

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task ImportAsync_AlreadyCancelled_ThrowsAndImportsNothing()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var imported = new List<string>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AsyncGuard.WithTimeout(
            OrderImporter.ImportAsync(
                ["A-1", "A-2"],
                (line, _) => { imported.Add(line); return Task.CompletedTask; },
                cts.Token)));

        Assert.Empty(imported);
    }

    [Fact]
    public async Task ImportAsync_CancelledInTheMiddle_StopsBeforeNextLine()
    {
        using var cts = new CancellationTokenSource();
        var imported = new List<string>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AsyncGuard.WithTimeout(
            OrderImporter.ImportAsync(
                ["A-1", "A-2", "A-3", "A-4"],
                (line, _) =>
                {
                    imported.Add(line);
                    if (line == "A-2")
                    {
                        cts.Cancel(); // пользователь нажал «Отмена» во время второго заказа
                    }

                    return Task.CompletedTask;
                },
                cts.Token)));

        Assert.Equal(new[] { "A-1", "A-2" }, imported);
    }

    [Fact]
    public async Task ImportAsync_PassesTokenToImportOne()
    {
        using var cts = new CancellationTokenSource();
        var received = new List<CancellationToken>();

        await AsyncGuard.WithTimeout(OrderImporter.ImportAsync(
            ["A-1"],
            (_, ct) => { received.Add(ct); return Task.CompletedTask; },
            cts.Token));

        Assert.Equal(cts.Token, Assert.Single(received));
    }

    [Fact]
    public async Task ImportAsync_ImportOneThrows_ExceptionPropagates()
    {
        await Assert.ThrowsAsync<FormatException>(() => AsyncGuard.WithTimeout(
            OrderImporter.ImportAsync(
                ["bad line"],
                (_, _) => Task.FromException(new FormatException("Некорректная строка заказа")),
                CancellationToken.None)));
    }

    [Fact]
    public async Task ImportAsync_NullArguments_ThrowArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OrderImporter.ImportAsync(null!, (_, _) => Task.CompletedTask, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => OrderImporter.ImportAsync(["A-1"], null!, CancellationToken.None));
    }
}
