using CoreBanking.Core.Common;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Persistence.Configurations;
using CoreBanking.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CoreBanking.Infrastructure.Data
{
    public class BankingDbContext : DbContext
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options)
            : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!; // Uses this style to effect Outbox pattern
        public DbSet<DomainEvent> DomainEvents { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.Ignore<IDomainEvent>();
            modelBuilder.Ignore<AccountId>();
            modelBuilder.Ignore<ContactInfo>();

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .HasConversion(customerId => customerId.Value,
                                value => CustomerId.Create(value));

                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                //entity.Property(c => c.Email).IsRequired().HasMaxLength(255);
                //entity.Property(c => c.PhoneNumber).HasMaxLength(20);

                // Customer has many Accounts
                entity.HasMany(c => c.Accounts)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                    .HasConversion(userId => userId.Value,
                                value => UserId.Create(value));
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                //entity.Property(u => u.DeletedBy).IsRequired();
                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .IsRequired();
            });

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(c => c.Id)
                    .HasConversion(AccountId => AccountId.Value,
                                value => AccountId.Create(value));

                // Configure AccountNumber as owned type (Value Object)
                entity.Property(a => a.AccountNumber)
                    .HasConversion(
                        accountNumber => accountNumber.Value,
                        value => AccountNumber.Create(value))
                    .HasColumnName("AccountNumber")
                    .HasMaxLength(10)
                    .IsRequired();

                // Configure CurrentBalance as owned type (Value Object)
                entity.OwnsOne(a => a.CurrentBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("CurrentBalanceAmount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("CurrentBalanceCurrency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                // Configure AvailableBalance as owned type (Value Object)
                entity.OwnsOne(a => a.AvailableBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("AvailableBalanceAmount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("AvailableBalanceCurrency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                entity.Property(a => a.AccountType)
                    .HasConversion<string>()
                    .IsRequired();

                // Account has many Transactions
                entity.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId);

                // Ensure we don't accidentally load all transactions
                entity.Navigation(a => a.Transactions).AutoInclude(false);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(c => c.Id)
                    .HasConversion(TransactionId => TransactionId.Value,
                                value => TransactionId.Create(value));

                // Configure AccountId foreign key
                entity.Property(t => t.AccountId)
                    .HasConversion(
                        accountId => accountId.Value,
                        value => AccountId.Create(value))
                    .IsRequired();

                // Configure RelatedAccountId foreign key (nullable, for transfers)
                entity.Property(t => t.RelatedAccountId)
                    .HasConversion(
                        accountId => accountId == null ? (Guid?)null : accountId.Value,
                        value => value == null ? null : AccountId.Create(value.Value))
                    .IsRequired(false);

                // Configure RelatedAccount relationship - MUST use NoAction to prevent cascade conflict
                entity.HasOne(t => t.RelatedAccount)
                    .WithMany()
                    .HasForeignKey(t => t.RelatedAccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Configure Money as owned type
                entity.OwnsOne(t => t.Amount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Amount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3);
                });

                entity.Property(t => t.Type)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(t => t.Description).HasMaxLength(500);
                entity.Property(t => t.Reference).HasMaxLength(50);
                entity.Property(t => t.DateCreated).IsRequired();
            });

            // Global query filter in DbContext - Automatically Exclude Deleted Records
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.Account.IsDeleted);

            // Account concurrency implementation
            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(a => a.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            // Seed the DB
            modelBuilder.Entity<Customer>().HasData(new {
                    Id = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice.johnson@email.com",
                    PhoneNumber = "555-0101",
                    BVN = "20000000009",
                    CreditScore = 40,
                    DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2025, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                    IsDeleted = false
                }
            );

            modelBuilder.Entity<Account>().HasData(new {
                    Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    AccountNumber = AccountNumber.Create("1000000001"),
                    AccountType = AccountType.Checking,
                    AccountStatus = AccountStatus.Active,// EF handles enum conversion
                    CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    DateOpened = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2025, 11, 11, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false
                }
            );

            // Seed the owned Money types for Account
            modelBuilder.Entity<Account>().OwnsOne(a => a.CurrentBalance).HasData(
                new
                {
                    AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    Amount = 1500.00m,
                    Currency = "NGN"
                }
            );

            modelBuilder.Entity<Account>().OwnsOne(a => a.AvailableBalance).HasData(
                new
                {
                    AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    Amount = 1500.00m,
                    Currency = "NGN"
                }
            );

            modelBuilder.Entity<User>().HasData(
                new
                {
                    Id = UserId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    Username = "admin_user",
                    PasswordHash = "$2a$11$8gT9vYQYZ5XJZKqJX5qJUeY7vQ3qY5qYF5vQ3qY5qYF5vQ3qY5qYF", // Password123!
                    Role = UserRole.Admin,
                    Email = "azeezokhamena@gmail.com",
                    IsActive = true,
                    FailedLoginAttempts = 0,
                    IsDeleted = false,
                    DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2025, 11, 11, 0, 0, 0, DateTimeKind.Utc)
                }
                );

            // Then configure the owned types separately
            //modelBuilder.Entity<Account>().OwnsOne(a => a.AvailableBalance).HasData(
            //    new
            //    {
            //        AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
            //        Amount = 1500.00m,
            //        Currency = "NGN"
            //    }
            //);

        }

        public async Task SaveChangesWithOutboxAsync(CancellationToken cancellationToken = default)
        {
            // Convert domain events to outbox messages
            var events = ChangeTracker.Entries<AggregateRoot<AccountId>>()
                .SelectMany(x => x.Entity.DomainEvents)
                .Select(domainEvent => new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    OccurredOn = domainEvent.OccurredOn
                })
                .ToList();

            // Clear domain events from aggregates
            ChangeTracker.Entries<AggregateRoot<AccountId>>()
                .ToList()
                .ForEach(entry => entry.Entity.ClearDomainEvents());

            // Save changes (including outbox messages) in single transaction
            await base.SaveChangesAsync(cancellationToken);

            // Add outbox messages after saving to ensure they're included in transaction
            if (events.Any())
            {
                await OutboxMessages.AddRangeAsync(events, cancellationToken);
                await base.SaveChangesAsync(cancellationToken);
            }
        }

    }
}