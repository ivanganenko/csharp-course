using System.Reflection;
using Module08.Tasks;

namespace Module08.Tests;

/// <summary>
/// Почему тесты используют reflection и приведение через object?
/// Если бы тест писал просто <c>IMessageSource&lt;Message&gt; s = emailSource;</c>, то без out в интерфейсе
/// НЕ скомпилировался бы весь тестовый проект — и упали бы тесты всех задач модуля.
/// Поэтому вариантность проверяется во время выполнения: приведение через object
/// компилируется всегда, а успешно выполняется только для вариантного интерфейса.
/// </summary>
public class VarianceTests
{
    [Fact]
    public void IMessageSource_IsCovariant()
    {
        Type parameter = typeof(IMessageSource<>).GetGenericArguments()[0];

        Assert.True(parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant),
            "Объявите параметр типа как out T");
    }

    [Fact]
    public void IMessageHandler_IsContravariant()
    {
        Type parameter = typeof(IMessageHandler<>).GetGenericArguments()[0];

        Assert.True(parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant),
            "Объявите параметр типа как in T");
    }

    [Fact]
    public void QueueMessageSource_ReturnsMessagesInOrder_ThenNull()
    {
        var source = new QueueMessageSource<SmsMessage>(new[]
        {
            new SmsMessage("Code 1111", "+70000000001"),
            new SmsMessage("Code 2222", "+70000000002"),
        });

        Assert.Equal("Code 1111", source.Next()!.Text);
        Assert.Equal("Code 2222", source.Next()!.Text);
        Assert.Null(source.Next());
        Assert.Null(source.Next());
    }

    [Fact]
    public void QueueMessageSource_CopiesInput()
    {
        var input = new List<EmailMessage> { new("Hi", "user1@example.com") };
        var source = new QueueMessageSource<EmailMessage>(input);

        input.Add(new EmailMessage("Added later", "user2@example.com"));

        Assert.Equal("Hi", source.Next()!.Text);
        Assert.Null(source.Next());
    }

    [Fact]
    public void QueueMessageSource_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new QueueMessageSource<EmailMessage>(null!));
    }

    [Fact]
    public void EmailSource_CanBeUsedAsSourceOfAnyMessages()
    {
        object emailSource = new QueueMessageSource<EmailMessage>(new[] { new EmailMessage("Hello", "user@example.com") });

        // Covariance: IMessageSource<EmailMessage> → IMessageSource<Message>
        var anySource = Assert.IsAssignableFrom<IMessageSource<Message>>(emailSource);

        Message? message = anySource.Next();
        Assert.IsType<EmailMessage>(message);
    }

    [Fact]
    public void LoggingHandler_RecordsTypeNameAndText()
    {
        var handler = new LoggingHandler();

        handler.Handle(new EmailMessage("Hello", "user@example.com"));
        handler.Handle(new SmsMessage("Code 1234", "+70000000000"));

        Assert.Equal(new[] { "EmailMessage: Hello", "SmsMessage: Code 1234" }, handler.Log);
    }

    [Fact]
    public void LoggingHandler_Null_ThrowsArgumentNullException()
    {
        var handler = new LoggingHandler();

        Assert.Throws<ArgumentNullException>(() => handler.Handle(null!));
    }

    [Fact]
    public void GeneralHandler_CanBeUsedAsEmailHandler()
    {
        var logging = new LoggingHandler();

        // Contravariance: IMessageHandler<Message> → IMessageHandler<EmailMessage>
        var emailHandler = Assert.IsAssignableFrom<IMessageHandler<EmailMessage>>((object)logging);
        emailHandler.Handle(new EmailMessage("Invoice", "user@example.com"));

        Assert.Equal(new[] { "EmailMessage: Invoice" }, logging.Log);
    }

    [Fact]
    public void Drain_PumpsAllMessages_AndReturnsCount()
    {
        var source = new QueueMessageSource<EmailMessage>(new[]
        {
            new EmailMessage("One", "user1@example.com"),
            new EmailMessage("Two", "user2@example.com"),
            new EmailMessage("Three", "user3@example.com"),
        });
        var logging = new LoggingHandler();
        var emailHandler = (IMessageHandler<EmailMessage>)(object)logging;

        int count = MessagePump.Drain(source, emailHandler);

        Assert.Equal(3, count);
        Assert.Equal(new[] { "EmailMessage: One", "EmailMessage: Two", "EmailMessage: Three" }, logging.Log);
        Assert.Null(source.Next());
    }

    [Fact]
    public void Drain_NullArguments_ThrowArgumentNullException()
    {
        var source = new QueueMessageSource<SmsMessage>(Array.Empty<SmsMessage>());
        var handler = (IMessageHandler<SmsMessage>)(object)new LoggingHandler();

        Assert.Throws<ArgumentNullException>(() => MessagePump.Drain<SmsMessage>(null!, handler));
        Assert.Throws<ArgumentNullException>(() => MessagePump.Drain(source, null!));
    }
}
