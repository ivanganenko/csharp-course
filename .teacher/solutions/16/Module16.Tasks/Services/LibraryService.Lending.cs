using Microsoft.EntityFrameworkCore;
using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

public sealed partial class LibraryService
{
    public async Task<Loan> LendBookAsync(int bookId, int memberId, DateOnly today, CancellationToken cancellationToken = default)
    {
        // Транзакция объединяет проверки и запись: «проверил → записал» должно быть атомарным.
        // Оговорка: в SQLite запись в транзакции блокирует всю базу; в SQL Server/PostgreSQL при уровне
        // Read Committed две параллельные выдачи всё ещё могут пройти проверку одновременно — там нужны
        // Serializable, блокировка строки (SELECT ... FOR UPDATE) или optimistic concurrency (rowversion).
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken)
            ?? throw new NotFoundException($"Книга {bookId} не найдена");

        var memberExists = await _db.Members.AnyAsync(m => m.Id == memberId, cancellationToken);
        if (!memberExists)
        {
            throw new NotFoundException($"Читатель {memberId} не найден");
        }

        // Каждая проверка — COUNT/EXISTS в базе, а не загрузка всех выдач в память.
        var hasOverdue = await _db.Loans.AnyAsync(
            l => l.MemberId == memberId && l.ReturnedOn == null && l.DueOn < today,
            cancellationToken);
        if (hasOverdue)
        {
            throw new LibraryRuleException("У читателя есть просроченные книги");
        }

        var activeLoans = await _db.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedOn == null, cancellationToken);
        if (activeLoans >= MaxActiveLoansPerMember)
        {
            throw new LibraryRuleException($"Нельзя держать больше {MaxActiveLoansPerMember} книг одновременно");
        }

        var copiesOnLoan = await _db.Loans.CountAsync(l => l.BookId == bookId && l.ReturnedOn == null, cancellationToken);
        if (copiesOnLoan >= book.CopiesTotal)
        {
            throw new LibraryRuleException("Нет свободных экземпляров");
        }

        var loan = new Loan
        {
            BookId = bookId,
            MemberId = memberId,
            LoanedOn = today,
            DueOn = today.AddDays(LoanPeriodDays),
        };
        _db.Loans.Add(loan);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Если исключение случилось до CommitAsync, `await using` откатит транзакцию (Dispose → Rollback).
        return loan;
    }

    public async Task ReturnBookAsync(int loanId, DateOnly today, CancellationToken cancellationToken = default)
    {
        // Tracking-запрос: EF запомнит исходные значения и при SaveChanges сгенерирует UPDATE только изменённых колонок.
        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == loanId, cancellationToken)
            ?? throw new NotFoundException($"Выдача {loanId} не найдена");

        if (loan.ReturnedOn is not null)
        {
            throw new LibraryRuleException("Книга уже возвращена");
        }

        loan.ReturnedOn = today;

        // Одна SaveChanges — уже транзакция; явная не нужна.
        await _db.SaveChangesAsync(cancellationToken);
    }
}
