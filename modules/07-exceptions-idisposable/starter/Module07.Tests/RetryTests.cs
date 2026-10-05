using Module07.Tasks;

namespace Module07.Tests;

public class RetryTests
{
    [Fact]
    public void Execute_SucceedsFirstTime_CallsOnce()
    {
        int calls = 0;

        var result = Retry.Execute(() => { calls++; return 42; }, maxAttempts: 3);

        Assert.Equal(42, result);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void Execute_FailsTwiceThenSucceeds_ReturnsResult()
    {
        int calls = 0;

        var result = Retry.Execute(
            () =>
            {
                calls++;
                if (calls < 3)
                {
                    throw new TimeoutException($"attempt {calls}");
                }

                return "ok";
            },
            maxAttempts: 3);

        Assert.Equal("ok", result);
        Assert.Equal(3, calls);
    }

    [Fact]
    public void Execute_AlwaysFails_RethrowsTheSameLastException()
    {
        int calls = 0;
        InvalidOperationException? last = null;

        var thrown = Assert.Throws<InvalidOperationException>(() => Retry.Execute<int>(
            () =>
            {
                calls++;
                last = new InvalidOperationException($"attempt {calls}");
                throw last;
            },
            maxAttempts: 3));

        Assert.Equal(3, calls);
        Assert.Same(last, thrown);
        Assert.Equal("attempt 3", thrown.Message);
    }

    [Fact]
    public void Execute_NonTransientException_IsNotRetried()
    {
        int calls = 0;

        Assert.Throws<ArgumentException>(() => Retry.Execute<int>(
            () =>
            {
                calls++;
                throw new ArgumentException("bug, not a glitch");
            },
            maxAttempts: 5,
            isTransient: ex => ex is TimeoutException));

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Execute_TransientByPredicate_IsRetried()
    {
        int calls = 0;

        var result = Retry.Execute(
            () =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new TimeoutException();
                }

                return calls;
            },
            maxAttempts: 2,
            isTransient: ex => ex is TimeoutException);

        Assert.Equal(2, result);
    }

    [Fact]
    public void Execute_OnRetry_CalledWithFailedAttemptNumbers()
    {
        var log = new List<string>();
        int calls = 0;

        Assert.Throws<TimeoutException>(() => Retry.Execute<int>(
            () =>
            {
                calls++;
                throw new TimeoutException($"t{calls}");
            },
            maxAttempts: 3,
            onRetry: (attempt, ex) => log.Add($"{attempt}:{ex.Message}")));

        // После третьей (последней) неудачи onRetry уже не вызывается — повторять нечего.
        Assert.Equal("1:t1|2:t2", string.Join("|", log));
    }

    [Fact]
    public void Execute_MaxAttemptsOne_NoRetry()
    {
        int calls = 0;

        Assert.Throws<TimeoutException>(() => Retry.Execute<int>(
            () =>
            {
                calls++;
                throw new TimeoutException();
            },
            maxAttempts: 1));

        Assert.Equal(1, calls);
    }

    [Fact]
    public void Execute_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Retry.Execute<int>(null!, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => Retry.Execute(() => 1, 0));
    }
}
