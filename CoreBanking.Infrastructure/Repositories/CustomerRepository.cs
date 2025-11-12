using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly BankingDbContext _context;

        public CustomerRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetByIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(int pageSize, int PageNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .Include(c => c.Accounts)
                .OrderByDescending(c => c.DateCreated)
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        }

        public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            await _context.Customers.AddAsync(customer, cancellationToken);
        }

        public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            _context.Customers.Update(customer);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Customers
                .AnyAsync(c => c.Id == customerId, cancellationToken);
        }
    }
}