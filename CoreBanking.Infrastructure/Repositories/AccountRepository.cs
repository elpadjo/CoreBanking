using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Exceptions;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CoreBanking.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext _context;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(BankingDbContext context, ILogger<AccountRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IUnitOfWork UnitOfWork => (IUnitOfWork)_context;

        public async Task<Account?> GetByIdAsync(AccountId id, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer) // ← Eager load Customer
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == id, cancellationToken);
        }

        public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer) 
                .Include(a => a.Transactions)
                .ToListAsync(cancellationToken);
        }

        public async Task<Account?> GetByAccountNumberAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }

        public async Task<IEnumerable<Account>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Where(a => a.CustomerId == customerId)
                .Include(a => a.Customer) 
                .Include(a => a.Transactions)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        {
            await _context.Accounts.AddAsync(account, cancellationToken);
        }

        public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Update(account);
            await Task.CompletedTask;
        }

        public async Task UpdateAccountBalanceAsync(AccountId accountId, Money newBalance, CancellationToken cancellationToken = default)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId, cancellationToken);

            if (account == null)
                throw new InvalidOperationException("Account not found.");

            // Replace the value object
            account.UpdateBalance(newBalance);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Account was modified by another user. Please refresh and try again.");
            }
        }

        public async Task<bool> AccountNumberExistsAsync(AccountNumber accountNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }


        public async Task<List<Account>> GetInactiveAccountsSinceAsync(DateTime sinceDate, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                //.Where(a => a.LastActivityDate < sinceDate &&
                .Where(a => a.DeletedAt < sinceDate &&
                           a.IsActive == true && // Only active accounts
                           a.Balance.Amount == 0)  // Only zero balance accounts
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetInterestBearingAccountsAsync(CancellationToken cancellationToken = default)
        {
            var interestBearingTypes = new[] { AccountType.Savings, AccountType.FixedDeposit };

            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => interestBearingTypes.Contains(a.AccountType) &&
                           a.IsActive == true)
                           //&& a.IsInterestBearing)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetActiveAccountsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.IsActive == true)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAccountsByStatusAsync(bool status, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.IsActive == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAccountsWithLowBalanceAsync(decimal minimumBalance, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Balance.Amount < minimumBalance &&
                           a.IsActive == true)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Account>> GetAccountsByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
        {
            return await _context.Accounts
                .Include(a => a.Customer)
                .Where(a => a.Customer.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteAsync(Account entity, CancellationToken cancellationToken = default)
        {
            _context.Accounts.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}