using CoreBanking.Core.Models;

namespace CoreBanking.Core.Interfaces
{
    public interface IAccountRepository
    {
        AccountModel GetById(int id);
        IEnumerable<AccountModel> GetAll();
        void Add(AccountModel account);
    }
}