namespace Module17.Tasks;

// SRP: правило скидок живёт в одном классе. Поменялась маркетинговая политика —
// меняем только его, а OrderProcessor и доставку не трогаем (и не рискуем их сломать).
public sealed class TierDiscountPolicy : IDiscountPolicy
{
    public const decimal BulkThreshold = 20000m;
    public const decimal BulkRate = 0.03m;

    public decimal CalculateDiscount(CustomerTier tier, decimal subtotal)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(subtotal);

        var rate = tier switch
        {
            CustomerTier.Silver => 0.05m,
            CustomerTier.Gold => 0.10m,
            _ => 0m,
        };

        if (subtotal >= BulkThreshold)
        {
            rate += BulkRate;
        }

        // decimal — потому что деньги (модуль 02); AwayFromZero — так считал legacy-код,
        // а по умолчанию Math.Round использует банковское округление (ToEven).
        return Math.Round(subtotal * rate, 2, MidpointRounding.AwayFromZero);
    }
}
