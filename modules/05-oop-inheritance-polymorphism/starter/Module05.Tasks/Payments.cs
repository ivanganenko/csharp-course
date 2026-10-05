namespace Module05.Tasks;

/// <summary>Результат оплаты. Уже готов — менять не нужно.</summary>
public sealed class PaymentResult
{
    private PaymentResult(bool success, string method, decimal charged, string? error)
    {
        Success = success;
        Method = method;
        Charged = charged;
        Error = error;
    }

    public bool Success { get; }

    /// <summary>Имя способа оплаты (PaymentMethod.Name) или "none".</summary>
    public string Method { get; }

    /// <summary>Сколько списано всего (сумма + комиссия). При неудаче — 0.</summary>
    public decimal Charged { get; }

    /// <summary>Текст ошибки при неудаче, иначе null.</summary>
    public string? Error { get; }

    public static PaymentResult Ok(string method, decimal charged) => new(true, method, charged, null);

    public static PaymentResult Failed(string method, string error) => new(false, method, 0m, error);
}

/// <summary>
/// Задача 05.5 — способ оплаты. Паттерн Template Method:
/// Pay — общий НЕвиртуальный алгоритм, а «шаги» (комиссия, проверка, списание) переопределяют наследники.
/// </summary>
public abstract class PaymentMethod
{
    /// <summary>Имя способа оплаты.</summary>
    public abstract string Name { get; }

    /// <summary>Комиссия за оплату суммы amount.</summary>
    public abstract decimal CalculateFee(decimal amount);

    /// <summary>
    /// Можно ли оплатить сумму этим способом. Базовая версия: amount &gt; 0.
    /// Наследники могут добавить свои условия, вызвав base.CanPay(amount).
    /// </summary>
    public virtual bool CanPay(decimal amount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Алгоритм оплаты (одинаков для всех способов):
    /// 1) amount &lt;= 0 → ArgumentOutOfRangeException;
    /// 2) если !CanPay(amount) → PaymentResult.Failed(Name, &lt;любой непустой текст&gt;), ничего не списывается;
    /// 3) total = amount + CalculateFee(amount); Charge(total); → PaymentResult.Ok(Name, total).
    /// </summary>
    public PaymentResult Pay(decimal amount)
    {
        throw new NotImplementedException();
    }

    /// <summary>Фактическое списание итоговой суммы (вызывается только из Pay).</summary>
    protected abstract void Charge(decimal total);
}

/// <summary>
/// Банковская карта. Name → "Card". Комиссия 2% от суммы, округлённая до 2 знаков (AwayFromZero),
/// но не меньше 0.30. TotalCharged — сколько всего списано по карте.
/// </summary>
public sealed class CardPayment : PaymentMethod
{
    public const decimal FeeRate = 0.02m;
    public const decimal MinFee = 0.30m;

    public decimal TotalCharged => throw new NotImplementedException();

    public override string Name => throw new NotImplementedException();

    public override decimal CalculateFee(decimal amount) => throw new NotImplementedException();

    protected override void Charge(decimal total) => throw new NotImplementedException();
}

/// <summary>
/// Электронный кошелёк с балансом. Name → "Wallet". Комиссии нет.
/// CanPay: базовое условие И amount + комиссия &lt;= Balance. Charge уменьшает Balance.
/// balance &lt; 0 в конструкторе → ArgumentOutOfRangeException.
/// </summary>
public sealed class WalletPayment : PaymentMethod
{
    public WalletPayment(decimal balance)
    {
        throw new NotImplementedException();
    }

    public decimal Balance => throw new NotImplementedException();

    public override string Name => throw new NotImplementedException();

    public override decimal CalculateFee(decimal amount) => throw new NotImplementedException();

    public override bool CanPay(decimal amount) => throw new NotImplementedException();

    protected override void Charge(decimal total) => throw new NotImplementedException();
}

/// <summary>
/// Банковский перевод. Name → "BankTransfer". Фиксированная комиссия Fee = 1.00.
/// CanPay: базовое условие И amount &gt;= MinAmount (10). TotalCharged — сколько всего списано.
/// </summary>
public sealed class BankTransferPayment : PaymentMethod
{
    public const decimal Fee = 1.00m;
    public const decimal MinAmount = 10m;

    public decimal TotalCharged => throw new NotImplementedException();

    public override string Name => throw new NotImplementedException();

    public override decimal CalculateFee(decimal amount) => throw new NotImplementedException();

    public override bool CanPay(decimal amount) => throw new NotImplementedException();

    protected override void Charge(decimal total) => throw new NotImplementedException();
}

/// <summary>Оформление заказа — работает с любыми способами оплаты через базовый тип.</summary>
public static class Checkout
{
    /// <summary>
    /// Перебирает способы оплаты по порядку и платит ПЕРВЫМ, который может оплатить сумму (CanPay).
    /// Если ни один не подходит → PaymentResult.Failed("none", &lt;любой непустой текст&gt;).
    /// methods == null → ArgumentNullException; amount &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static PaymentResult PayWithFirstAvailable(IEnumerable<PaymentMethod> methods, decimal amount)
    {
        throw new NotImplementedException();
    }
}
