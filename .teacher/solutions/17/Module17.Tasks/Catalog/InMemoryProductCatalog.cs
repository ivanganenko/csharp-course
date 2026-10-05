namespace Module17.Tasks;

// Был static-класс с глобальным словарём — его нельзя было подменить в тестах.
// Теперь это обычный объект за интерфейсом: данные приходят через конструктор.
public sealed class InMemoryProductCatalog : IProductCatalog
{
    private readonly Dictionary<string, Product> _products;

    public InMemoryProductCatalog(IEnumerable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        _products = products.ToDictionary(p => p.Sku, StringComparer.Ordinal);
    }

    public Product? FindBySku(string sku) => _products.GetValueOrDefault(sku);

    public static InMemoryProductCatalog CreateDefault() => new(new[]
    {
        new Product("BOOK-001", "Книга «Основы C#»", 1500.00m, 0.6m),
        new Product("MUG-001", "Кружка разработчика", 500.00m, 0.4m),
        new Product("KB-001", "Механическая клавиатура", 6500.00m, 1.2m),
        new Product("MON-001", "Монитор 27\"", 18990.00m, 6.5m),
        new Product("CABLE-01", "Кабель USB-C", 390.00m, 0.1m),
        new Product("STICKER-01", "Набор стикеров", 199.90m, 0.05m),
    });
}
