using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface IHoldRepository
    {
        Task<IReadOnlyCollection<Hold>> GetAllHoldsAsync(int pageNumber, int pageSize);
 
        Task<Hold?> GetByIdAsync(HoldId id);

        Task<IReadOnlyList<Hold>> GetByAccountNumberAsync(string accountNumber);

        Task AddAsync(Hold hold);

        Task UpdateAsync(Hold hold);

        Task RemoveAsync(AccountId accountId, HoldId holdId);

    }
}
