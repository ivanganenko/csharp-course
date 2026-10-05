namespace Module06.Tasks;

public sealed record ProductDto(string Sku, string Name, decimal Price);

public static class ProductOperations
{
    public static ProductDto ApplyDiscount(ProductDto product, int percent)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfNegative(percent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 100);

        var newPrice = Math.Round(product.Price * (100 - percent) / 100m, 2, MidpointRounding.AwayFromZero);

        // with копирует record и меняет только указанные свойства. Исходный объект не трогаем —
        // его может держать кто-то ещё (кэш, другой поток), и неожиданное изменение было бы багом.
        return product with { Price = newPrice };
    }

    public static ProductDto Rename(ProductDto product, string newName)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        return product with { Name = newName.Trim() };
    }
}

public sealed record ShoppingList(string Owner, IReadOnlyList<string> Items)
{
    // Синтезированное равенство record сравнило бы Items через EqualityComparer<IReadOnlyList<string>>.Default,
    // то есть ПО ССЫЛКЕ. Для коллекций это почти никогда не то, что нужно, поэтому заменяем.
    // В sealed record метод не virtual (в не-sealed он обязан быть virtual).
    public bool Equals(ShoppingList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Owner, other.Owner, StringComparison.Ordinal)
            && Items.SequenceEqual(other.Items);
    }

    // Переопределили Equals — обязаны переопределить и GetHashCode (иначе warning CS8851 и,
    // что важнее, сломанный HashSet: равные списки попадут в разные бакеты).
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Owner); // string.GetHashCode — ordinal, согласовано с Ordinal в Equals
        foreach (var item in Items)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }
}
