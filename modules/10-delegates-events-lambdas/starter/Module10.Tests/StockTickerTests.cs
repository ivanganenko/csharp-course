using Module10.Tasks;

namespace Module10.Tests;

public class StockTickerTests
{
    [Fact]
    public void UpdatePrice_FirstTime_RaisesEventWithNullOldPrice()
    {
        var ticker = new StockTicker();
        PriceChangedEventArgs? received = null;
        object? receivedSender = null;
        ticker.PriceChanged += (sender, e) => { receivedSender = sender; received = e; };

        ticker.UpdatePrice("ACME", 100.5m);

        Assert.NotNull(received);
        Assert.Same(ticker, receivedSender);
        Assert.Equal("ACME", received.Symbol);
        Assert.Null(received.OldPrice);
        Assert.Equal(100.5m, received.NewPrice);
    }

    [Fact]
    public void UpdatePrice_Changed_RaisesEventWithOldAndNewPrice()
    {
        var ticker = new StockTicker();
        ticker.UpdatePrice("ACME", 100m);
        var events = new List<PriceChangedEventArgs>();
        ticker.PriceChanged += (_, e) => events.Add(e);

        ticker.UpdatePrice("ACME", 105m);

        var single = Assert.Single(events);
        Assert.Equal(100m, single.OldPrice);
        Assert.Equal(105m, single.NewPrice);
        Assert.Equal(105m, ticker.GetPrice("ACME"));
    }

    [Fact]
    public void UpdatePrice_SamePrice_DoesNotRaiseEvent()
    {
        var ticker = new StockTicker();
        ticker.UpdatePrice("ACME", 100m);
        int raised = 0;
        ticker.PriceChanged += (_, _) => raised++;

        ticker.UpdatePrice("ACME", 100m);

        Assert.Equal(0, raised);
    }

    [Fact]
    public void UpdatePrice_NoSubscribers_DoesNotThrow()
    {
        var ticker = new StockTicker();

        ticker.UpdatePrice("ACME", 10m);

        Assert.Equal(10m, ticker.GetPrice("ACME"));
    }

    [Fact]
    public void UpdatePrice_PriceIsStoredBeforeEventIsRaised()
    {
        var ticker = new StockTicker();
        decimal? priceSeenByHandler = null;
        ticker.PriceChanged += (_, e) => priceSeenByHandler = ticker.GetPrice(e.Symbol);

        ticker.UpdatePrice("ACME", 42m);

        Assert.Equal(42m, priceSeenByHandler);
    }

    [Fact]
    public void UpdatePrice_AllSubscribersCalledInSubscriptionOrder()
    {
        var ticker = new StockTicker();
        var calls = new List<string>();
        ticker.PriceChanged += (_, _) => calls.Add("first");
        ticker.PriceChanged += (_, _) => calls.Add("second");

        ticker.UpdatePrice("ACME", 1m);

        Assert.Equal(new[] { "first", "second" }, calls);
    }

    [Fact]
    public void UpdatePrice_InvalidArguments_Throw()
    {
        var ticker = new StockTicker();

        Assert.Throws<ArgumentOutOfRangeException>(() => ticker.UpdatePrice("ACME", -1m));
        Assert.ThrowsAny<ArgumentException>(() => ticker.UpdatePrice(" ", 1m));
        Assert.ThrowsAny<ArgumentException>(() => ticker.UpdatePrice(null!, 1m));
    }

    [Fact]
    public void GetPrice_Unknown_ReturnsNull()
    {
        Assert.Null(new StockTicker().GetPrice("NONE"));
    }

    [Fact]
    public void SubscriberCount_TracksSubscribeAndUnsubscribe()
    {
        var ticker = new StockTicker();
        EventHandler<PriceChangedEventArgs> handler = (_, _) => { };

        Assert.Equal(0, ticker.SubscriberCount);
        ticker.PriceChanged += handler;
        ticker.PriceChanged += (_, _) => { };
        Assert.Equal(2, ticker.SubscriberCount);

        ticker.PriceChanged -= handler;
        Assert.Equal(1, ticker.SubscriberCount);
    }

    [Fact]
    public void PriceAlert_RecordsOnlyOwnSymbolAtOrAboveThreshold()
    {
        var ticker = new StockTicker();
        using var alert = new PriceAlert(ticker, "ACME", 100m);

        ticker.UpdatePrice("ACME", 90m);
        ticker.UpdatePrice("ACME", 100m);
        ticker.UpdatePrice("OTHER", 500m);
        ticker.UpdatePrice("ACME", 120m);

        Assert.Equal(new[] { 100m, 120m }, alert.TriggeredPrices);
    }

    [Fact]
    public void PriceAlert_SubscribesOnCreate_UnsubscribesOnDispose()
    {
        var ticker = new StockTicker();
        var alert = new PriceAlert(ticker, "ACME", 0m);
        Assert.Equal(1, ticker.SubscriberCount);

        alert.Dispose();
        ticker.UpdatePrice("ACME", 10m);

        Assert.Equal(0, ticker.SubscriberCount);
        Assert.Empty(alert.TriggeredPrices);
    }

    [Fact]
    public void PriceAlert_DisposeTwice_IsSafe()
    {
        var ticker = new StockTicker();
        var other = new PriceAlert(ticker, "OTHER", 0m);
        var alert = new PriceAlert(ticker, "ACME", 0m);

        alert.Dispose();
        alert.Dispose();

        // Повторный Dispose не должен «зацепить» чужую подписку.
        Assert.Equal(1, ticker.SubscriberCount);
        other.Dispose();
    }

    [Fact]
    public void PriceAlert_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new PriceAlert(null!, "ACME", 1m));
        Assert.ThrowsAny<ArgumentException>(() => new PriceAlert(new StockTicker(), "", 1m));
    }
}
