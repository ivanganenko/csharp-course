namespace Module01.Tasks;

/// <summary>
/// Задача 01.4 — калькулятор чаевых.
/// Деньги считаем в decimal (почему не double — подробно в модуле 02; коротко: double не умеет точно хранить 0.1).
/// Тесты: dotnet test --filter "FullyQualifiedName~TipCalculatorTests"
/// </summary>
public static class TipCalculator
{
    /// <summary>
    /// Сумма чаевых: bill * percent / 100, округлённая до 2 знаков после запятой
    /// по правилу «половина — от нуля» (MidpointRounding.AwayFromZero): 6.825 → 6.83.
    /// bill &lt; 0, percent &lt; 0 или percent &gt; 100 → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal CalculateTip(decimal bill, decimal percent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Итоговая сумма: bill + CalculateTip(bill, percent). Те же правила валидации.
    /// </summary>
    public static decimal Total(decimal bill, decimal percent)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сколько должен заплатить каждый из <paramref name="people"/> человек:
    /// Total(bill, percent) / people, округлённое ВВЕРХ до копейки (0.01),
    /// чтобы компания гарантированно собрала не меньше нужной суммы: 110 / 3 = 36.666… → 36.67.
    /// people ≤ 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static decimal PerPerson(decimal bill, decimal percent, int people)
    {
        throw new NotImplementedException();
    }
}
