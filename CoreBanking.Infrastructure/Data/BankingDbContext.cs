// using CoreBanking.Core.Common;
// using CoreBanking.Core.Entities;
// using CoreBanking.Core.Enums;
// using CoreBanking.Core.Interfaces;
// using CoreBanking.Core.ValueObjects;
// using CoreBanking.Infrastructure.Persistence.Configurations;
// using CoreBanking.Infrastructure.Persistence.Outbox;
// using Microsoft.EntityFrameworkCore;
// using System.Text.Json;

// namespace CoreBanking.Infrastructure.Data
// {
//     public class BankingDbContext : DbContext
//     {
//         public BankingDbContext(DbContextOptions<BankingDbContext> options)
//             : base(options) { }

//         public DbSet<Customer> Customers => Set<Customer>();
//         public DbSet<Account> Accounts => Set<Account>();
//         public DbSet<Transaction> Transactions => Set<Transaction>();
//         public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!; // Uses this style to effect Outbox pattern
//         public DbSet<DomainEvent> DomainEvents { get; set; }
//         // public DbSet<Transfer> Transfer {get; set;}


//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

//             base.OnModelCreating(modelBuilder);

//             modelBuilder.Ignore<DomainEvent>();
//             modelBuilder.Ignore<IDomainEvent>();
//             modelBuilder.Ignore<AccountId>();
//             modelBuilder.Ignore<ContactInfo>();
//             modelBuilder.Ignore<Hold>();

//             // Customer configuration
//             modelBuilder.Entity<Customer>(entity =>
//             {
//                 entity.HasKey(c => c.Id);
//                 entity.Property(c => c.Id)
//                     .HasConversion(customerId => customerId.Value,
//                                 value => CustomerId.Create(value));

//                 entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
//                 entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
//                 //entity.Property(c => c.Email).IsRequired().HasMaxLength(255);
//                 //entity.Property(c => c.PhoneNumber).HasMaxLength(20);

//                 // Customer has many Accounts
//                 entity.HasMany(c => c.Accounts)
//                     .WithOne(a => a.Customer)
//                     .HasForeignKey(a => a.CustomerId);

//                 entity.OwnsOne(c => c.ContactInfo, contact =>
//                 {
//                     contact.Property(p => p.Email).IsRequired();
//                     contact.Property(p => p.PhoneNumber).IsRequired();
//                     contact.OwnsOne(p => p.Address);
//                 });
//             });

//             // Account configuration
//             modelBuilder.Entity<Account>(entity =>
//             {
//                 entity.HasKey(a => a.Id);
//                 entity.Property(c => c.Id)
//                     .HasConversion(AccountId => AccountId.Value,
//                                 value => AccountId.Create(value));

//                 // Configure AccountNumber as owned type (Value Object)
//                 entity.Property(a => a.AccountNumber)
//                     .HasConversion(
//                         accountNumber => accountNumber.Value,
//                         value => AccountNumber.Create(value))
//                     .HasColumnName("AccountNumber")
//                     .HasMaxLength(10)
//                     .IsRequired();

//                 // Configure Money as owned type (Value Object)
//                 entity.OwnsOne(a => a.CurrentBalance, money =>
//                 {
//                     money.Property(m => m.Amount)
//                         .HasColumnName("Amount")
//                         .HasPrecision(18, 2);
//                     money.Property(m => m.Currency)
//                         .HasColumnName("Currency")
//                         .HasMaxLength(3)
//                         .HasDefaultValue("NGN");
//                 });

//                 entity.OwnsOne(a => a.AvailableBalance, money =>
//                 {
//                     money.Property(m => m.Amount)
//                         .HasColumnName("Amount")
//                         .HasPrecision(18, 2);
//                     money.Property(m => m.Currency)
//                         .HasColumnName("Currency")
//                         .HasMaxLength(3)
//                         .HasDefaultValue("NGN");
//                 });

//                 entity.Property(a => a.AccountType)
//                     .HasConversion<string>()
//                     .IsRequired();

//                 // Account has many Transactions
//                 entity.HasMany(a => a.Transactions)
//                     .WithOne(t => t.Account)
//                     .HasForeignKey(t => t.AccountId);

