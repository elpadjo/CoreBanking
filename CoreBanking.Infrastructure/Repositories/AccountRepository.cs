using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.Models;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly List<AccountModel> _accounts = new()
            {
                new AccountModel { Id = 1, Name = "John Doe", Balance = 5000 },
                new AccountModel { Id = 2, Name = "Jane Smith", Balance = 8000 }
            };

        public IEnumerable<AccountModel> GetAll() => _accounts;

        public AccountModel GetById(int id) => _accounts.FirstOrDefault(a => a.Id == id)!;

        public void Add(AccountModel account) => _accounts.Add(account);

        public Task<Account> GetByIdAsync(Guid accountId)
        {
            throw new NotImplementedException();
        }

        public Task<Account> GetByAccountNumberAsync(AccountNumber accountNumber)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId)
        {
            throw new NotImplementedException();
        }

        public Task AddAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Account account)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber)
        {
            throw new NotImplementedException();
        }
    }
}