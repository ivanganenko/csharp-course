using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Services;

namespace Module16.Tests;

/// <summary>Задача 4. Выдача и возврат книги: инварианты + транзакция.</summary>
public sealed class LendingTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly LibraryData _data;

    public LendingTests()
    {
        _data = LibraryData.Seed(_database);
    }

    public void Dispose() => _database.Dispose();

    private int CountLoans()
    {
        using var db = _database.CreateContext();
        return db.Loans.Count();
    }

    [Fact]
    public async Task Lend_ValidRequest_CreatesLoanWithDueDate()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        var loan = await service.LendBookAsync(_data.RingOfSalt.Id, _data.Petr.Id, LibraryData.Today);

        Assert.True(loan.Id > 0);
        Assert.Equal(LibraryData.Today, loan.LoanedOn);
        Assert.Equal(new DateOnly(2026, 3, 24), loan.DueOn);
        Assert.Null(loan.ReturnedOn);

        await using var check = _database.CreateContext();
        var stored = await check.Loans.SingleAsync(l => l.Id == loan.Id);
        Assert.Equal(_data.RingOfSalt.Id, stored.BookId);
        Assert.Equal(_data.Petr.Id, stored.MemberId);
    }

    [Fact]
    public async Task Lend_MissingBook_ThrowsNotFound()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.LendBookAsync(999, _data.Petr.Id, LibraryData.Today));
    }

    [Fact]
    public async Task Lend_MissingMember_ThrowsNotFound()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.LendBookAsync(_data.RingOfSalt.Id, 999, LibraryData.Today));
    }

    [Fact]
    public async Task Lend_NoFreeCopies_ThrowsRule_AndDoesNotCreateLoan()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);
        var before = CountLoans();

        // Stone Garden: 1 экземпляр, он у Ивана.
        await Assert.ThrowsAsync<LibraryRuleException>(() => service.LendBookAsync(_data.StoneGarden.Id, _data.Petr.Id, LibraryData.Today));

        Assert.Equal(before, CountLoans());
    }

    [Fact]
    public async Task Lend_MemberHasOverdueLoan_ThrowsRule()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // У Ольги просрочены L3 и L4; Ring of Salt свободна.
        await Assert.ThrowsAsync<LibraryRuleException>(() => service.LendBookAsync(_data.RingOfSalt.Id, _data.Olga.Id, LibraryData.Today));
    }

    [Fact]
    public async Task Lend_MemberAtLimit_ThrowsRule()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // У Ивана 1 активная (L2). Ещё две — можно, четвёртая — нет.
        await service.LendBookAsync(_data.RingOfSalt.Id, _data.Ivan.Id, LibraryData.Today);
        await service.LendBookAsync(_data.AtlasOfFog.Id, _data.Ivan.Id, LibraryData.Today);

        await Assert.ThrowsAsync<LibraryRuleException>(() => service.LendBookAsync(_data.RingKeeper.Id, _data.Ivan.Id, LibraryData.Today));
        Assert.Equal(7, CountLoans());
    }

    [Fact]
    public async Task Lend_LastFreeCopy_ThenNextLendFails()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        // The Ring Keeper: 2 экземпляра, 1 на руках у Ольги (L4) → один свободный.
        await service.LendBookAsync(_data.RingKeeper.Id, _data.Petr.Id, LibraryData.Today);

        await Assert.ThrowsAsync<LibraryRuleException>(() => service.LendBookAsync(_data.RingKeeper.Id, _data.Ivan.Id, LibraryData.Today));
    }

    [Fact]
    public async Task Return_ActiveLoan_SetsReturnedOn_AndFreesCopy()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await service.ReturnBookAsync(_data.L2.Id, LibraryData.Today);
        var newLoan = await service.LendBookAsync(_data.StoneGarden.Id, _data.Petr.Id, LibraryData.Today);

        await using var check = _database.CreateContext();
        var returned = await check.Loans.SingleAsync(l => l.Id == _data.L2.Id);
        Assert.Equal(LibraryData.Today, returned.ReturnedOn);
        Assert.True(newLoan.Id > 0);
    }

    [Fact]
    public async Task Return_AlreadyReturned_ThrowsRule()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAsync<LibraryRuleException>(() => service.ReturnBookAsync(_data.L1.Id, LibraryData.Today));
    }

    [Fact]
    public async Task Return_MissingLoan_ThrowsNotFound()
    {
        await using var db = _database.CreateContext();
        var service = new LibraryService(db);

        await Assert.ThrowsAsync<NotFoundException>(() => service.ReturnBookAsync(999, LibraryData.Today));
    }
}
