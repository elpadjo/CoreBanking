using CoreBanking.Core.Entities;
using CoreBanking.Core.Models;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface IAccountRepository
    {
        AccountModel GetById(int id);
        IEnumerable<AccountModel> GetAll();
        void Add(AccountModel account);

        //DDD Account contracts
        Task<Account> GetByIdAsync(Guid accountId);
        Task<Account> GetByAccountNumberAsync(AccountNumber accountNumber);
        Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber);
    }
}