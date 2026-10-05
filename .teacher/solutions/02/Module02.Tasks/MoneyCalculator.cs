namespace Module02.Tasks;

public static class MoneyCalculator
{
    private const int CentsDecimals = 2;
    private const decimal CentsPerUnit = 100m;

    public static decimal ApplyDiscount(decimal price, decimal discountPercent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        ArgumentOutOfRangeException.ThrowIfNegative(discountPercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(discountPercent, 100m);

        // Сначала умножаем, потом делим: деление первым (discountPercent / 100) для decimal
        // не страшно, но для целых типов дало бы 0. Привычка «умножение раньше деления» полезна.
        return RoundToCents(price * (100m - discountPercent) / 100m);
    }

    public static decimal AddVat(decimal amount, decimal vatPercent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(vatPercent);

        return RoundToCents(amount * (100m + vatPercent) / 100m);
    }

    public static decimal[] Split(decimal total, int parts)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parts);

        // Переходим к целым копейкам: делить деньги «как есть» (100 / 3 = 33.333…)
        // и потом округлять — значит потерять или создать копейку из воздуха.
        // Целочисленное деление + остаток гарантируют: сумма частей == total.
        var cents = total * CentsPerUnit;
        if (cents != decimal.Truncate(cents))
        {
            throw new ArgumentException("Сумма не может содержать доли копейки.", nameof(total));
        }

        var totalCents = (long)cents;
        var baseCents = totalCents / parts;   // сколько получает каждый
        var remainder = totalCents % parts;   // сколько «лишних» копеек раздать первым

        var result = new decimal[parts];
        for (var i = 0; i < parts; i++)
        {
            var share = baseCents + (i < remainder ? 1 : 0);
            result[i] = share / CentsPerUnit; // long / decimal → decimal (implicit conversion long → decimal)
        }

        return result;
    }

    public static decimal CompoundInterest(decimal principal, decimal annualRatePercent, int years)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(principal);
        ArgumentOutOfRangeException.ThrowIfNegative(annualRatePercent);
        ArgumentOutOfRangeException.ThrowIfNegative(years);

        // Округляем один раз в конце: округление на каждом шаге накапливает ошибку.
        // (В реальном банке правило задаёт договор — и тогда округляют там, где он велит.)
        var multiplier = 1m + annualRatePercent / 100m;
        var amount = principal;
        for (var year = 0; year < years; year++)
        {
            amount *= multiplier;
        }

        return RoundToCents(amount);
    }

    // Явный MidpointRounding: по умолчанию Math.Round использует банковское округление (ToEven),
    // и 0.125 стало бы 0.12. Для чеков привычнее «школьное» — от нуля.
    private static decimal RoundToCents(decimal value) =>
        Math.Round(value, CentsDecimals, MidpointRounding.AwayFromZero);
}
