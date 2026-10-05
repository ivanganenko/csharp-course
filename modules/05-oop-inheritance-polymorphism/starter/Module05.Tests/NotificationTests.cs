using Module05.Tasks;

namespace Module05.Tests;

public class NotificationTests
{
    private static readonly Recipient Anna = new("Anna", email: "anna@example.com", phone: "+1-555-0100");
    private static readonly Recipient EmailOnly = new("Boris", email: "boris@example.com");
    private static readonly Recipient Nobody = new("Clara");

    // Свой канал прямо в тестах: NotificationService работает с ЛЮБОЙ реализацией интерфейса.
    // Format не реализован — используется default interface method.
    private sealed class FakeChannel : INotificationChannel
    {
        public int SendCount { get; private set; }

        public string Name => "Fake";

        public bool CanSend(Recipient recipient) => true;

        public void Send(Recipient recipient, string message) => SendCount++;
    }

    [Fact]
    public void EmailChannel_CanSend_DependsOnEmail()
    {
        var email = new EmailChannel();

        Assert.Equal("Email", email.Name);
        Assert.True(email.CanSend(Anna));
        Assert.False(email.CanSend(Nobody));
        Assert.False(email.CanSend(new Recipient("Dan", email: "   ")));
    }

    [Fact]
    public void EmailChannel_Send_UsesDefaultFormat()
    {
        var email = new EmailChannel();

        email.Send(Anna, "Hello");

        Assert.Single(email.Sent);
        Assert.Equal("anna@example.com: [Email] Hello", email.Sent[0]);
    }

    [Fact]
    public void Channels_SendWhenCannotSend_ThrowInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() => new EmailChannel().Send(Nobody, "Hi"));
        Assert.Throws<InvalidOperationException>(() => new SmsChannel().Send(EmailOnly, "Hi"));
    }

    [Fact]
    public void DefaultInterfaceMethod_IsAvailableThroughInterfaceReference()
    {
        INotificationChannel email = new EmailChannel();
        INotificationChannel fake = new FakeChannel();

        Assert.Equal("[Email] hi", email.Format("hi"));
        Assert.Equal("[Fake] hi", fake.Format("hi"));
    }

    [Fact]
    public void SmsChannel_OverridesFormat_NoPrefixAndTruncated()
    {
        INotificationChannel sms = new SmsChannel();
        var longText = new string('x', 100);

        Assert.Equal("SMS", sms.Name);
        Assert.Equal("Hi", sms.Format("Hi"));
        Assert.Equal(SmsChannel.MaxLength, sms.Format(longText).Length);
    }

    [Fact]
    public void SmsChannel_Send_RecordsPhoneAndFormattedText()
    {
        var sms = new SmsChannel();

        sms.Send(Anna, "Your code is 1234");

        Assert.Equal("+1-555-0100: Your code is 1234", Assert.Single(sms.Sent));
    }

    [Fact]
    public void Notify_SendsThroughEveryChannelThatCanSend()
    {
        var email = new EmailChannel();
        var sms = new SmsChannel();
        var service = new NotificationService(new INotificationChannel[] { email, sms });

        Assert.Equal(2, service.Notify(Anna, "Sale!"));
        Assert.Equal(1, service.Notify(EmailOnly, "Sale!"));
        Assert.Equal(0, service.Notify(Nobody, "Sale!"));

        Assert.Equal(2, email.Sent.Count);
        Assert.Single(sms.Sent);
    }

    [Fact]
    public void Notify_WorksWithAnyImplementation()
    {
        var fake = new FakeChannel();
        var service = new NotificationService(new List<INotificationChannel> { fake });

        service.Notify(Nobody, "ping");
        service.Notify(Anna, "ping");

        Assert.Equal(2, fake.SendCount);
    }

    [Fact]
    public void Notify_InvalidArguments_Throw()
    {
        var service = new NotificationService(new List<INotificationChannel>());

        Assert.Throws<ArgumentNullException>(() => service.Notify(null!, "text"));
        Assert.ThrowsAny<ArgumentException>(() => service.Notify(Anna, "  "));
        Assert.Throws<ArgumentNullException>(() => new NotificationService(null!));
    }
}
