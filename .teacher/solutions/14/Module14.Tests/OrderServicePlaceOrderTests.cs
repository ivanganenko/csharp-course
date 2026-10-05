using Module14.Tasks;
using NSubstitute;

namespace Module14.Tests;

public class OrderServicePlaceOrderTests
{
    private const string Email = "buyer@example.com";
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly OrderService _sut;

    public OrderServicePlaceOrderTests()
    {
        _clock.UtcNow.Returns(Now);
        _sut = new OrderService(_gateway, _emailSender, _clock);
    }

    private static OrderLine[] TwoBooks() => [new OrderLine("BOOK-1", 2, 150m)];

    private void GatewayReturns(PaymentResult result) =>
        _gateway.ChargeAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));

    [Fact]
    public async Task PlaceOrderAsync_PaymentSucceeds_OrderIsPaidAndConfirmationSent()
    {
        _gateway.ChargeAsync(Arg.Is(Email), Arg.Is(300m), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(PaymentResult.Success("tx-001")));

        var order = await _sut.PlaceOrderAsync(Email, TwoBooks(), CancellationToken.None);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal("tx-001", order.TransactionId);
        Assert.Equal(Now, order.PaidAt);
        await _emailSender.Received(1).SendAsync(
            Arg.Is(Email), Arg.Is("Order confirmed"), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlaceOrderAsync_SetsTotalAndCreatedAt()
    {
        GatewayReturns(PaymentResult.Success("tx-002"));

        var order = await _sut.PlaceOrderAsync(Email, TwoBooks(), CancellationToken.None);

        Assert.Equal(300m, order.Total);
        Assert.Equal(Now, order.CreatedAt);
        Assert.Equal(Email, order.CustomerEmail);
    }

    [Fact]
    public async Task PlaceOrderAsync_ChargesTotalWithDiscount()
    {
        GatewayReturns(PaymentResult.Success("tx-003"));
        OrderLine[] lines = [new OrderLine("LAPTOP", 1, 1000m)];

        await _sut.PlaceOrderAsync(Email, lines, CancellationToken.None);

        // Проверяем ВЗАИМОДЕЙСТВИЕ: в шлюз ушла сумма со скидкой 10%.
        await _gateway.Received(1).ChargeAsync(Arg.Is(Email), Arg.Is(900m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PlaceOrderAsync_PaymentDeclined_StatusPaymentFailed_NoEmail()
    {
        GatewayReturns(PaymentResult.Failure("Insufficient funds"));

        var order = await _sut.PlaceOrderAsync(Email, TwoBooks(), CancellationToken.None);

        Assert.Equal(OrderStatus.PaymentFailed, order.Status);
        Assert.Null(order.PaidAt);
        Assert.Null(order.TransactionId);
        await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task PlaceOrderAsync_GatewayThrows_ExceptionPropagates_NoEmail()
    {
        _gateway.ChargeAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<PaymentResult>(new HttpRequestException("Bank is unavailable")));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => _sut.PlaceOrderAsync(Email, TwoBooks(), CancellationToken.None));

        await _emailSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!, default!, default);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PlaceOrderAsync_EmptyEmail_ThrowsArgumentException_AndDoesNotCharge(string email)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.PlaceOrderAsync(email, TwoBooks(), CancellationToken.None));

        await _gateway.DidNotReceiveWithAnyArgs().ChargeAsync(default!, default, default);
    }

    [Fact]
    public async Task PlaceOrderAsync_PassesCancellationTokenToDependencies()
    {
        GatewayReturns(PaymentResult.Success("tx-004"));
        using var cts = new CancellationTokenSource();

        await _sut.PlaceOrderAsync(Email, TwoBooks(), cts.Token);

        await _gateway.Received(1).ChargeAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Is(cts.Token));
        await _emailSender.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is(cts.Token));
    }
}
