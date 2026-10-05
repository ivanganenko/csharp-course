namespace Module05.Tasks;

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

    public string Method { get; }

    public decimal Charged { get; }

    public string? Error { get; }

    public static PaymentResult Ok(string method, decimal charged) => new(true, method, charged, null);

    public static PaymentResult Failed(string method, string error) => new(false, method, 0m, error);
}

public abstract class PaymentMethod
{
    public abstract string Name { get; }

    public abstract decimal CalculateFee(decimal amount);

    // virtual с разумной базовой реализацией: наследники РАСШИРЯЮТ её через base.CanPay(...),
    // а не копируют проверку amount > 0 каждый раз.
    public virtual bool CanPay(decimal amount) => amount > 0;

    // Template Method: алгоритм зафиксирован в базовом классе и НЕ virtual — наследник не может
    // «забыть» валидацию или вызвать Charge до проверки. Меняются только шаги.
    public PaymentResult Pay(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (!CanPay(amount))
        {
            return PaymentResult.Failed(Name, $"{Name} cannot process this payment.");
        }

        var total = amount + CalculateFee(amount);
        Charge(total);
        return PaymentResult.Ok(Name, total);
    }

    // protected: вызывать Charge напрямую снаружи нельзя — только через Pay с его проверками.
    protected abstract void Charge(decimal total);
}

public sealed class CardPayment : PaymentMethod
{
    public const decimal FeeRate = 0.02m;
    public const decimal MinFee = 0.30m;

    public decimal TotalCharged { get; private set; }

    public override string Name => "Card";

    public override decimal CalculateFee(decimal amount) =>
        Math.Max(Math.Round(amount * FeeRate, 2, MidpointRounding.AwayFromZero), MinFee);

    protected override void Charge(decimal total) => TotalCharged += total;
}

public sealed class WalletPayment : PaymentMethod
{
    public WalletPayment(decimal balance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(balance);
        Balance = balance;
    }

    public decimal Balance { get; private set; }

    public override string Name => "Wallet";

    public override decimal CalculateFee(decimal amount) => 0m;

    // Расширяем базовое условие, а не заменяем его.
    public override bool CanPay(decimal amount) =>
        base.CanPay(amount) && amount + CalculateFee(amount) <= Balance;

    protected override void Charge(decimal total) => Balance -= total;
}

public sealed class BankTransferPayment : PaymentMethod
{
    public const decimal Fee = 1.00m;
    public const decimal MinAmount = 10m;

    public decimal TotalCharged { get; private set; }

    public override string Name => "BankTransfer";

    public override decimal CalculateFee(decimal amount) => Fee;

    public override bool CanPay(decimal amount) => base.CanPay(amount) && amount >= MinAmount;

    protected override void Charge(decimal total) => TotalCharged += total;
}

public static class Checkout
{
    public static PaymentResult PayWithFirstAvailable(IEnumerable<PaymentMethod> methods, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(methods);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        // Ни одного switch по типу: каждый способ оплаты сам отвечает на вопрос «могу ли я?».
        foreach (var method in methods)
        {
            if (method.CanPay(amount))
            {
                return method.Pay(amount);
            }
        }

        return PaymentResult.Failed("none", "No payment method can process this amount.");
    }
}
