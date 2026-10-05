using Module14.Tasks;
using NSubstitute;

namespace Module14.Tests;

// Задача 14.4 (см. exercises.md). Один тест — ОБРАЗЕЦ работы с NSubstitute. Допиши остальные.
public class OrderServicePlaceOrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    // xUnit создаёт НОВЫЙ экземпляр тест-класса для КАЖДОГО теста,
    // поэтому поля и конструктор — естественное место для общего Arrange.
    // Тесты не делят состояние и не влияют друг на друга.
    private readonly IPaymentGateway _gateway = Substitute.For<IPaymentGateway>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly OrderService _sut; // sut = system under test

    public OrderServicePlaceOrderTests()
    {
        _clock.UtcNow.Returns(Now); // stub: «сейчас» всегда одно и то же
        _sut = new OrderService(_gateway, _emailSender, _clock);
    }

    [Fact]
    public async Task PlaceOrderAsync_PaymentSucceeds_OrderIsPaidAndConfirmationSent()
    {
        // Arrange: шлюз «успешно» списывает 300 (если используешь Arg.* — используй для всех аргументов)
        _gateway.ChargeAsync(Arg.Is("buyer@example.com"), Arg.Is(300m), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(PaymentResult.Success("tx-001")));
        var lines = new[] { new OrderLine("BOOK-1", 2, 150m) };

        // Act
        var order = await _sut.PlaceOrderAsync("buyer@example.com", lines, CancellationToken.None);

        // Assert: состояние...
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal("tx-001", order.TransactionId);
        Assert.Equal(Now, order.PaidAt);
        // ...и взаимодействие (mock): письмо отправлено ровно один раз
        await _emailSender.Received(1).SendAsync(
            Arg.Is("buyer@example.com"), Arg.Is("Order confirmed"), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // TODO: отказ платежа, исключение шлюза, сумма со скидкой, пустой email, передача токена...
}
