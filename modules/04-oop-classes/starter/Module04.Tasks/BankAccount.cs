namespace Module04.Tasks;

/// <summary>Тип операции по счёту. Уже готов — менять не нужно.</summary>
public enum TransactionType
{
    Deposit,
    Withdrawal,
}

/// <summary>
/// Одна операция по счёту (запись в истории). Уже готов — менять не нужно.
/// Обратите внимание: у класса только get-свойства — после создания его нельзя изменить (immutable).
/// </summary>
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

    /// <summary>Баланс счёта сразу после этой операции.</summary>
    public decimal BalanceAfter { get; }
}

/// <summary>
/// Задача 04.1 — банковский счёт с инвариантами.
/// Инвариант (то, что всегда истинно для объекта): баланс никогда не бывает отрицательным,
/// а история операций меняется только через методы счёта.
/// Подсказка: замените выражения "=> throw ..." на свойства/поля. Сигнатуры public-членов не меняйте.
/// </summary>
public class BankAccount
{
    /// <summary>
    /// Создаёт счёт.
    /// owner: null, пустая строка или только пробелы → ArgumentException (или наследник, например ArgumentNullException).
    /// Пробелы по краям имени владельца убираются.
    /// initialBalance &lt; 0 → ArgumentOutOfRangeException.
    /// Начальный баланс НЕ записывается в историю: после создания History пуста.
    /// </summary>
    public BankAccount(string owner, decimal initialBalance = 0m)
    {
        throw new NotImplementedException();
    }

    /// <summary>Владелец счёта. Не меняется после создания.</summary>
    public string Owner => throw new NotImplementedException();

    /// <summary>Текущий баланс. Снаружи только читается — меняется только через Deposit/Withdraw/TransferTo.</summary>
    public decimal Balance => throw new NotImplementedException();

    /// <summary>
    /// История операций в порядке выполнения.
    /// Снаружи её нельзя изменить: если вернуть внутренний List&lt;T&gt;, вызывающий код сможет
    /// привести его к IList&lt;T&gt; и вызвать Add. Верните read-only обёртку.
    /// </summary>
    public IReadOnlyList<Transaction> History => throw new NotImplementedException();

    /// <summary>
    /// Пополнение. amount &lt;= 0 → ArgumentOutOfRangeException (баланс и история не меняются).
    /// В историю добавляется Transaction(Deposit, amount, новый баланс).
    /// </summary>
    public void Deposit(decimal amount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Снятие. amount &lt;= 0 → ArgumentOutOfRangeException.
    /// amount &gt; Balance → InvalidOperationException (баланс и история не меняются).
    /// Снять ровно весь баланс можно.
    /// В историю добавляется Transaction(Withdrawal, amount, новый баланс).
    /// </summary>
    public void Withdraw(decimal amount)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Перевод на другой счёт: снятие с этого счёта + пополнение target.
    /// target == null → ArgumentNullException; перевод самому себе → ArgumentException.
    /// Недостаточно средств → InvalidOperationException, и НИ ОДИН из счетов не меняется.
    /// </summary>
    public void TransferTo(BankAccount target, decimal amount)
    {
        throw new NotImplementedException();
    }
}
