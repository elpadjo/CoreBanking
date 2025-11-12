using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Customer>> GetAllAsync(int pageSize, int pageNumber, CancellationToken cancellationToken = default);
        Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
        Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    }
}