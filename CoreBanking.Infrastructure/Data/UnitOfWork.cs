using CoreBanking.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;

    public UnitOfWork(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        LogTrackedChanges();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void LogTrackedChanges()
    {
        var entries = _context.ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added ||
                        e.State == EntityState.Modified ||
                        e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            Console.WriteLine($"Entity: {entry.Entity.GetType().Name}");
            Console.WriteLine($"State: {entry.State}");

            if (entry.State == EntityState.Modified)
            {
                foreach (var property in entry.Properties)
                {
                    var originalValue = property.OriginalValue;
                    var currentValue = property.CurrentValue;

                    if (!Equals(originalValue, currentValue))
                    {
                        Console.WriteLine($" - {property.Metadata.Name}: {originalValue} → {currentValue}");
                    }
                }
            }

            if (entry.State == EntityState.Added)
            {
                foreach (var property in entry.Properties)
                {
                    Console.WriteLine($" + {property.Metadata.Name}: {property.CurrentValue}");
                }
            }

            if (entry.State == EntityState.Deleted)
            {
                foreach (var property in entry.Properties)
                {
                    Console.WriteLine($" x {property.Metadata.Name}: {property.OriginalValue}");
                }
            }

            Console.WriteLine("---------------------------------------------------");
        }
    }
}