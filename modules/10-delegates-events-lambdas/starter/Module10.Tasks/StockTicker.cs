namespace Module10.Tasks;

/// <summary>Данные события «цена изменилась». Класс готов — менять не нужно.</summary>
public sealed class PriceChangedEventArgs : EventArgs
{
    public PriceChangedEventArgs(string symbol, decimal? oldPrice, decimal newPrice)
    {
        Symbol = symbol;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }

    /// <summary>Тикер инструмента, например "ACME".</summary>
    public string Symbol { get; }

    /// <summary>Предыдущая цена или null, если цена устанавливается впервые.</summary>
    public decimal? OldPrice { get; }

    public decimal NewPrice { get; }
}

/// <summary>
/// Задача 10.3 — биржевой тикер (вымышленные инструменты) с событием <see cref="PriceChanged"/>.
/// Тесты: StockTickerTests.
/// </summary>
public class StockTicker
{
    /// <summary>
    /// Событие изменения цены. sender — сам тикер (this).
    /// Объявлено как event, а не как публичное поле-делегат, — почему, см. README.
    /// </summary>
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    /// <summary>
    /// Количество подписчиков события (подсказка: <see cref="Delegate.GetInvocationList"/>).
    /// Пригодится, чтобы проверять отписку и искать утечки памяти.
    /// </summary>
    public int SubscriberCount => throw new NotImplementedException();

    /// <summary>Текущая цена инструмента или null, если цены ещё не было. Сравнение тикеров — ordinal.</summary>
    public decimal? GetPrice(string symbol)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Устанавливает новую цену и, если она ИЗМЕНИЛАСЬ (или устанавливается впервые), вызывает PriceChanged.
    /// Цена сохраняется ДО вызова события: обработчик, вызвавший GetPrice, видит уже новую цену.
    /// Если подписчиков нет — просто сохраняет цену (никаких NullReferenceException).
    /// symbol null/пустой/из пробелов → <see cref="ArgumentException"/> (или наследник);
    /// newPrice &lt; 0 → <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public void UpdatePrice(string symbol, decimal newPrice)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Подписчик: «оповещение о цене». При создании подписывается на тикер, при Dispose — отписывается.
/// Запоминает новые цены своего инструмента, которые больше или равны порогу.
/// </summary>
public sealed class PriceAlert : IDisposable
{
    /// <summary>
    /// Подписывается на <paramref name="ticker"/>.PriceChanged.
    /// ticker == null → <see cref="ArgumentNullException"/>; symbol null/пустой → <see cref="ArgumentException"/> (или наследник).
    /// </summary>
    public PriceAlert(StockTicker ticker, string symbol, decimal threshold)
    {
        throw new NotImplementedException();
    }

    /// <summary>Цены (NewPrice) своего инструмента, достигшие порога, в порядке поступления.</summary>
    public IReadOnlyList<decimal> TriggeredPrices => throw new NotImplementedException();

    /// <summary>Отписывается от события. Повторный вызов безопасен и ничего не делает.</summary>
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
