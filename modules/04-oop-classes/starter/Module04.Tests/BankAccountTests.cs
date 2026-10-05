using Module04.Tasks;

namespace Module04.Tests;

public class BankAccountTests
{
    [Fact]
    public void Constructor_ValidArguments_SetsOwnerAndBalanceAndEmptyHistory()
    {
        var account = new BankAccount("  Anna Petrova ", 100m);

        Assert.Equal("Anna Petrova", account.Owner);
        Assert.Equal(100m, account.Balance);
        Assert.Empty(account.History);
        Assert.Equal(0m, new BankAccount("Boris").Balance); // initialBalance по умолчанию — 0
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankOwner_ThrowsArgumentException(string? owner)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BankAccount(owner!, 10m));
    }

    [Fact]
    public void Constructor_NegativeInitialBalance_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BankAccount("Anna", -1m));
    }

    [Fact]
    public void Deposit_PositiveAmount_IncreasesBalance()
    {
        var account = new BankAccount("Anna", 10m);

        account.Deposit(15.50m);

        Assert.Equal(25.50m, account.Balance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void DepositAndWithdraw_NonPositiveAmount_ThrowAndKeepState(int amount)
    {
        var account = new BankAccount("Anna", 10m);

        Assert.Throws<ArgumentOutOfRangeException>(() => account.Deposit(amount));
        Assert.Throws<ArgumentOutOfRangeException>(() => account.Withdraw(amount));
        Assert.Equal(10m, account.Balance);
        Assert.Empty(account.History);
    }

    [Theory]
    [InlineData(30, 70)]
    [InlineData(100, 0)]
    public void Withdraw_AmountNotGreaterThanBalance_DecreasesBalance(int amount, int expectedBalance)
    {
        var account = new BankAccount("Anna", 100m);

        account.Withdraw(amount);

        Assert.Equal(expectedBalance, account.Balance);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ThrowsInvalidOperationAndKeepsState()
    {
        var account = new BankAccount("Anna", 50m);

        Assert.Throws<InvalidOperationException>(() => account.Withdraw(50.01m));
        Assert.Equal(50m, account.Balance);
        Assert.Empty(account.History);
    }

    [Fact]
    public void History_RecordsOperationsInOrderWithBalanceAfter()
    {
        var account = new BankAccount("Anna", 100m);

        account.Deposit(50m);
        account.Withdraw(30m);

        Assert.Equal(2, account.History.Count);
        Assert.Equal(TransactionType.Deposit, account.History[0].Type);
        Assert.Equal(50m, account.History[0].Amount);
        Assert.Equal(150m, account.History[0].BalanceAfter);
        Assert.Equal(TransactionType.Withdrawal, account.History[1].Type);
        Assert.Equal(30m, account.History[1].Amount);
        Assert.Equal(120m, account.History[1].BalanceAfter);
    }

    [Fact]
    public void History_CannotBeModifiedFromOutside()
    {
        var account = new BankAccount("Anna", 100m);
        account.Deposit(1m);

        // Если History — это внутренний List<T>, его можно привести к IList<T> и «подделать» историю.
        if (account.History is IList<Transaction> list)
        {
            Assert.Throws<NotSupportedException>(
                () => list.Add(new Transaction(TransactionType.Deposit, 1_000_000m, 1_000_000m)));
        }

        Assert.Single(account.History);
    }

    [Fact]
    public void TransferTo_EnoughMoney_MovesMoneyAndRecordsHistory()
    {
        var from = new BankAccount("Anna", 100m);
        var to = new BankAccount("Boris", 10m);

        from.TransferTo(to, 40m);

        Assert.Equal(60m, from.Balance);
        Assert.Equal(50m, to.Balance);
        Assert.Equal(TransactionType.Withdrawal, from.History[from.History.Count - 1].Type);
        Assert.Equal(TransactionType.Deposit, to.History[to.History.Count - 1].Type);
    }

    [Fact]
    public void TransferTo_InsufficientFunds_ChangesNeitherAccount()
    {
        var from = new BankAccount("Anna", 100m);
        var to = new BankAccount("Boris", 10m);

        Assert.Throws<InvalidOperationException>(() => from.TransferTo(to, 100.01m));
        Assert.Equal(100m, from.Balance);
        Assert.Equal(10m, to.Balance);
        Assert.Empty(from.History);
        Assert.Empty(to.History);
    }

    [Fact]
    public void TransferTo_NullOrSelf_Throws()
    {
        var account = new BankAccount("Anna", 100m);

        Assert.Throws<ArgumentNullException>(() => account.TransferTo(null!, 10m));
        Assert.Throws<ArgumentException>(() => account.TransferTo(account, 10m));
        Assert.Equal(100m, account.Balance);
    }
}
