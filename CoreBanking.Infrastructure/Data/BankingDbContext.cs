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

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .HasConversion(
                        customerId => customerId.Value,
                        value => CustomerId.Create(value)
                    );

                // Basic properties
                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.BVN)
                    .IsRequired()
                    .HasMaxLength(11)
                    .IsFixedLength(); // BVN is always 11 digits

                entity.Property(c => c.CreditScore)
                    .IsRequired();

                entity.Property(c => c.DateOfBirth)
                    .IsRequired();

                entity.Property(c => c.IsActive)
                    .IsRequired();

                entity.Property(c => c.IsDeleted)
                    .IsRequired();

                entity.Property(c => c.DeletedAt)
                    .IsRequired(false);

                entity.Property(c => c.DeletedBy)
                    .HasMaxLength(255)
                    .IsRequired(false);

                // From AggregateRoot
                entity.Property(c => c.DateCreated)
                    .IsRequired();

                entity.Property(c => c.DateUpdated)
                    .IsRequired();

                // Configure ContactInfo as owned type with nested Address
                entity.OwnsOne(c => c.ContactInfo, contact =>
                {
                    contact.Property(e => e.Email)
                        .HasColumnName("Email")
                        .HasMaxLength(255)
                        .IsRequired();

                    contact.Property(p => p.PhoneNumber)
                        .HasColumnName("PhoneNumber")
                        .HasMaxLength(20)
                        .IsRequired();

                    // Configure nested Address as owned type within ContactInfo
                    contact.OwnsOne(c => c.Address, address =>
                    {
                        address.Property(s => s.Street)
                            .HasColumnName("Street")
                            .HasMaxLength(200)
                            .IsRequired();

                        address.Property(c => c.City)
                            .HasColumnName("City")
                            .HasMaxLength(100)
                            .IsRequired();

                        address.Property(s => s.State)
                            .HasColumnName("State")
                            .HasMaxLength(50)
                            .IsRequired();

                        address.Property(z => z.ZipCode)
                            .HasColumnName("ZipCode")
                            .HasMaxLength(20)
                            .IsRequired();

                        address.Property(c => c.Country)
                            .HasColumnName("Country")
                            .HasMaxLength(50)
                            .HasDefaultValue("US")
                            .IsRequired();
                    });
                });

                // Customer has many Accounts
                entity.HasMany(c => c.Accounts)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId);

                // Global query filter for soft delete
                entity.HasQueryFilter(c => !c.IsDeleted);
            });

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id)
                    .HasConversion(AccountId => AccountId.Value,
                                value => AccountId.Create(value));

                // Configure AccountNumber as owned type (Value Object)
                entity.Property(a => a.AccountNumber)
                    .HasConversion(
                        accountNumber => accountNumber.Value,
                        value => AccountNumber.Create(value))
                    .HasColumnName("AccountNumber")
                    .HasMaxLength(10)
                    .IsFixedLength() // Account Number is always 10 digits
                    .IsRequired();

                // Configure CurrentBalance as owned type
                entity.OwnsOne(a => a.CurrentBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("CurrentBalance")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                // Configure AvailableBalance as owned type
                entity.OwnsOne(a => a.AvailableBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("AvailableBalance")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("AvailableBalanceCurrency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                entity.Property(a => a.AccountType)
                    .HasConversion<string>()
                    .IsRequired();

                // From AggregateRoot
                entity.Property(a => a.DateCreated).IsRequired();
                entity.Property(a => a.DateUpdated).IsRequired();

                // Account-specific properties
                entity.Property(a => a.DateOpened).IsRequired();
                entity.Property(a => a.DateClosed).IsRequired(false);
                entity.Property(a => a.AccountStatus)
                    .HasConversion<string>()
                    .IsRequired();
                entity.Property(a => a.IsDeleted).IsRequired();
                entity.Property(a => a.DeletedAt).IsRequired(false);
                entity.Property(a => a.DeletedBy).HasMaxLength(255).IsRequired(false);
                entity.Property(a => a.RowVersion).IsRowVersion().IsConcurrencyToken();

                // Customer relationship
                entity.HasOne(a => a.Customer)
                    .WithMany(c => c.Accounts)
                    .HasForeignKey(a => a.CustomerId);

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
                entity.Property(t => t.Id)
                    .HasConversion(
                        transactionId => transactionId.Value,
                        value => TransactionId.Create(value)
                    );

                // Configure AccountId as simple property conversion
                entity.Property(t => t.AccountId)
                    .HasConversion(
                        accountId => accountId.Value,
                        value => AccountId.Create(value)
                    )
                    .IsRequired();

                // Configure RelatedAccountId as simple property conversion
                entity.Property(t => t.RelatedAccountId)
                    .HasConversion(
                        accountId => accountId != null ? accountId.Value : (Guid?)null,
                        value => value.HasValue ? AccountId.Create(value.Value) : null
                    )
                    .IsRequired(false);

                // Configure the ACTUAL navigation to Account (using Account property)
                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey("AccountId")  // Use string, not lambda
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure RelatedAccount navigation (optional)
                entity.HasOne(t => t.RelatedAccount)
                    .WithMany()  // No navigation back from Account
                    .HasForeignKey("RelatedAccountId")  // Use string, not lambda
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

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
                entity.Property(t => t.TransactionReference).HasMaxLength(100).IsRequired();
                entity.Property(t => t.RunningBalance).HasPrecision(18, 2);
                entity.Property(t => t.DateCreated).IsRequired();
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
            modelBuilder.Entity<Customer>().HasData(new
            {
                Id = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                FirstName = "Alice",
                LastName = "Johnson",
                BVN = "20000000009",
                CreditScore = 750,
                DateOfBirth = new DateTime(1993, 5, 15),
                IsActive = true,
                IsDeleted = false,
                DateCreated = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc), // Fixed date
                DateUpdated = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)  // Fixed date
            });

            // Seed the owned ContactInfo type separately
            modelBuilder.Entity<Customer>().OwnsOne(c => c.ContactInfo).HasData(
                new
                {
                    CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    Email = "alice.johnson@email.com",
                    PhoneNumber = "555-0101"
                }
            );

            // Seed the nested Address type within ContactInfo
            modelBuilder.Entity<Customer>().OwnsOne(c => c.ContactInfo)
                .OwnsOne(ci => ci.Address).HasData(
                new
                {
                    ContactInfoCustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    Street = "123 Main Street",
                    City = "Lagos",
                    State = "Lagos",
                    ZipCode = "100001",
                    Country = "Nigeria"
                }
            );

            modelBuilder.Entity<Account>().HasData(new
            {
                Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                AccountNumber = AccountNumber.Create("1000000001"),
                AccountType = AccountType.Checking,
                CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                DateOpened = new DateTime(2024, 1, 25, 14, 15, 0, DateTimeKind.Utc),     // Fixed date
                DateCreated = new DateTime(2024, 1, 25, 14, 15, 0, DateTimeKind.Utc),   // Fixed date
                DateUpdated = new DateTime(2024, 1, 25, 14, 15, 0, DateTimeKind.Utc),   // Fixed date
                AccountStatus = AccountStatus.Active,
                IsDeleted = false
            });

            // Seed the owned balance types for Account
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