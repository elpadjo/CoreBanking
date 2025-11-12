using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBanking.Infrastructure.Repositories
{
    public class HoldRepository : IHoldRepository
    {
        private readonly BankingDbContext _context;

        public HoldRepository(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Hold?> GetByIdAsync(HoldId id)
        {
            return await _context.Holds
                .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
        }


        public async Task<IReadOnlyList<Hold>> GetByAccountNumberAsync(string accountNumber)
        {
            var account = await _context.Accounts
        .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

            if (account == null)
                return new List<Hold>();

            return await _context.Holds
                .Where(h => h.AccountId == account.Id && !h.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(Hold hold)
        {
            await _context.Holds.AddAsync(hold);
        }


        public async Task UpdateAsync(Hold hold)
        {
            // EF Core tracks entities, so attaching if not already tracked
            var tracked = await _context.Holds.FindAsync(hold.Id);
            if (tracked is null)
            {
                _context.Holds.Update(hold);
            }
            else
            {
                _context.Entry(tracked).CurrentValues.SetValues(hold);
            }
            await SaveChangesAsync();
        }

        public async Task RemoveAsync(AccountId accountId, HoldId holdId)
        {
            var hold = await _context.Holds
                .FirstOrDefaultAsync(h => h.AccountId == accountId && h.Id == holdId && !h.IsDeleted);

            if (hold is not null)
            {
                // Soft delete
                hold.SoftDelete("Admin");

                _context.Holds.Update(hold);

                await SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Hold>> GetAllHoldsAsync(int pageNumber, int pageSize)
        {
            return await _context.Holds
          .Where(h => !h.IsDeleted)
          .OrderByDescending(h => h.DateCreated)
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();
        }
    }
}
