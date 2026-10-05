using Module17.Tasks;

namespace Module17.Tests;

/// <summary>Задача 5. Adapter для сторонней библиотеки.</summary>
public sealed class MailGatewayAdapterTests
{
    [Fact]
    public void Send_PassesAddressSubjectAndHtmlBody()
    {
        var gateway = new LegacyMailGateway();
        INotificationSender sender = new MailGatewayNotificationSender(gateway);

        sender.Send("anna@example.test", "Order 42 is ready");

        var mail = Assert.Single(gateway.Sent);
        Assert.Equal("anna@example.test", mail.Address);
        Assert.Equal(MailGatewayNotificationSender.Subject, mail.Subject);
        Assert.Equal("<p>Order 42 is ready</p>", mail.HtmlBody);
    }

    [Fact]
    public void Send_EncodesHtmlSpecialCharacters()
    {
        var gateway = new LegacyMailGateway();
        var sender = new MailGatewayNotificationSender(gateway);

        sender.Send("anna@example.test", "Tom & Jerry <b>sale</b>");

        Assert.Equal("<p>Tom &amp; Jerry &lt;b&gt;sale&lt;/b&gt;</p>", Assert.Single(gateway.Sent).HtmlBody);
    }

    [Fact]
    public void Send_KeepsCyrillicText()
    {
        var gateway = new LegacyMailGateway();
        var sender = new MailGatewayNotificationSender(gateway);

        sender.Send("anna@example.test", "Заказ оформлен");

        Assert.Contains("Заказ оформлен", Assert.Single(gateway.Sent).HtmlBody);
    }

    [Fact]
    public void Send_GatewayReturnsErrorCode_ThrowsNotificationException()
    {
        var gateway = new LegacyMailGateway();
        var sender = new MailGatewayNotificationSender(gateway);

        var ex = Assert.Throws<NotificationException>(() => sender.Send("not-an-email", "Hello"));

        Assert.Contains(LegacyMailGateway.InvalidAddress.ToString(System.Globalization.CultureInfo.InvariantCulture), ex.Message);
        Assert.Empty(gateway.Sent);
    }

    [Fact]
    public void Constructor_NullGateway_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MailGatewayNotificationSender(null!));
    }
}
