namespace Module17.Tasks;

// Decorator: реализует тот же интерфейс, что и оборачиваемый объект, и добавляет поведение «вокруг» него.
// Клиент (OrderProcessor) не знает, что работает с декоратором, — декораторы можно вкладывать друг в друга.
public sealed class CappedDiscountPolicy : IDiscountPolicy
{
    private readonly IDiscountPolicy _inner;
    private readonly decimal _maxDiscount;

    public CappedDiscountPolicy(IDiscountPolicy inner, decimal maxDiscount)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentOutOfRangeException.ThrowIfNegative(maxDiscount);

        _inner = inner;
        _maxDiscount = maxDiscount;
    }

    public decimal CalculateDiscount(CustomerTier tier, decimal subtotal) =>
        Math.Min(_inner.CalculateDiscount(tier, subtotal), _maxDiscount);
}
