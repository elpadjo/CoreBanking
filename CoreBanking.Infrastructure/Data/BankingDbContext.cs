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
            modelBuilder.Ignore<CustomerId>();

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

                entity.OwnsOne(c => c.ContactInfo, contact =>
                {
                    contact.Property(p => p.Email).IsRequired();
                    contact.Property(p => p.PhoneNumber).IsRequired();

                    contact.OwnsOne(p => p.Address, address =>
                    {
                        address.Property(p => p.Street).IsRequired();
                        address.Property(p => p.City).IsRequired();
                        address.Property(p => p.State).IsRequired();
                        address.Property(p => p.ZipCode);
                        address.Property(p => p.Country).IsRequired();
                    });
                });


                //entity.OwnsOne(c => c.CustomerId, owned =>
                //{
                //    owned.Property(c => c.Value)
                //         .HasColumnName("CustomerId")
                //         .IsRequired();
                //});

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
                entity.OwnsOne(a => a.CurrentBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Amount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                entity.OwnsOne(a => a.AvailableBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Amount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
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

                entity.Property(c => c.AccountId)
                    .HasConversion(Id => Id.Value,
                                value => AccountId.Create(value))
                    .IsRequired();

                entity.HasOne(t => t.RelatedAccount)
     .WithMany()
     .HasForeignKey("RelatedAccountId") // name of the DB column
     .OnDelete(DeleteBehavior.Restrict);

                entity.Property(t => t.RelatedAccountId)
                    .HasConversion(
                        id => id.Value,
                        value => AccountId.Create(value))
                    .HasColumnName("RelatedAccountId");


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

                entity.Property(t => t.RunningBalance)
              .HasColumnType("decimal")
                  .HasPrecision(18, 2)
                  .IsRequired();


                entity.Property(t => t.Type)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(t => t.Description).HasMaxLength(500);
                entity.Property(t => t.Reference).HasMaxLength(50);
                entity.Property(t => t.DateCreated).IsRequired();
            });

            //Hold configuration



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
                Email = "alice.johnson@email.com",
                BVN = "20000000009",
                CreditScore = 40,
                PhoneNumber = "555-0101",
                Street = "123 Main St",
                City = "Lagos",
                State = "Lagos",
                ZipCode = "100001",
                Country = "NG",
                DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false
            }
            );


            modelBuilder.Entity<Account>().HasData(new
            {
                Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                AccountNumber = AccountNumber.Create("1000000001"),
                AccountType = AccountType.Checking, // EF handles enum conversion
                AccountStatus = AccountStatus.Active,
                CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                Currency = "NGN",
                DateOpened = DateTime.UtcNow.AddDays(-20),
                DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false
            }
            );

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