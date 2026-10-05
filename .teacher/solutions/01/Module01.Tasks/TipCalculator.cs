namespace Module01.Tasks;

public static class TipCalculator
{
    // Именованные константы вместо «магических чисел» 100 и 2 в формулах:
    // читатель сразу видит смысл, а изменить правило можно в одном месте.
    private const decimal MaxPercent = 100m;
    private const int MoneyDecimals = 2;

    public static decimal CalculateTip(decimal bill, decimal percent)
    {
        Validate(bill, percent);

        // decimal, а не double: деньги — десятичные дроби, double хранит их в двоичном виде
        // приближённо (0.1 + 0.2 != 0.3). Банковские расчёты на double — классический баг.
        // MidpointRounding.AwayFromZero явно: по умолчанию Math.Round использует
        // «банковское» округление (ToEven), и 6.825 превратилось бы в 6.82.
        return Math.Round(bill * percent / MaxPercent, MoneyDecimals, MidpointRounding.AwayFromZero);
    }

    public static decimal Total(decimal bill, decimal percent) =>
        bill + CalculateTip(bill, percent); // переиспользуем валидацию и округление, а не дублируем

    public static decimal PerPerson(decimal bill, decimal percent, int people)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(people);

        var share = Total(bill, percent) / people;

        // Округляем ВВЕРХ: при обычном округлении 3 × 36.67 ≥ 110, но 3 × 36.66 < 110 —
        // официанту не хватило бы копейки. Math.Ceiling работает только с целыми,
        // поэтому переходим в копейки и обратно.
        return Math.Ceiling(share * 100m) / 100m;
    }

    private static void Validate(decimal bill, decimal percent)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bill);
        ArgumentOutOfRangeException.ThrowIfNegative(percent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, MaxPercent);
    }
}
