using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CoreBanking.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _context;

        public AccountRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(AccountId accountId)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .ToListAsync();
        }

        public async Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<List<Account>> GetByCustomerIdAsync(CustomerId customerId)
        {
            return await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .ToListAsync();
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        public async Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber)
        {
            return await _context.Accounts
                .AnyAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<AccountNumber> GenerateAccountNumberAsync()
        {
            var conn = _context.Database.GetDbConnection();

            if (string.IsNullOrWhiteSpace(conn.ConnectionString))
                throw new InvalidOperationException("Database connection string is not initialized. Ensure UseSqlServer(...) is configured.");

            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NEXT VALUE FOR dbo.AccountNumberSeq";

            var result = (long)await cmd.ExecuteScalarAsync();

            return AccountNumber.Create(result.ToString("D10"));
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}