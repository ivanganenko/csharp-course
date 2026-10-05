namespace Module12.Tasks;

public sealed class PriceChangedEventArgs(string symbol, decimal price) : EventArgs
{
    public string Symbol { get; } = symbol;
    public decimal Price { get; } = price;
}

public sealed class StockTicker
{
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    // Внутри класса событие — обычное поле-delegate, поэтому можно посмотреть его invocation list.
    // null означает «нет подписчиков» (после отписки последнего delegate снова становится null).
    public int SubscriberCount => PriceChanged?.GetInvocationList().Length ?? 0;

    public void Publish(string symbol, decimal price)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        // ?.Invoke читает поле один раз — нет гонки «проверили на null, а между проверкой
        // и вызовом последний подписчик отписался».
        PriceChanged?.Invoke(this, new PriceChangedEventArgs(symbol, price));
    }
}

public sealed class PriceWidget : IDisposable
{
    private readonly StockTicker _ticker;
    private bool _disposed;

    public PriceWidget(StockTicker ticker, string symbol)
    {
        ArgumentNullException.ThrowIfNull(ticker);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        _ticker = ticker;
        Symbol = symbol;

        // Подписываемся МЕТОДОМ (method group), а не лямбдой «на месте»:
        // для отписки нужен delegate на тот же метод того же объекта.
        // Анонимную лямбду потом не отписать (если не сохранить её в поле).
        _ticker.PriceChanged += OnPriceChanged;
    }

    public string Symbol { get; }

    public decimal? LastPrice { get; private set; }

    public int UpdateCount { get; private set; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // Главное в задаче: издатель живёт дольше подписчика и держит его через delegate.
        // Без отписки виджет никогда не будет собран GC и продолжит получать события.
        _ticker.PriceChanged -= OnPriceChanged;
        _disposed = true;
    }

    private void OnPriceChanged(object? sender, PriceChangedEventArgs e)
    {
        if (e.Symbol != Symbol)
        {
            return;
        }

        LastPrice = e.Price;
        UpdateCount++;
    }
}
