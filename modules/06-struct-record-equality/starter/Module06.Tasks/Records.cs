namespace Module06.Tasks;

/// <summary>
/// Задача 06.2 (часть 1) — DTO (data transfer object) как positional record. Уже готов — менять не нужно.
/// Компилятор сам сгенерировал: init-свойства Sku/Name/Price, конструктор, Equals/GetHashCode/==
/// по значениям всех свойств, ToString, Deconstruct и поддержку with.
/// </summary>
public sealed record ProductDto(string Sku, string Name, decimal Price);

/// <summary>Операции над ProductDto. Каждая возвращает НОВЫЙ record через выражение with, исходный не меняется.</summary>
public static class ProductOperations
{
    /// <summary>
    /// Цена со скидкой percent% (0–100), округление до 2 знаков AwayFromZero.
    /// 19.99 со скидкой 10% → 17.99 (17.991). product == null → ArgumentNullException;
    /// percent вне 0–100 → ArgumentOutOfRangeException.
    /// </summary>
    public static ProductDto ApplyDiscount(ProductDto product, int percent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Новое имя (без пробелов по краям). product == null → ArgumentNullException;
    /// newName null/пусто/пробелы → ArgumentException (или наследник).
    /// </summary>
    public static ProductDto Rename(ProductDto product, string newName)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Задача 06.2 (часть 2) — ловушка: record сравнивает свойства через EqualityComparer&lt;T&gt;.Default,
/// а для списка это сравнение ССЫЛОК. Два списка покупок с одинаковыми товарами в разных экземплярах List
/// по умолчанию НЕ равны.
/// Переопредели равенство: списки равны, если совпадает Owner (с учётом регистра) и Items совпадают
/// поэлементно в том же порядке. other == null → false.
/// GetHashCode должен быть согласован с Equals.
/// </summary>
public sealed record ShoppingList(string Owner, IReadOnlyList<string> Items)
{
    public bool Equals(ShoppingList? other)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
