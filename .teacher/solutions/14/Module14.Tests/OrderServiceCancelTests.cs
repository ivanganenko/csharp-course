using Module14.Tasks;
using NSubstitute;

namespace Module14.Tests;

// Тесты написаны ДО реализации CancelOrderAsync (TDD): каждое правило из спецификации — отдельный тест.
public class OrderServiceCancelTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 20, 9, 0, 0, TimeSpan.Zero);

    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly OrderService _sut;

    public OrderServiceCancelTests()
    {
        _clock.UtcNow.Returns(Now);
        _sut = new OrderService(_gateway, _emailSender, _clock);
    }

    private static Order PaidOrder(DateTimeOffset paidAt) => new()
    {
        CustomerEmail = "buyer@example.com",
        Total = 500m,
        Status = OrderStatus.Paid,
        CreatedAt = paidAt,
        PaidAt = paidAt,
        TransactionId = "tx-42",
    };

    private void RefundReturns(bool result) =>
        _gateway.RefundAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

    [Fact]
    public async Task CancelOrderAsync_PaidRecently_RefundsAndCancels()
    {
        var order = PaidOrder(Now.AddDays(-3));
        RefundReturns(true);

        await _sut.CancelOrderAsync(order, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(Now, order.CancelledAt);
        await _gateway.Received(1).RefundAsync(Arg.Is("tx-42"), Arg.Is(500m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelOrderAsync_Success_SendsCancellationEmail()
    {
        var order = PaidOrder(Now.AddDays(-1));
        RefundReturns(true);

        await _sut.CancelOrderAsync(order, CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            Arg.Is("buyer@example.com"), Arg.Is("Order cancelled"), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelOrderAsync_ExactlyFourteenDays_IsStillAllowed()
    {
        var order = PaidOrder(Now - OrderService.CancellationWindow);
        RefundReturns(true);

        await _sut.CancelOrderAsync(order, CancellationToken.None);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelOrderAsync_WindowExpired_ThrowsAndDoesNotRefund()
    {
        var order = PaidOrder(Now - OrderService.CancellationWindow - TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CancelOrderAsync(order, CancellationToken.None));

        Assert.Equal(OrderStatus.Paid, order.Status);
        await _gateway.DidNotReceiveWithAnyArgs().RefundAsync(default!, default, default);
    }

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.PaymentFailed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelOrderAsync_NotPaid_ThrowsAndDoesNotRefund(OrderStatus status)
    {
        var order = PaidOrder(Now.AddDays(-1));
        order.Status = status;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CancelOrderAsync(order, CancellationToken.None));

        await _gateway.DidNotReceiveWithAnyArgs().RefundAsync(default!, default, default);
    }

    [Fact]
    public async Task CancelOrderAsync_RefundDeclined_ThrowsAndKeepsOrderPaid()
    {
        var order = PaidOrder(Now.AddDays(-2));
        RefundReturns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CancelOrderAsync(order, CancellationToken.None));

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Null(order.CancelledAt);
        await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task CancelOrderAsync_NullOrder_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.CancelOrderAsync(null!, CancellationToken.None));
    }
}
