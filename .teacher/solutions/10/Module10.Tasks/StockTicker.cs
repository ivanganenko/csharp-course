namespace Module10.Tasks;

public sealed class PriceChangedEventArgs : EventArgs
{
    public PriceChangedEventArgs(string symbol, decimal? oldPrice, decimal newPrice)
    {
        Symbol = symbol;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }

    public string Symbol { get; }

    public decimal? OldPrice { get; }

    public decimal NewPrice { get; }
}

public class StockTicker
{
    private readonly Dictionary<string, decimal> _prices = new(StringComparer.Ordinal);

    // event, а не public-поле делегата: снаружи можно только += и -=.
    // Внешний код не может вызвать событие от имени тикера и не может затереть чужие подписки через `= null`.
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    // Внутри класса событие — обычное поле-делегат, поэтому доступен GetInvocationList.
    public int SubscriberCount => PriceChanged?.GetInvocationList().Length ?? 0;

    public decimal? GetPrice(string symbol) =>
        _prices.TryGetValue(symbol, out decimal price) ? price : null;

    public void UpdatePrice(string symbol, decimal newPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentOutOfRangeException.ThrowIfNegative(newPrice);

        decimal? oldPrice = GetPrice(symbol);
        if (oldPrice == newPrice)
        {
            return; // ничего не изменилось — подписчиков не беспокоим
        }

        // Сначала меняем состояние, потом уведомляем: подписчик может обратиться к тикеру
        // и должен увидеть согласованное (новое) состояние.
        _prices[symbol] = newPrice;
        OnPriceChanged(new PriceChangedEventArgs(symbol, oldPrice, newPrice));
    }

    // Стандартный .NET-паттерн: protected virtual OnXxx — наследник может переопределить реакцию.
    protected virtual void OnPriceChanged(PriceChangedEventArgs e)
    {
        // ?.Invoke: если подписчиков нет, делегат равен null. Чтение поля происходит один раз,
        // поэтому нет гонки «проверили на null → другой поток отписался → NullReferenceException».
        PriceChanged?.Invoke(this, e);
    }
}

public sealed class PriceAlert : IDisposable
{
    private readonly StockTicker _ticker;
    private readonly string _symbol;
    private readonly decimal _threshold;
    private readonly List<decimal> _triggeredPrices = new();
    private bool _disposed;

    public PriceAlert(StockTicker ticker, string symbol, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(ticker);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _ticker = ticker;
        _symbol = symbol;
        _threshold = threshold;

        // Подписка через method group. Важно: событие хранит ссылку на this (PriceAlert),
        // поэтому, пока мы подписаны, тикер не даёт GC собрать оповещение — классическая утечка памяти.
        _ticker.PriceChanged += OnPriceChanged;
    }

    public IReadOnlyList<decimal> TriggeredPrices => _triggeredPrices;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // -= работает с НОВЫМ экземпляром делегата из того же method group: делегаты равны,
        // если совпадают target (this) и метод. С анонимной лямбдой так бы не вышло — её нужно сохранить в поле.
        _ticker.PriceChanged -= OnPriceChanged;
        _disposed = true;
    }

    private void OnPriceChanged(object? sender, PriceChangedEventArgs e)
    {
        if (e.Symbol == _symbol && e.NewPrice >= _threshold)
        {
            _triggeredPrices.Add(e.NewPrice);
        }
    }
}
