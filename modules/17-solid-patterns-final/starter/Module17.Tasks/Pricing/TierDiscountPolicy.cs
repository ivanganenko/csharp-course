namespace Module17.Tasks;

/// <summary>
/// Задача 3. Скидка по уровню клиента — перенеси сюда логику из OrderProcessor (ровно ту же!).
/// <list type="bullet">
/// <item>Ставка: Regular — 0%, Silver — 5%, Gold — 10%.</item>
/// <item>Если subtotal ≥ <see cref="BulkThreshold"/> — к ставке прибавляется <see cref="BulkRate"/> (3%).</item>
/// <item>Скидка = subtotal × ставка, округление до 2 знаков, MidpointRounding.AwayFromZero.</item>
/// <item>subtotal &lt; 0 → ArgumentOutOfRangeException.</item>
/// </list>
/// </summary>
public sealed class TierDiscountPolicy : IDiscountPolicy
{
    public const decimal BulkThreshold = 20000m;
    public const decimal BulkRate = 0.03m;

    public decimal CalculateDiscount(CustomerTier tier, decimal subtotal)
    {
        throw new NotImplementedException();
    }
}
