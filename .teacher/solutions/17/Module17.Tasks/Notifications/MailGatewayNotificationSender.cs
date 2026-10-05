using System.Globalization;
using System.Net;

namespace Module17.Tasks;

// Adapter: переводит «наш» интерфейс INotificationSender на язык сторонней библиотеки.
// Вся «грязь» чужого API (subject, HTML, коды ошибок) спрятана здесь; остальной код о ней не знает,
// и замена провайдера рассылки затронет только этот класс.
public sealed class MailGatewayNotificationSender : INotificationSender
{
    public const string Subject = "Ваш заказ";

    private readonly LegacyMailGateway _gateway;

    public MailGatewayNotificationSender(LegacyMailGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(gateway);
        _gateway = gateway;
    }

    public void Send(string recipient, string message)
    {
        // Экранирование обязательно: текст сообщения может содержать <, >, & — без него получим
        // сломанную вёрстку или HTML-инъекцию.
        var htmlBody = "<p>" + WebUtility.HtmlEncode(message) + "</p>";

        var code = _gateway.SendMail(recipient, Subject, htmlBody);

        // Коды ошибок → исключение: в нашем коде ошибки сообщаются исключениями, и проигнорировать их случайно нельзя.
        if (code != LegacyMailGateway.Ok)
        {
            throw new NotificationException(
                "Не удалось отправить письмо, код ошибки шлюза: " + code.ToString(CultureInfo.InvariantCulture));
        }
    }
}
