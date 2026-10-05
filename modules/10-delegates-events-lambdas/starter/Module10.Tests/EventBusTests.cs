using Module10.Tasks;

namespace Module10.Tests;

public class EventBusTests
{
    private sealed record OrderPlaced(int OrderId, decimal Total);

    private sealed record UserRegistered(string Login);

    [Fact]
    public void Publish_NoSubscribers_ReturnsZero()
    {
        var bus = new EventBus();

        Assert.Equal(0, bus.Publish(new OrderPlaced(1, 10m)));
    }

    [Fact]
    public void Publish_CallsSubscribersOfThatTypeInOrder()
    {
        var bus = new EventBus();
        var log = new List<string>();
        bus.Subscribe<OrderPlaced>(e => log.Add($"email:{e.OrderId}"));
        bus.Subscribe<OrderPlaced>(e => log.Add($"stock:{e.OrderId}"));

        int handled = bus.Publish(new OrderPlaced(7, 99m));

        Assert.Equal(2, handled);
        Assert.Equal(new[] { "email:7", "stock:7" }, log);
    }

    [Fact]
    public void Publish_DoesNotCallSubscribersOfOtherTypes()
    {
        var bus = new EventBus();
        int userEvents = 0;
        bus.Subscribe<UserRegistered>(_ => userEvents++);

        bus.Publish(new OrderPlaced(1, 1m));

        Assert.Equal(0, userEvents);
    }

    [Fact]
    public void Subscribe_ReturnsDisposable_ThatUnsubscribes()
    {
        var bus = new EventBus();
        int calls = 0;
        IDisposable subscription = bus.Subscribe<UserRegistered>(_ => calls++);
        Assert.Equal(1, bus.SubscriberCount<UserRegistered>());

        subscription.Dispose();
        subscription.Dispose(); // повторный — безопасен
        bus.Publish(new UserRegistered("user1"));

        Assert.Equal(0, calls);
        Assert.Equal(0, bus.SubscriberCount<UserRegistered>());
    }

    [Fact]
    public void Dispose_RemovesOnlyItsOwnSubscription()
    {
        var bus = new EventBus();
        var log = new List<string>();
        var first = bus.Subscribe<UserRegistered>(_ => log.Add("first"));
        bus.Subscribe<UserRegistered>(_ => log.Add("second"));

        first.Dispose();
        bus.Publish(new UserRegistered("user1"));

        Assert.Equal(new[] { "second" }, log);
    }

    [Fact]
    public void Publish_HandlerUnsubscribesItselfDuringPublish_DoesNotBreakDelivery()
    {
        var bus = new EventBus();
        var log = new List<string>();
        IDisposable? self = null;
        self = bus.Subscribe<UserRegistered>(_ => { log.Add("once"); self!.Dispose(); });
        bus.Subscribe<UserRegistered>(_ => log.Add("always"));

        int firstRound = bus.Publish(new UserRegistered("a"));
        int secondRound = bus.Publish(new UserRegistered("b"));

        Assert.Equal(2, firstRound);
        Assert.Equal(1, secondRound);
        Assert.Equal(new[] { "once", "always", "always" }, log);
    }

    [Fact]
    public void Publish_HandlerSubscribesNewHandler_NewOneNotCalledInCurrentPublish()
    {
        var bus = new EventBus();
        int lateCalls = 0;
        bus.Subscribe<UserRegistered>(outer => bus.Subscribe<UserRegistered>(inner => lateCalls++));

        int handled = bus.Publish(new UserRegistered("a"));

        Assert.Equal(1, handled);
        Assert.Equal(0, lateCalls);
        Assert.Equal(2, bus.SubscriberCount<UserRegistered>());
    }

    [Fact]
    public void NullArguments_Throw()
    {
        var bus = new EventBus();

        Assert.Throws<ArgumentNullException>(() => bus.Subscribe<UserRegistered>(null!));
        Assert.Throws<ArgumentNullException>(() => bus.Publish<UserRegistered>(null!));
    }
}
