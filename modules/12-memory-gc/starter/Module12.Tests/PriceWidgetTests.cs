using Module12.Tasks;

namespace Module12.Tests;

// Утечку проверяем ДЕТЕРМИНИРОВАННО — по числу подписчиков у издателя,
// а не через GC.Collect + WeakReference (результат GC в тестах зависит от JIT/режима сборки).
public class PriceWidgetTests
{
    [Fact]
    public void Widget_ReceivesPriceForItsSymbol()
    {
        var ticker = new StockTicker();
        using var widget = new PriceWidget(ticker, "ACME");

        ticker.Publish("ACME", 101.5m);

        Assert.Equal(101.5m, widget.LastPrice);
        Assert.Equal(1, widget.UpdateCount);
        Assert.Equal("ACME", widget.Symbol);
    }

    [Fact]
    public void Widget_IgnoresOtherSymbols()
    {
        var ticker = new StockTicker();
        using var widget = new PriceWidget(ticker, "ACME");

        ticker.Publish("CONTOSO", 50m);

        Assert.Null(widget.LastPrice);
        Assert.Equal(0, widget.UpdateCount);
    }

    [Fact]
    public void CreatingWidgets_IncreasesSubscriberCount()
    {
        var ticker = new StockTicker();

        using var w1 = new PriceWidget(ticker, "ACME");
        using var w2 = new PriceWidget(ticker, "ACME");
        using var w3 = new PriceWidget(ticker, "CONTOSO");

        Assert.Equal(3, ticker.SubscriberCount);
    }

    [Fact]
    public void NoSubscribers_SubscriberCountIsZero_AndPublishDoesNotThrow()
    {
        var ticker = new StockTicker();

        ticker.Publish("ACME", 1m);

        Assert.Equal(0, ticker.SubscriberCount);
    }

    [Fact]
    public void Dispose_Unsubscribes_NoLeak()
    {
        var ticker = new StockTicker();
        var widgets = new List<PriceWidget>();
        for (var i = 0; i < 5; i++)
        {
            widgets.Add(new PriceWidget(ticker, "ACME"));
        }

        foreach (var widget in widgets)
        {
            widget.Dispose();
        }

        Assert.Equal(0, ticker.SubscriberCount);
    }

    [Fact]
    public void DisposedWidget_DoesNotReceiveUpdates()
    {
        var ticker = new StockTicker();
        var widget = new PriceWidget(ticker, "ACME");
        ticker.Publish("ACME", 10m);

        widget.Dispose();
        ticker.Publish("ACME", 20m);

        Assert.Equal(10m, widget.LastPrice);
        Assert.Equal(1, widget.UpdateCount);
    }

    [Fact]
    public void DisposeTwice_DoesNotThrow_AndDoesNotAffectOtherWidgets()
    {
        var ticker = new StockTicker();
        var first = new PriceWidget(ticker, "ACME");
        using var second = new PriceWidget(ticker, "ACME");

        first.Dispose();
        first.Dispose();
        ticker.Publish("ACME", 7m);

        Assert.Equal(1, ticker.SubscriberCount);
        Assert.Equal(7m, second.LastPrice);
    }

    [Fact]
    public void WidgetWithoutDispose_StaysSubscribed_ThisIsTheLeak()
    {
        var ticker = new StockTicker();

        CreateAndForgetWidget(ticker);

        // Мы «забыли» виджет, но ticker всё ещё держит ссылку на него через delegate.
        Assert.Equal(1, ticker.SubscriberCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Publish_EmptySymbol_ThrowsArgumentException(string symbol)
    {
        var ticker = new StockTicker();

        Assert.Throws<ArgumentException>(() => ticker.Publish(symbol, 1m));
    }

    [Fact]
    public void Constructor_NullTicker_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PriceWidget(null!, "ACME"));
    }

    private static void CreateAndForgetWidget(StockTicker ticker)
    {
        _ = new PriceWidget(ticker, "ACME");
    }
}
