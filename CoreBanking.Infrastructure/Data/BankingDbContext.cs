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

                // Configure Money as owned type (Value Object)
                entity.OwnsOne(a => a.AvailableBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("AvailableAmount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("AvailableCurrency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                entity.OwnsOne(a => a.CurrentBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("CurrentAmount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("CurrentCurrency")
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
            //modelBuilder.Entity<Transaction>(entity =>
            //{
            //    entity.HasKey(t => t.Id);
            //    entity.Property(c => c.Id)
            //        .HasConversion(TransactionId => TransactionId.Value,
            //                    value => TransactionId.Create(value));

            //    // Configure Money as owned type
            //    entity.OwnsOne(t => t.Amount, money =>
            //    {
            //        money.Property(m => m.Amount)
            //            .HasColumnName("Amount")
            //            .HasPrecision(18, 2);
            //        money.Property(m => m.Currency)
            //            .HasColumnName("Currency")
            //            .HasMaxLength(3);
            //    });

            //    entity.Property(t => t.Type)
            //        .HasConversion<string>()
            //        .IsRequired();

            //    entity.Property(t => t.Description).HasMaxLength(500);
            //    entity.Property(t => t.Reference).HasMaxLength(50);
            //    entity.Property(t => t.DateCreated).IsRequired();
            //});

            modelBuilder.Entity<Transaction>(entity =>
            {
                // Primary Key
                entity.HasKey(t => t.Id);

                // Transaction ID (Strongly-typed ID)
                entity.Property(t => t.Id)
                    .HasConversion(
                        id => id.Value,
                        value => TransactionId.Create(value))
                    .IsRequired();

                // Account ID (Strongly-typed ID - Foreign Key)
                entity.Property(t => t.AccountId)
                    .HasConversion(
                        id => id.Value,
                        value => AccountId.Create(value))
                    .IsRequired();

                // Related Account ID (Strongly-typed ID - Nullable Foreign Key)
                //entity.Property(t => t.RelatedAccountId)
                //    .HasConversion(
                //        id => id.Value,
                //        value => AccountId.Create(value));

                entity.Property(t => t.RelatedAccountId)
    .HasConversion<Guid?>(
        id => id == null ? null : id.Value,
        value => value.HasValue ? AccountId.Create(value.Value) : null);

                // Money Value Object (Owned Type)
                entity.OwnsOne(t => t.Amount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Amount")
                        .HasPrecision(18, 2)
                        .IsRequired();

                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                // Transaction Type Enum
                entity.Property(t => t.Type)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

                // Running Balance
                entity.Property(t => t.RunningBalance)
                    .HasPrecision(18, 2)
                    .IsRequired();

                // String Properties
                entity.Property(t => t.Description)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(t => t.Reference)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(t => t.TransactionReference)
                    .HasMaxLength(50)
                    .IsRequired();

                // Audit Properties
                entity.Property(t => t.DateCreated)
                    .IsRequired();

                entity.Property(t => t.DateUpdated)
                    .IsRequired();

                // Soft Delete Properties (ISoftDelete)
                //entity.Property(t => t.IsDeleted)
                //    .IsRequired()
                //    .HasDefaultValue(false);

                //entity.Property(t => t.DeletedAt);

                //entity.Property(t => t.DeletedBy)
                //    .HasMaxLength(100);

                // Navigation Properties
                entity.HasOne(t => t.Account)
                    .WithMany()
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();



                // Query Filter for Soft Delete
                //entity.HasQueryFilter(t => !t.IsDeleted);
            });
            // Global query filter in DbContext - Automatically Exclude Deleted Records
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
                DateOfBirth = new DateTime(1995, 1, 15, 0, 0, 0, DateTimeKind.Utc), // Static date
                DateCreated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), // Static date
                IsActive = true,
                    IsDeleted = false,
                    DateUpdated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc),
            }
            );

            modelBuilder.Entity<Account>().HasData(new
            {
                Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                AccountNumber = AccountNumber.Create("1234567890"),
                AccountType = AccountType.Checking, // EF handles enum conversion
                CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                Currency = "NGN",
                DateOpened = new DateTime(2025, 10, 11, 10, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false,
                AccountStatus = AccountStatus.Active,
                DateCreated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc),
            }
            );

            // Then configure the owned types separately
            modelBuilder.Entity<Account>().OwnsOne(a => a.CurrentBalance).HasData(
                new
                {
                    AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    Amount = 10500.00m,
                    Currency = "NGN"
                }
            );
            modelBuilder.Entity<Account>().OwnsOne(a => a.AvailableBalance).HasData(
                new
                {
                    AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    Amount = 10000.00m,
                    Currency = "NGN"
                }
            );


            modelBuilder.Entity<Transaction>().OwnsOne(t => t.Amount).HasData(
                new
                {
                    TransactionId = TransactionId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                    Amount = 1500.00m,
                    Currency = "NGN"
                }
            );
            modelBuilder.Entity<Transaction>().OwnsOne(t => t.Amount).HasData(
                new
                {
                    TransactionId =  TransactionId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                    Amount = 1500.00m,
                    Currency = "NGN"
                }
            );


            // Transaction 1: Initial Deposit for Alice
            modelBuilder.Entity<Transaction>().HasData(new
            {
                Id = TransactionId.Create(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                RelatedAccountId = (AccountId?)null,
                TransactionReference = "20241022120000-11111111",
                Type = TransactionType.Deposit,
                RunningBalance = 50000.00m,
                Description = "Initial Deposit",
                Reference = "DEP-001",
                DateCreated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), // Already static
                DateUpdated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), // Already static
                //IsDeleted = false,
                //DeletedAt = (DateTime?)null,
                //DeletedBy = (string?)null
            });

            // Transaction 2: Withdrawal from Alice's account
            modelBuilder.Entity<Transaction>().HasData(new
            {
                Id = TransactionId.Create(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                RelatedAccountId = (AccountId?)null,
                TransactionReference = "20241023100000-22222222",
                Type = TransactionType.Withdrawal,
                RunningBalance = 45000.00m,
                Description = "ATM Withdrawal",
                Reference = "WTH-001",
                DateCreated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), // Already static
                DateUpdated = new DateTime(2025, 10, 1, 10, 0, 0, DateTimeKind.Utc), // Already static,
                //IsDeleted = false,
                //DeletedAt = (DateTime?)null,
                //DeletedBy = (string?)null
            });


            // Then configure the owned types separately
            /*modelBuilder.Entity<Account>().OwnsOne(a => a.Balance).HasData(
                new
                {
                    AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    Amount = 1500.00m,
                    Currency = "NGN"
                }
            );*/

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