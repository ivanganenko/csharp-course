namespace Module12.Tasks;

/// <summary>Данные события «цена изменилась». Готово, менять не нужно.</summary>
public sealed class PriceChangedEventArgs(string symbol, decimal price) : EventArgs
{
    public string Symbol { get; } = symbol;
    public decimal Price { get; } = price;
}

/// <summary>
/// Задача 12.2 (часть 1) — издатель (publisher) события.
/// Живёт долго (например, singleton на всё приложение). Всё, что на него подписано,
/// он держит через ссылки в своём delegate — и GC не может это собрать.
/// </summary>
public sealed class StockTicker
{
    /// <summary>Событие изменения цены.</summary>
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    /// <summary>
    /// Текущее количество подписчиков (обработчиков в событии). Нет подписчиков → 0.
    /// Подсказка: delegate.GetInvocationList().
    /// </summary>
    public int SubscriberCount => throw new NotImplementedException();

    /// <summary>
    /// Публикует новую цену: вызывает событие с <see cref="PriceChangedEventArgs"/>, sender = this.
    /// Если подписчиков нет — ничего не происходит (без исключения).
    /// symbol пустой или из пробелов → <see cref="ArgumentException"/>.
    /// </summary>
    public void Publish(string symbol, decimal price)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Задача 12.2 (часть 2) — подписчик, который НЕ должен утекать.
/// Виджет на экране показывает цену одного тикера. Экран закрыли → виджет должен
/// отписаться от <see cref="StockTicker"/>, иначе ticker продолжит держать его в памяти
/// (и вызывать его обработчик) вечно.
/// </summary>
public sealed class PriceWidget : IDisposable
{
    /// <summary>
    /// Подписывается на ticker.PriceChanged.
    /// ticker == null → ArgumentNullException; symbol пустой/пробелы → ArgumentException.
    /// </summary>
    public PriceWidget(StockTicker ticker, string symbol)
    {
        throw new NotImplementedException();
    }

    /// <summary>Тикер, который показывает виджет.</summary>
    public string Symbol => throw new NotImplementedException();

    /// <summary>Последняя полученная цена ПО СВОЕМУ symbol (null — ещё ничего не получено).</summary>
    public decimal? LastPrice => throw new NotImplementedException();

    /// <summary>Сколько обновлений по своему symbol получил виджет.</summary>
    public int UpdateCount => throw new NotImplementedException();

    /// <summary>
    /// Отписывается от ticker. Повторный вызов безопасен.
    /// После Dispose виджет не получает обновлений.
    /// </summary>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
