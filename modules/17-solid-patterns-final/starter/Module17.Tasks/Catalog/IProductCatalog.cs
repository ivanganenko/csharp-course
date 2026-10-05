namespace Module17.Tasks;

/// <summary>Товар каталога.</summary>
public sealed record Product(string Sku, string Name, decimal Price, decimal WeightKg);

/// <summary>Каталог товаров (абстракция для DIP — задача 6).</summary>
public interface IProductCatalog
{
    /// <summary>Товар по артикулу или null, если такого нет. Сравнение артикулов — точное (ordinal).</summary>
    Product? FindBySku(string sku);
}
