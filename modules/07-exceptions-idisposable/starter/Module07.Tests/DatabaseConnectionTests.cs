using Module07.Tasks;

namespace Module07.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public void Constructor_OpensConnection()
    {
        var tracker = new ConnectionTracker();

        var connection = new DatabaseConnection("db", tracker);

        Assert.Equal("db", connection.Name);
        Assert.False(connection.IsDisposed);
        Assert.Equal(1, tracker.OpenConnections);
        Assert.Equal("open:db", string.Join("|", tracker.Events));
    }

    [Fact]
    public void Constructor_InvalidArguments_Throw()
    {
        var tracker = new ConnectionTracker();

        Assert.ThrowsAny<ArgumentException>(() => new DatabaseConnection(" ", tracker));
        Assert.Throws<ArgumentNullException>(() => new DatabaseConnection("db", null!));
        Assert.Equal(0, tracker.OpenConnections);
    }

    [Fact]
    public void Execute_RecordsCommand()
    {
        var tracker = new ConnectionTracker();
        using var connection = new DatabaseConnection("db", tracker);

        connection.Execute("SELECT 1");

        Assert.Equal("open:db|exec:db:SELECT 1", string.Join("|", tracker.Events));
    }

    [Fact]
    public void Execute_FailingCommand_ThrowsAndDoesNotRecord()
    {
        var tracker = new ConnectionTracker();
        using var connection = new DatabaseConnection("db", tracker);

        Assert.Throws<InvalidOperationException>(() => connection.Execute("FAIL please"));
        Assert.Equal("open:db", string.Join("|", tracker.Events));
    }

    [Fact]
    public void Dispose_ClosesOnce_EvenIfCalledTwice()
    {
        var tracker = new ConnectionTracker();
        var connection = new DatabaseConnection("db", tracker);

        connection.Dispose();
        connection.Dispose();

        Assert.True(connection.IsDisposed);
        Assert.Equal(0, tracker.OpenConnections);
        Assert.Equal("open:db|close:db", string.Join("|", tracker.Events));
    }

    [Fact]
    public void Execute_AfterDispose_ThrowsObjectDisposed()
    {
        var connection = new DatabaseConnection("db", new ConnectionTracker());
        connection.Dispose();

        Assert.Throws<ObjectDisposedException>(() => connection.Execute("SELECT 1"));
    }

    [Fact]
    public void UsingStatement_ClosesConnectionWhenExceptionIsThrownInside()
    {
        var tracker = new ConnectionTracker();

        Assert.Throws<InvalidOperationException>(() =>
        {
            using (var connection = new DatabaseConnection("db", tracker))
            {
                connection.Execute("FAIL");
            }
        });

        Assert.Equal(0, tracker.OpenConnections);
    }

    [Fact]
    public void RunCommands_Success_ReturnsCountAndCloses()
    {
        var tracker = new ConnectionTracker();

        var count = ReportService.RunCommands(tracker, "report", new[] { "A", "B" });

        Assert.Equal(2, count);
        Assert.Equal("open:report|exec:report:A|exec:report:B|close:report", string.Join("|", tracker.Events));
    }

    [Fact]
    public void RunCommands_Failure_PropagatesExceptionButClosesConnection()
    {
        var tracker = new ConnectionTracker();

        Assert.Throws<InvalidOperationException>(
            () => ReportService.RunCommands(tracker, "report", new[] { "A", "FAIL", "C" }));

        Assert.Equal(0, tracker.OpenConnections);
        Assert.Equal("open:report|exec:report:A|close:report", string.Join("|", tracker.Events));
    }

    [Fact]
    public void RunCommands_NullCommands_ThrowsBeforeOpening()
    {
        var tracker = new ConnectionTracker();

        Assert.Throws<ArgumentNullException>(() => ReportService.RunCommands(tracker, "report", null!));
        Assert.Empty(tracker.Events);
    }

    [Fact]
    public void Transfer_Success_DisposesInReverseOrder()
    {
        var tracker = new ConnectionTracker();

        ReportService.Transfer(tracker, "cmd");

        Assert.Equal(
            "open:source|open:target|exec:source:cmd|exec:target:cmd|close:target|close:source",
            string.Join("|", tracker.Events));
    }

    [Fact]
    public void Transfer_Failure_StillDisposesBothInReverseOrder()
    {
        var tracker = new ConnectionTracker();

        Assert.Throws<InvalidOperationException>(() => ReportService.Transfer(tracker, "FAIL"));

        Assert.Equal(0, tracker.OpenConnections);
        Assert.Equal("open:source|open:target|close:target|close:source", string.Join("|", tracker.Events));
    }
}
