namespace Module10.Tasks;

/// <summary>
/// Задача 10.5 — простой EventBus (шина событий): издатели и подписчики не знают друг о друге,
/// общаются через типы сообщений. Пример: OrderPlaced → отправить email, списать склад, начислить бонусы.
/// Тесты: EventBusTests.
/// </summary>
public class EventBus
{
    /// <summary>
    /// Подписывает обработчик на сообщения ровно типа <typeparamref name="TEvent"/>
    /// (наследники и базовые типы не учитываются — для простоты).
    /// Возвращает <see cref="IDisposable"/>: его Dispose отписывает обработчик (повторный Dispose безопасен).
    /// null → <see cref="ArgumentNullException"/>.
    /// </summary>
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Вызывает всех подписчиков типа <typeparamref name="TEvent"/> в порядке подписки и возвращает их количество.
    /// Подписчики могут отписываться или подписывать новых прямо во время Publish:
    /// это не должно ломать текущую рассылку (вызываются те, кто был подписан на момент начала Publish).
    /// null-сообщение → <see cref="ArgumentNullException"/>.
    /// </summary>
    public int Publish<TEvent>(TEvent message)
    {
        throw new NotImplementedException();
    }

    /// <summary>Количество подписчиков на тип <typeparamref name="TEvent"/>.</summary>
    public int SubscriberCount<TEvent>()
    {
        throw new NotImplementedException();
    }
}
