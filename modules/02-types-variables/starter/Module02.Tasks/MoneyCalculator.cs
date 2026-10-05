namespace Module02.Tasks;

/// <summary>
/// Задача 02.3 — денежные расчёты на decimal.
/// Везде, где сказано «округлить до копеек», — 2 знака, MidpointRounding.AwayFromZero.
/// Тесты: dotnet test --filter "FullyQualifiedName~MoneyCalculatorTests"
/// </summary>
public static class MoneyCalculator
{
    /// <summary>
    /// Цена со скидкой: price * (100 - discountPercent) / 100, округлить до копеек.
    /// 199.99 со скидкой 15% → 169.99 (169.9915 → 169.99).
    /// price &lt; 0 или discountPercent вне [0; 100] → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal ApplyDiscount(decimal price, decimal discountPercent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сумма с НДС: amount * (100 + vatPercent) / 100, округлить до копеек. 19.99 + 20% → 23.99.
    /// amount &lt; 0 или vatPercent &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal AddVat(decimal amount, decimal vatPercent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Делит сумму total на parts частей ТОЧНО до копейки: сумма частей должна быть равна total.
    /// Лишние копейки достаются первым частям: Split(100, 3) → [33.34, 33.33, 33.33];
    /// Split(0.05, 3) → [0.02, 0.02, 0.01].
    /// total &lt; 0 или parts ≤ 0 → ArgumentOutOfRangeException.
    /// total с долями копейки (например 10.005) → ArgumentException.
    /// </summary>
    public static decimal[] Split(decimal total, int parts)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Вклад с ежегодной капитализацией: каждый год сумма умножается на (1 + rate/100).
    /// Округлить до копеек ОДИН раз — в конце: 1000 под 5% на 3 года → 1157.63 (1157.625).
    /// years == 0 → principal. principal &lt; 0, annualRatePercent &lt; 0 или years &lt; 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal CompoundInterest(decimal principal, decimal annualRatePercent, int years)
    {
        throw new NotImplementedException();
    }
}
