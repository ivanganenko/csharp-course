namespace Module05.Tasks;

// Задача 05.7 ⭐ — рефакторинг switch-по-типу в полиморфизм.
// LegacyPriceCalculator — «старый» рабочий код: менять его НЕ нужно, тесты используют его как эталон поведения.
// Его проблема: чтобы добавить новый тип клиента, нужно лезть внутрь switch (и во все другие такие switch в проекте).
// Твоя задача — реализовать новую версию через иерархию Customer так, чтобы результаты совпадали с legacy,
// а новый тип клиента можно было добавить, НЕ меняя PriceCalculator.

/// <summary>Тип клиента в старом коде.</summary>
public enum CustomerType
{
    Regular,
    Silver,
    Gold,
    Employee,
}

/// <summary>Старый калькулятор. НЕ МЕНЯТЬ — это спецификация поведения.</summary>
public static class LegacyPriceCalculator
{
    public static decimal FinalPrice(CustomerType type, decimal price, int yearsWithUs)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price));
        }

        decimal discountPercent;
        switch (type)
        {
            case CustomerType.Regular:
                discountPercent = 0;
                break;
            case CustomerType.Silver:
                discountPercent = 5;
                break;
            case CustomerType.Gold:
                discountPercent = 10 + yearsWithUs;
                if (discountPercent > 20)
                {
                    discountPercent = 20;
                }

                break;
            case CustomerType.Employee:
                discountPercent = 30;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }

        var result = price - price * discountPercent / 100;
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// Клиент. Каждый наследник сам знает свою скидку — вместо switch по enum.
/// (Базовый класс уже готов; можно добавить в него что-то своё, но DiscountPercent должен остаться abstract.)
/// </summary>
public abstract class Customer
{
    /// <summary>Скидка в процентах, ожидается диапазон 0–100.</summary>
    public abstract decimal DiscountPercent { get; }
}

/// <summary>Обычный клиент: скидка 0%.</summary>
public sealed class RegularCustomer : Customer
{
    public override decimal DiscountPercent => throw new NotImplementedException();
}

/// <summary>Silver: скидка 5%.</summary>
public sealed class SilverCustomer : Customer
{
    public override decimal DiscountPercent => throw new NotImplementedException();
}

/// <summary>
/// Gold: скидка 10% + 1% за каждый год с нами, но не больше 20%.
/// yearsWithUs &lt; 0 → ArgumentOutOfRangeException.
/// </summary>
public sealed class GoldCustomer : Customer
{
    public GoldCustomer(int yearsWithUs)
    {
        throw new NotImplementedException();
    }

    public int YearsWithUs => throw new NotImplementedException();

    public override decimal DiscountPercent => throw new NotImplementedException();
}

/// <summary>Сотрудник: скидка 30%.</summary>
public sealed class EmployeeCustomer : Customer
{
    public override decimal DiscountPercent => throw new NotImplementedException();
}

/// <summary>Новый калькулятор — НЕ знает о конкретных типах клиентов.</summary>
public static class PriceCalculator
{
    /// <summary>
    /// Итоговая цена = price - price * DiscountPercent / 100, округление до 2 знаков AwayFromZero
    /// (точно как в LegacyPriceCalculator).
    /// customer == null → ArgumentNullException; price &lt; 0 → ArgumentOutOfRangeException;
    /// DiscountPercent вне диапазона 0–100 → InvalidOperationException (защита от «кривого» наследника).
    /// </summary>
    public static decimal FinalPrice(Customer customer, decimal price)
    {
        throw new NotImplementedException();
    }
}
