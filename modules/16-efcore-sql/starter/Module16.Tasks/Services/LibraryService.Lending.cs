using Module16.Tasks.Entities;

namespace Module16.Tasks.Services;

// Задача 4. Выдача и возврат книги: проверка инвариантов + транзакция.
public sealed partial class LibraryService
{
    /// <summary>
    /// Выдать книгу читателю на дату <paramref name="today"/>. Проверки — строго в этом порядке:
    /// <list type="number">
    /// <item>Книга существует, иначе <see cref="NotFoundException"/>.</item>
    /// <item>Читатель существует, иначе <see cref="NotFoundException"/>.</item>
    /// <item>У читателя нет просроченных выдач (ReturnedOn == null и DueOn &lt; today), иначе <see cref="LibraryRuleException"/>.</item>
    /// <item>У читателя меньше <see cref="MaxActiveLoansPerMember"/> активных выдач, иначе <see cref="LibraryRuleException"/>.</item>
    /// <item>Есть свободный экземпляр: активных выдач этой книги меньше CopiesTotal, иначе <see cref="LibraryRuleException"/>.</item>
    /// </list>
    /// Создаёт Loan (LoanedOn = today, DueOn = today + <see cref="LoanPeriodDays"/> дней), сохраняет и возвращает его (с Id).
    /// Проверки и запись выполняются в одной транзакции (Database.BeginTransactionAsync → CommitAsync).
    /// При нарушении правила в базе ничего не меняется.
    /// </summary>
    public Task<Loan> LendBookAsync(int bookId, int memberId, DateOnly today, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Вернуть книгу: ReturnedOn = today.
    /// Нет выдачи → <see cref="NotFoundException"/>; уже возвращена → <see cref="LibraryRuleException"/>.
    /// </summary>
    public Task ReturnBookAsync(int loanId, DateOnly today, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