//                 // Ensure we don't accidentally load all transactions
//                 entity.Navigation(a => a.Transactions).AutoInclude(false);
//             });

//             // Transaction configuration
//             modelBuilder.Entity<Transaction>(entity =>
//             {
//                 entity.HasKey(t => t.Id);
//                 entity.Property(c => c.Id)
//                     .HasConversion(TransactionId => TransactionId.Value,
//                                 value => TransactionId.Create(value));

//                 entity.Property(c => c.AccountId)
//                     .HasConversion(Id => Id.Value,
//                                 value => AccountId.Create(value))
//                     .IsRequired();

//                 entity.HasOne(t => t.RelatedAccount)
//      .WithMany()
//      .HasForeignKey("RelatedAccountId") // name of the DB column
//      .OnDelete(DeleteBehavior.Restrict);

//                 entity.Property(t => t.RelatedAccountId)
//                     .HasConversion(
//                         id => id.Value,
//                         value => AccountId.Create(value))
//                     .HasColumnName("RelatedAccountId");


//                 // Configure Money as owned type
//                 entity.OwnsOne(t => t.Amount, money =>
//                 {
//                     money.Property(m => m.Amount)
//                         .HasColumnName("Amount")
//                         .HasPrecision(18, 2);
//                     money.Property(m => m.Currency)
//                         .HasColumnName("Currency")
//                         .HasMaxLength(3);
//                 });

//                 entity.Property(t => t.RunningBalance)
//               .HasColumnType("decimal")
//                   .HasPrecision(18, 2)
//                   .IsRequired();


//                 entity.Property(t => t.Type)
//                     .HasConversion<string>()
//                     .IsRequired();

//                 entity.Property(t => t.Description).HasMaxLength(500);
//                 entity.Property(t => t.Reference).HasMaxLength(50);
//                 entity.Property(t => t.DateCreated).IsRequired();
//             });

//             //Hold configuration



//             // Global query filter in DbContext - Automatically Exclude Deleted Records
//             modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
//             modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
//             modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.Account.IsDeleted);

//             // Account concurrency implementation
//             modelBuilder.Entity<Account>(entity =>
//             {
//                 entity.Property(a => a.RowVersion)
//                     .IsRowVersion()
//                     .IsConcurrencyToken();
//             });

//             // Seed the DB
//             modelBuilder.Entity<Customer>().HasData(new
//             {
//                 Id = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
//                 FirstName = "Alice",
//                 LastName = "Johnson",
//                 Email = "alice.johnson@email.com",
//                 PhoneNumber = "555-0101",
//                 BVN = "20000000009",
//                 CreditScore = 40,
//                 DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                 DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
//                 DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
//                 IsActive = true,
//                 IsDeleted = false
//             }
//             );


//             modelBuilder.Entity<Account>().HasData(new
//             {
//                 Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
//                 AccountNumber = AccountNumber.Create("1000000001"),
//                 AccountType = AccountType.Checking, // EF handles enum conversion
//                 AccountStatus = AccountStatus.Active,
//                 CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
//                 Currency = "NGN",
//                 DateOpened = new DateTime(2024, 9, 11, 0, 0, 0, DateTimeKind.Utc),
//                 DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
//                 DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
//                 IsActive = true,
//                 IsDeleted = false
//             }
//             );

//             // Then configure the owned types separately
//             /*modelBuilder.Entity<Account>().OwnsOne(a => a.Balance).HasData(
//                 new
//                 {
//                     AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
//                     Amount = 1500.00m,
//                     Currency = "NGN"
//                 }
//             );*/

//         }

//         public async Task SaveChangesWithOutboxAsync(CancellationToken cancellationToken = default)
//         {
//             // Convert domain events to outbox messages
//             var events = ChangeTracker.Entries<AggregateRoot<AccountId>>()
//                 .SelectMany(x => x.Entity.DomainEvents)
//                 .Select(domainEvent => new OutboxMessage
//                 {
//                     Id = Guid.NewGuid(),
//                     Type = domainEvent.GetType().Name,
//                     Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
//                     OccurredOn = domainEvent.OccurredOn
//                 })
//                 .ToList();

//             // Clear domain events from aggregates
//             ChangeTracker.Entries<AggregateRoot<AccountId>>()
//                 .ToList()
//                 .ForEach(entry => entry.Entity.ClearDomainEvents());

