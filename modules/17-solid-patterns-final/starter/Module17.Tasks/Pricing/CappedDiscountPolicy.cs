namespace Module17.Tasks;

/// <summary>
/// Задача 3. Decorator: оборачивает любую политику скидок и ограничивает скидку сверху.
/// Новое поведение добавляется БЕЗ изменения TierDiscountPolicy (Open/Closed Principle).
/// </summary>
public sealed class CappedDiscountPolicy : IDiscountPolicy
{
    /// <summary>inner == null → ArgumentNullException; maxDiscount &lt; 0 → ArgumentOutOfRangeException.</summary>
    public CappedDiscountPolicy(IDiscountPolicy inner, decimal maxDiscount)
    {
        throw new NotImplementedException();
    }

    /// <summary>Скидка внутренней политики, но не больше maxDiscount.</summary>
    public decimal CalculateDiscount(CustomerTier tier, decimal subtotal)
    {
        throw new NotImplementedException();
    }
}
