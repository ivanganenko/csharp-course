namespace Module04.Tasks;

public enum TransactionType
{
    Deposit,
    Withdrawal,
}

public sealed class Transaction
{
    public Transaction(TransactionType type, decimal amount, decimal balanceAfter)
    {
        Type = type;
        Amount = amount;
        BalanceAfter = balanceAfter;
    }

    public TransactionType Type { get; }

    public decimal Amount { get; }

    public decimal BalanceAfter { get; }
}

public class BankAccount
{
    // readonly: ссылку на список нельзя подменить после конструктора — история не «потеряется».
    // Сам список изменяемый, но он private: менять его могут только методы этого класса.
    private readonly List<Transaction> _history = new();

    public BankAccount(string owner, decimal initialBalance = 0m)
    {
        // Guard clauses в начале конструктора: объект либо создаётся валидным, либо не создаётся вовсе.
        // Так инвариант «баланс >= 0» действует с первой же строчки жизни объекта.
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        ArgumentOutOfRangeException.ThrowIfNegative(initialBalance);

        Owner = owner.Trim();
        Balance = initialBalance;
    }

    // Get-only auto-property: присвоить можно только в конструкторе — владелец не меняется.
    public string Owner { get; }

    // private set: читать могут все, менять — только методы класса, которые соблюдают инвариант.
    // Публичный set позволил бы написать account.Balance = -1_000_000 в обход всех проверок.
    public decimal Balance { get; private set; }

    // AsReadOnly() возвращает обёртку ReadOnlyCollection<T>: Add/Remove на ней бросают NotSupportedException.
    // Просто вернуть _history как IReadOnlyList<T> недостаточно — интерфейс «прячет» методы
    // только от компилятора, а приведение (IList<T>)account.History даст доступ к оригиналу.
    public IReadOnlyList<Transaction> History => _history.AsReadOnly();

    public void Deposit(decimal amount)
    {
        EnsurePositive(amount);

        Balance += amount;
        _history.Add(new Transaction(TransactionType.Deposit, amount, Balance));
    }

    public void Withdraw(decimal amount)
    {
        EnsurePositive(amount);

        // Сначала проверяем, потом меняем состояние: если бросим исключение — объект остался прежним.
        if (amount > Balance)
        {
            throw new InvalidOperationException(
                $"Insufficient funds: requested {amount}, available {Balance}.");
        }

        Balance -= amount;
        _history.Add(new Transaction(TransactionType.Withdrawal, amount, Balance));
    }

    public void TransferTo(BankAccount target, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(target);

        // ReferenceEquals, а не ==: нас интересует «тот же самый объект», а == в будущем
        // может быть перегружен (см. модуль 06) и сравнивать по значению.
        if (ReferenceEquals(target, this))
        {
            throw new ArgumentException("Cannot transfer to the same account.", nameof(target));
        }

        // Порядок важен: Withdraw бросит исключение ДО того, как деньги попадут на target,
        // поэтому при нехватке средств ни один счёт не меняется. Deposit после успешного
        // Withdraw упасть не может: amount уже проверен как положительный.
        Withdraw(amount);
        target.Deposit(amount);
    }

    // Одна точка проверки суммы вместо дублирования в каждом методе (DRY).
    private static void EnsurePositive(decimal amount) =>
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
}
