using Module10.Tasks;

namespace Module10.Tests;

public class PipelineTests
{
    private static Func<RequestHandler, RequestHandler> Logging(string name) =>
        next => context =>
        {
            context.Log.Add($"{name}:before");
            next(context);
            context.Log.Add($"{name}:after");
        };

    [Fact]
    public void Build_WithoutRun_Returns404()
    {
        RequestHandler app = new PipelineBuilder().Build();
        var context = new RequestContext("/unknown");

        app(context);

        Assert.Equal(404, context.StatusCode);
        Assert.Equal("Not Found", context.Response);
    }

    [Fact]
    public void Run_TerminalHandlesRequest()
    {
        RequestHandler app = new PipelineBuilder()
            .Run(context => context.Response = $"Hello from {context.Path}")
            .Build();
        var context = new RequestContext("/hello");

        app(context);

        Assert.Equal(200, context.StatusCode);
        Assert.Equal("Hello from /hello", context.Response);
    }

    [Fact]
    public void Middlewares_RunInRegistrationOrder_AndUnwindInReverse()
    {
        RequestHandler app = new PipelineBuilder()
            .Use(Logging("A"))
            .Use(Logging("B"))
            .Run(context => context.Log.Add("handler"))
            .Build();
        var context = new RequestContext("/");

        app(context);

        Assert.Equal(new[] { "A:before", "B:before", "handler", "B:after", "A:after" }, context.Log);
    }

    [Fact]
    public void Middleware_CanShortCircuit()
    {
        bool handlerCalled = false;
        RequestHandler app = new PipelineBuilder()
            .Use(Logging("log"))
            .Use(next => context =>
            {
                if (context.Path.StartsWith("/admin", StringComparison.Ordinal))
                {
                    context.StatusCode = 401;
                    return; // next НЕ вызываем
                }
                next(context);
            })
            .Run(_ => handlerCalled = true)
            .Build();
        var context = new RequestContext("/admin/users");

        app(context);

        Assert.Equal(401, context.StatusCode);
        Assert.False(handlerCalled);
        Assert.Equal(new[] { "log:before", "log:after" }, context.Log);
    }

    [Fact]
    public void Use_And_Run_ReturnSameBuilder()
    {
        var builder = new PipelineBuilder();

        Assert.Same(builder, builder.Use(next => next));
        Assert.Same(builder, builder.Run(_ => { }));
    }

    [Fact]
    public void Run_CalledTwice_LastTerminalWins()
    {
        RequestHandler app = new PipelineBuilder()
            .Run(c => c.Response = "first")
            .Run(c => c.Response = "second")
            .Build();
        var context = new RequestContext("/");

        app(context);

        Assert.Equal("second", context.Response);
    }

    [Fact]
    public void Build_MiddlewareAddedAfterBuild_DoesNotAffectBuiltPipeline()
    {
        var builder = new PipelineBuilder().Run(c => c.Log.Add("handler"));
        RequestHandler app = builder.Build();

        builder.Use(Logging("late"));
        var context = new RequestContext("/");
        app(context);

        Assert.Equal(new[] { "handler" }, context.Log);
    }

    [Fact]
    public void BuiltPipeline_IsReusableForManyRequests()
    {
        RequestHandler app = new PipelineBuilder()
            .Use(Logging("A"))
            .Run(c => c.Response = c.Path.ToUpperInvariant())
            .Build();

        var first = new RequestContext("/a");
        var second = new RequestContext("/b");
        app(first);
        app(second);

        Assert.Equal("/A", first.Response);
        Assert.Equal("/B", second.Response);
        Assert.Equal(new[] { "A:before", "A:after" }, second.Log);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        var builder = new PipelineBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Use(null!));
        Assert.Throws<ArgumentNullException>(() => builder.Run(null!));
    }
}
