namespace Module17.Tasks;

// Реализация по умолчанию для конструктора без параметров — то же поведение, что у legacy SmtpEmailClient.
public sealed class ConsoleEmailSender : INotificationSender
{
    public void Send(string recipient, string message) =>
        Console.WriteLine($"[EMAIL] to={recipient}: {message}");
}
