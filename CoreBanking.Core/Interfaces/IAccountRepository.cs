using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;
using System.Threading.Tasks;

namespace CoreBanking.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(AccountId id, CancellationToken cancellationToken = default);
        Task<List<Account>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
        Task AddAsync(Account account, CancellationToken cancellationToken = default);
        Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
        Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default);

        Task<List<Account>> GetInactiveAccountsSinceAsync(DateTime sinceDate, CancellationToken cancellationToken);
        Task<List<Account>> GetInterestBearingAccountsAsync(CancellationToken cancellationToken);
        Task<List<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken);
        Task<List<Account>> GetAccountsByStatusAsync(bool status, CancellationToken cancellationToken = default);
        Task<List<Account>> GetAccountsWithLowBalanceAsync(decimal minimumBalance, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}