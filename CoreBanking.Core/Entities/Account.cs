// CoreBanking.Core/Entities/Account.cs
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;
using System.Transactions;

namespace CoreBanking.Core.Entities
{
    public class Account
    {
        public Guid AccountId { get; private set; }
        public AccountNumber AccountNumber { get; private set; }
        public AccountType AccountType { get; private set; }
        public Money Balance { get; private set; }
        public Guid CustomerId { get; private set; }
        public DateTime DateOpened { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation properties - private to enforce aggregate boundary
        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        // Required for EF Core
        private Account() { }

        public Account(AccountNumber accountNumber, AccountType accountType, Guid customerId)
        {
            AccountId = Guid.NewGuid();
            AccountNumber = accountNumber;
            AccountType = accountType;
            CustomerId = customerId;
            Balance = new Money(0);
            DateOpened = DateTime.UtcNow;
            IsActive = true;
        }

        // Core banking operations - these are the aggregate's public API
        public Transaction Deposit(Money amount, string description = "Deposit")
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot deposit to inactive account");

            if (amount.Amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            Balance += amount;

            var transaction = new Transaction(
                accountId: AccountId,
                type: TransactionType.Deposit,
                amount: amount,
                description: description
            );

            _transactions.Add(transaction);
            return transaction;
        }

        public Transaction Withdraw(Money amount, string description = "Withdrawal")
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot withdraw from inactive account");

            if (amount.Amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (Balance.Amount < amount.Amount)
                throw new InvalidOperationException("Insufficient funds");

            // Special business rule for Savings accounts
            if (AccountType == AccountType.Savings && _transactions.Count(t => t.Type == TransactionType.Withdrawal) >= 6)
                throw new InvalidOperationException("Savings account withdrawal limit reached");

            Balance -= amount;

            var transaction = new Transaction(
                accountId: AccountId,
                type: TransactionType.Withdrawal,
                amount: amount,
                description: description
            );

            _transactions.Add(transaction);
            return transaction;
        }

        public void CloseAccount()
        {
            if (Balance.Amount != 0)
                throw new InvalidOperationException("Cannot close account with non-zero balance");

            IsActive = false;
        }
    }
}