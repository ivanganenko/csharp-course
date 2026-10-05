namespace Module17.Tasks;

/// <summary>Отправка уведомления клиенту (абстракция: email, SMS, мессенджер…).</summary>
public interface INotificationSender
{
    void Send(string recipient, string message);
}

/// <summary>Не удалось отправить уведомление.</summary>
public sealed class NotificationException(string message) : Exception(message)
{
}
