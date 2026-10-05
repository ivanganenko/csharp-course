namespace Module17.Tasks;

/// <summary>
/// Задача 5. Adapter: приспосабливает <see cref="LegacyMailGateway"/> к нашему интерфейсу <see cref="INotificationSender"/>.
/// </summary>
public sealed class MailGatewayNotificationSender : INotificationSender
{
    /// <summary>Тема всех писем.</summary>
    public const string Subject = "Ваш заказ";

    /// <summary>gateway == null → ArgumentNullException.</summary>
    public MailGatewayNotificationSender(LegacyMailGateway gateway)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Отправляет письмо через шлюз: тема — <see cref="Subject"/>, тело — "&lt;p&gt;" + HTML-экранированный message + "&lt;/p&gt;"
    /// (System.Net.WebUtility.HtmlEncode). Если шлюз вернул код ≠ 0 — бросает <see cref="NotificationException"/>,
    /// в сообщении которого есть этот код.
    /// </summary>
    public void Send(string recipient, string message)
    {
        throw new NotImplementedException();
    }
}
