namespace Module17.Tasks;

/// <summary>Политика скидок (Strategy). Возвращает сумму скидки в рублях для подытога заказа.</summary>
public interface IDiscountPolicy
{
    decimal CalculateDiscount(CustomerTier tier, decimal subtotal);
}
