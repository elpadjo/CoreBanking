using CoreBanking.Core.Interfaces;
using CoreBanking.Core.Models;

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
    }
}