//             // Save changes (including outbox messages) in single transaction
//             await base.SaveChangesAsync(cancellationToken);

//             // Add outbox messages after saving to ensure they're included in transaction
//             if (events.Any())
//             {
//                 await OutboxMessages.AddRangeAsync(events, cancellationToken);
//                 await base.SaveChangesAsync(cancellationToken);
//             }
//         }

//     }
// }

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
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        public DbSet<DomainEvent> DomainEvents { get; set; }

        public DbSet<Transfer> Transfers => Set<Transfer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            base.OnModelCreating(modelBuilder);

            // modelBuilder.Ignore<DomainEvent>();
            // modelBuilder.Ignore<IDomainEvent>();
            // modelBuilder.Ignore<Hold>();

            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.Ignore<IDomainEvent>();
            modelBuilder.Ignore<AccountId>();
            modelBuilder.Ignore<ContactInfo>();
            modelBuilder.Ignore<Hold>();

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id)
                    .HasConversion(customerId => customerId.Value,
                                value => CustomerId.Create(value));

                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);

                // Customer has many Accounts
                entity.HasMany(c => c.Accounts)
                    .WithOne(a => a.Customer)
                    .HasForeignKey(a => a.CustomerId);

                entity.OwnsOne(c => c.ContactInfo, contact =>
                {
                    contact.Property(p => p.Email)
                        .HasColumnName("Email")
                        .HasMaxLength(255)
                        .IsRequired();
                    contact.Property(p => p.PhoneNumber)
                        .HasColumnName("PhoneNumber")
                        .HasMaxLength(20)
                        .IsRequired();

                    contact.OwnsOne(p => p.Address, address =>
                    {
                        address.Property(a => a.Street)
                            .HasColumnName("Address_Street")
                            .HasMaxLength(200);
                        address.Property(a => a.City)
                            .HasColumnName("Address_City")
                            .HasMaxLength(100);
                        address.Property(a => a.State)
                            .HasColumnName("Address_State")
                            .HasMaxLength(100);
                        address.Property(a => a.ZipCode)
                            .HasColumnName("Address_ZipCode")
                            .HasMaxLength(20);
                        address.Property(a => a.Country)
                            .HasColumnName("Address_Country")
                            .HasMaxLength(100);
                    });
                });
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

                // Configure CurrentBalance with unique column names
                entity.OwnsOne(a => a.CurrentBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("CurrentBalance_Amount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("CurrentBalance_Currency")
                        .HasMaxLength(3)
                        .HasDefaultValue("NGN");
                });

                // Configure AvailableBalance with unique column names
                entity.OwnsOne(a => a.AvailableBalance, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("AvailableBalance_Amount")
                        .HasPrecision(18, 2);
                    money.Property(m => m.Currency)
                        .HasColumnName("AvailableBalance_Currency")
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

                // Account concurrency implementation
                entity.Property(a => a.RowVersion)
                    .IsRowVersion();
                // .IsConcurrencyToken();
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

                entity.Property(t => t.RelatedAccountId)
                    .HasConversion(
                        id => id != null ? id.Value : (Guid?)null,
                        value => value.HasValue ? AccountId.Create(value.Value) : null)
                    .HasColumnName("RelatedAccountId")
                    .IsRequired(false);

                entity.HasOne(t => t.RelatedAccount)
                    .WithMany()
                    .HasForeignKey(nameof(Transaction.RelatedAccountId))
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Configure Transaction Amount as required owned type
                entity.OwnsOne(t => t.Amount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Transaction_Amount")
                        .HasPrecision(18, 2)
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("Transaction_Currency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                // Ensure Amount navigation is required
                entity.Navigation(t => t.Amount).IsRequired();

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


            modelBuilder.Entity<Transfer>(entity =>
            {
                entity.HasKey(t => t.Id);

                // Configure TransferId as value object
                entity.Property(t => t.Id)
                    .HasConversion(
                        transferId => transferId.Value,
                        value => TransferId.Create(value));

                // Configure FromAccountId
                entity.Property(t => t.FromAccountId)
                    .HasConversion(
                        id => id.Value,
                        value => AccountId.Create(value))
                    .IsRequired();

                // Configure ToAccountId
                entity.Property(t => t.ToAccountId)
                    .HasConversion(
                        id => id.Value,
                        value => AccountId.Create(value))
                    .IsRequired();

                // Configure Money as owned type for Amount
                entity.OwnsOne(t => t.Amount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Transfer_Amount")
                        .HasPrecision(18, 2)
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("Transfer_Currency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                // Configure Status enum
                entity.Property(t => t.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                // Configure other properties
                entity.Property(t => t.Reference)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.Property(t => t.InitiatedAt)
                    .IsRequired();

                entity.Property(t => t.ScheduledAt)
                    .IsRequired(false);

                entity.Property(t => t.CompletedAt)
                    .IsRequired(false);

                // Configure relationships with Account entity
                // FromAccount relationship
                entity.HasOne(t => t.FromAccount)
                    .WithMany() // Account doesn't have a collection of Transfers
                    .HasForeignKey(t => t.FromAccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // ToAccount relationship
                entity.HasOne(t => t.ToAccount)
                    .WithMany() // Account doesn't have a collection of Transfers
                    .HasForeignKey(t => t.ToAccountId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Indexes for better query performance
                entity.HasIndex(t => t.Reference)
                    .IsUnique();

                entity.HasIndex(t => t.FromAccountId);
                entity.HasIndex(t => t.ToAccountId);
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.ScheduledAt);
            });

            // Global query filter - Automatically Exclude Deleted Records
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(t => !t.Account.IsDeleted);

            // Seed the DB - Customers
            modelBuilder.Entity<Customer>().HasData(
                new
                {
                    Id = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    FirstName = "Alice",
                    LastName = "Johnson",
                    BVN = "20000000009",
                    CreditScore = 40,
                    DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false
                },
                new
                {
                    Id = CustomerId.Create(Guid.Parse("b2c3d4e5-2345-6789-abcd-234567890bcd")),
                    FirstName = "Bob",
                    LastName = "Smith",
                    BVN = "20000000010",
                    CreditScore = 55,
                    DateOfBirth = new DateTime(1988, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2024, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false
                }
            );

            // Seed ContactInfo for Customers
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.ContactInfo)
                .HasData(
                    new
                    {
                        CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                        Email = "alice.johnson@email.com",
                        PhoneNumber = "555-0101"
                    },
                    new
                    {
                        CustomerId = CustomerId.Create(Guid.Parse("b2c3d4e5-2345-6789-abcd-234567890bcd")),
                        Email = "bob.smith@email.com",
                        PhoneNumber = "555-0202"
                    }
                );

            // Seed Address for ContactInfo
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.ContactInfo)
                .OwnsOne(ci => ci.Address)
                .HasData(
                    new
                    {
                        ContactInfoCustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                        Street = "123 Main St",
                        City = "Lagos",
                        State = "Lagos",
                        ZipCode = "100001",
                        Country = "Nigeria"
                    },
                    new
                    {
                        ContactInfoCustomerId = CustomerId.Create(Guid.Parse("b2c3d4e5-2345-6789-abcd-234567890bcd")),
                        Street = "456 Victoria Island",
                        City = "Lagos",
                        State = "Lagos",
                        ZipCode = "101001",
                        Country = "Nigeria"
                    }
                );

            // Seed Accounts
            modelBuilder.Entity<Account>().HasData(
                new
                {
                    Id = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                    AccountNumber = AccountNumber.Create("1000000001"),
                    AccountType = AccountType.Checking,
                    AccountStatus = AccountStatus.Active,
                    CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
                    Currency = "NGN",
                    DateOpened = new DateTime(2024, 9, 11, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2024, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false,
                    RowVersion = new byte[] { 1, 0, 0, 0 }
                },
                new
                {
                    Id = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")),
                    AccountNumber = AccountNumber.Create("1000000002"),
                    AccountType = AccountType.Savings,
                    AccountStatus = AccountStatus.Active,
                    CustomerId = CustomerId.Create(Guid.Parse("b2c3d4e5-2345-6789-abcd-234567890bcd")),
                    Currency = "NGN",
                    DateOpened = new DateTime(2024, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                    DateCreated = new DateTime(2024, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                    DateUpdated = new DateTime(2024, 10, 5, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    IsDeleted = false,
                    RowVersion = new byte[] { 1, 0, 0, 1 }
                }
            );

            // Seed CurrentBalance for Accounts
            modelBuilder.Entity<Account>()
                .OwnsOne(a => a.CurrentBalance)
                .HasData(
                    new
                    {
                        AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                        Amount = 1500.00m,
                        Currency = "NGN"
                    },
                    new
                    {
                        AccountId = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")),
                        Amount = 5000.00m,
                        Currency = "NGN"
                    }
                );

            // Seed AvailableBalance for Accounts
            modelBuilder.Entity<Account>()
                .OwnsOne(a => a.AvailableBalance)
                .HasData(
                    new
                    {
                        AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                        Amount = 1500.00m,
                        Currency = "NGN"
                    },
                    new
                    {
                        AccountId = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")),
                        Amount = 5000.00m,
                        Currency = "NGN"
                    }
                );

        modelBuilder.Entity<Transfer>().HasData(
            // Completed immediate transfer from Alice to Bob
            new
            {
                Id = TransferId.Create(Guid.Parse("e5f6a7b8-5678-9012-0001-567890123001")),
                FromAccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")), // Alice's account
                ToAccountId = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")), // Bob's account
                Reference = "TRF-20241001-001",
                Description = "Payment for services",
                Status = TransferStatus.Completed,
                InitiatedAt = new DateTime(2024, 10, 1, 10, 30, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2024, 10, 1, 10, 30, 5, DateTimeKind.Utc),
                ScheduledAt = (DateTime?)null,
                DateCreated = new DateTime(2024, 10, 1, 10, 30, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2024, 10, 1, 10, 30, 5, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false
            },
            // Pending scheduled transfer from Bob to Alice
            new
            {
                Id = TransferId.Create(Guid.Parse("f6a7b8c9-6789-0123-0002-678901234002")),
                FromAccountId = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")), // Bob's account
                ToAccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")), // Alice's account
                Reference = "TRF-20241110-002",
                Description = "Scheduled monthly payment",
                Status = TransferStatus.Pending,
                InitiatedAt = new DateTime(2024, 11, 10, 8, 0, 0, DateTimeKind.Utc),
                CompletedAt = (DateTime?)null,
                ScheduledAt = new DateTime(2024, 11, 15, 9, 0, 0, DateTimeKind.Utc),
                DateCreated = new DateTime(2024, 11, 10, 8, 0, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2024, 11, 10, 8, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false
            },
            // Failed transfer from Alice to Bob
            new
            {
                Id = TransferId.Create(Guid.Parse("a7b8c9d0-7890-1234-0003-789012345003")),
                FromAccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")), // Alice's account
                ToAccountId = AccountId.Create(Guid.Parse("d4e5f6a7-4567-8901-def2-456789012def")), // Bob's account
                Reference = "TRF-20241005-003",
                Description = "Purchase payment - Failed: Insufficient funds",
                Status = TransferStatus.Failed,
                InitiatedAt = new DateTime(2024, 10, 5, 14, 20, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2024, 10, 5, 14, 20, 3, DateTimeKind.Utc),
                ScheduledAt = (DateTime?)null,
                DateCreated = new DateTime(2024, 10, 5, 14, 20, 0, DateTimeKind.Utc),
                DateUpdated = new DateTime(2024, 10, 5, 14, 20, 3, DateTimeKind.Utc),
                IsActive = true,
                IsDeleted = false
            }
        );

        // Seed Transfer Amounts
        modelBuilder.Entity<Transfer>()
            .OwnsOne(t => t.Amount)
            .HasData(
                new
                {
                    TransferId = TransferId.Create(Guid.Parse("e5f6a7b8-5678-9012-0001-567890123001")),
                    Amount = 500.00m,
                    Currency = "NGN"
                },
                new
                {
                    TransferId = TransferId.Create(Guid.Parse("f6a7b8c9-6789-0123-0002-678901234002")),
                    Amount = 1000.00m,
                    Currency = "NGN"
                },
                new
                {
                    TransferId = TransferId.Create(Guid.Parse("a7b8c9d0-7890-1234-0003-789012345003")),
                    Amount = 2500.00m,
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