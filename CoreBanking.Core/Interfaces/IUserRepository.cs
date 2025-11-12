using CoreBanking.Core.Entities;
using CoreBanking.Core.Models;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task<PaginatedResult<User>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

        Task<bool> UserExistAsync(UserId userId, CancellationToken cancellationToken);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

        Task AddAsync(User user, CancellationToken cancellationToken);
        Task UpdateAsync(User user);
        //Task Delete(User user);
    }
}
