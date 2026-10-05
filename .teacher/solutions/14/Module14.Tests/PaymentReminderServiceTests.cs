using Module14.Tasks;

namespace Module14.Tests;

public class PaymentReminderServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeClock _clock = new(Now);
    private readonly FakeEmailSender _emailSender = new();
    private readonly PaymentReminderService _sut;

    public PaymentReminderServiceTests()
    {
        _sut = new PaymentReminderService(_clock, _emailSender);
    }

    private static Order CreateOrder(string email, OrderStatus status, DateTimeOffset createdAt) => new()
    {
        CustomerEmail = email,
        Total = 100m,
        Status = status,
        CreatedAt = createdAt,
    };

    [Fact]
    public async Task SendRemindersAsync_FailedOrderOlderThanOneDay_SendsReminder()
    {
        var order = CreateOrder("customer1@example.com", OrderStatus.PaymentFailed, Now.AddDays(-2));

        var sent = await _sut.SendRemindersAsync([order], CancellationToken.None);

        Assert.Equal(1, sent);
        var email = Assert.Single(_emailSender.Sent);
        Assert.Equal("customer1@example.com", email.To);
        Assert.Equal("Payment reminder", email.Subject);
        Assert.Contains(order.Id.ToString(), email.Body);
    }

    [Fact]
    public async Task SendRemindersAsync_ExactlyOneDayOld_SendsReminder()
    {
        var order = CreateOrder("customer1@example.com", OrderStatus.PaymentFailed, Now.AddDays(-1));

        var sent = await _sut.SendRemindersAsync([order], CancellationToken.None);

        Assert.Equal(1, sent);
    }

    [Fact]
    public async Task SendRemindersAsync_LessThanOneDayOld_DoesNotSend()
    {
        var order = CreateOrder("customer1@example.com", OrderStatus.PaymentFailed, Now.AddHours(-23));

        var sent = await _sut.SendRemindersAsync([order], CancellationToken.None);

        Assert.Equal(0, sent);
        Assert.Empty(_emailSender.Sent);
    }

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task SendRemindersAsync_OtherStatuses_AreSkipped(OrderStatus status)
    {
        var order = CreateOrder("customer1@example.com", status, Now.AddDays(-5));

        var sent = await _sut.SendRemindersAsync([order], CancellationToken.None);

        Assert.Equal(0, sent);
        Assert.Empty(_emailSender.Sent);
    }

    [Fact]
    public async Task SendRemindersAsync_MixedOrders_SendsOnlyToDueCustomers()
    {
        var orders = new[]
        {
            CreateOrder("due1@example.com", OrderStatus.PaymentFailed, Now.AddDays(-3)),
            CreateOrder("fresh@example.com", OrderStatus.PaymentFailed, Now.AddHours(-1)),
            CreateOrder("paid@example.com", OrderStatus.Paid, Now.AddDays(-3)),
            CreateOrder("due2@example.com", OrderStatus.PaymentFailed, Now.AddDays(-1)),
        };

        var sent = await _sut.SendRemindersAsync(orders, CancellationToken.None);

        Assert.Equal(2, sent);
        Assert.Equal(new[] { "due1@example.com", "due2@example.com" }, _emailSender.Sent.Select(e => e.To));
    }

    [Fact]
    public async Task SendRemindersAsync_AfterTimePasses_OrderBecomesDue()
    {
        var order = CreateOrder("customer1@example.com", OrderStatus.PaymentFailed, Now.AddHours(-22));
        Assert.Equal(0, await _sut.SendRemindersAsync([order], CancellationToken.None));

        _clock.Advance(TimeSpan.FromHours(2)); // «перематываем» время — с DateTime.UtcNow так не сделать

        Assert.Equal(1, await _sut.SendRemindersAsync([order], CancellationToken.None));
    }

    [Fact]
    public async Task SendRemindersAsync_EmptyList_ReturnsZero()
    {
        var sent = await _sut.SendRemindersAsync([], CancellationToken.None);

        Assert.Equal(0, sent);
        Assert.Empty(_emailSender.Sent);
    }

    [Fact]
    public async Task SendRemindersAsync_NullOrders_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.SendRemindersAsync(null!, CancellationToken.None));
    }
}
