namespace Module05.Tasks;

public enum CustomerType
{
    Regular,
    Silver,
    Gold,
    Employee,
}

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

// Было: одна функция знает про ВСЕ типы клиентов (switch). Новый тип → правка switch,
// и так в каждом месте, где есть похожий switch (доставка, бонусы, лимиты...).
// Стало: знание о скидке живёт в самом типе клиента. Новый тип → новый класс, старый код не трогаем
// (Open/Closed principle). switch по типу — частый «запах», который лечится полиморфизмом.
public abstract class Customer
{
    public abstract decimal DiscountPercent { get; }
}

public sealed class RegularCustomer : Customer
{
    public override decimal DiscountPercent => 0m;
}

public sealed class SilverCustomer : Customer
{
    public override decimal DiscountPercent => 5m;
}

public sealed class GoldCustomer : Customer
{
    private const decimal BaseDiscount = 10m;
    private const decimal MaxDiscount = 20m;

    public GoldCustomer(int yearsWithUs)
    {
        // В legacy отрицательные годы молча давали меньшую скидку — в новой модели это ошибка данных.
        ArgumentOutOfRangeException.ThrowIfNegative(yearsWithUs);
        YearsWithUs = yearsWithUs;
    }

    public int YearsWithUs { get; }

    public override decimal DiscountPercent => Math.Min(BaseDiscount + YearsWithUs, MaxDiscount);
}

public sealed class EmployeeCustomer : Customer
{
    public override decimal DiscountPercent => 30m;
}

public static class PriceCalculator
{
    public static decimal FinalPrice(Customer customer, decimal price)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentOutOfRangeException.ThrowIfNegative(price);

        var percent = customer.DiscountPercent;

        // Базовый класс не может запретить наследнику вернуть 150% — проверяем контракт на границе.
        if (percent < 0 || percent > 100)
        {
            throw new InvalidOperationException(
                $"{customer.GetType().Name} returned invalid discount {percent}%.");
        }

        // Та же формула, что в legacy, — иначе из-за порядка операций могла бы отличаться точность округления.
        var result = price - price * percent / 100;
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }
}
