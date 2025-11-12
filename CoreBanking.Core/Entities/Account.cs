using CoreBanking.Core.Common;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Events;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities
{
    public class Account : AggregateRoot<AccountId>, ISoftDelete
    {
        public AccountNumber AccountNumber { get; private set; }
        public AccountType AccountType { get; private set; }
        // Use CurrentBalance when checking if money actually exists
        public Money CurrentBalance { get; private set; }
        // Use AvailableBalance when checking if money can be spent/withdrawn
        public Money AvailableBalance { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public Customer Customer { get; private set; }
        public DateTime DateOpened { get; private set; }
        public DateTime? DateClosed { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
        public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        // Navigation properties
        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        //private readonly List<Hold> _holds = new();
        //public IReadOnlyCollection<Hold> Holds => _holds.AsReadOnly();

        private Account() { } // EF Core needs this

        private Account(AccountNumber accountNumber, AccountType accountType, CustomerId customerId, Money initialBalance)
        {
            Id = AccountId.Create();
            AccountNumber = accountNumber;
            AccountType = accountType;
            CustomerId = customerId;
            CurrentBalance = initialBalance;
            AvailableBalance = initialBalance;
            DateOpened = DateTime.UtcNow;
            AccountStatus = AccountStatus.Active;
            DateCreated = DateTime.UtcNow;
            UpdateTimestamp();

            AddDomainEvent(new AccountCreatedEvent(Id, accountNumber, customerId, accountType, initialBalance));
        }

        public static Account Create(CustomerId customerId, AccountNumber accountNumber, AccountType accountType, Money initialBalance)
        {
            if (initialBalance.Amount < 0)
                throw new InvalidOperationException("Initial balance cannot be negative");

            if (initialBalance.Amount > 1000000)
                throw new InvalidOperationException("Initial deposit too large");

            return new Account(accountNumber, accountType, customerId, initialBalance);
        }

        public Transaction Deposit(Money amount, string description = "Deposit")
        {
            if (AccountStatus != AccountStatus.Active)
                throw new InvalidOperationException("Cannot deposit to inactive account");

            if (amount.Amount <= 0)
                throw new ArgumentException("Deposit amount must be positive");

            CurrentBalance += amount;
            AvailableBalance += amount; // Available immediately for cash deposits
            UpdateTimestamp();

            var transaction = new Transaction(
                accountId: Id,
                type: TransactionType.Deposit,
                amount: amount,
                description: description
            );

            _transactions.Add(transaction);

            AddDomainEvent(new AccountCreditedEvent(Id, amount, CurrentBalance, description, null));

            return transaction;
        }

        public Transaction Withdraw(Money amount, string description = "Withdrawal")
        {
            if (AccountStatus != AccountStatus.Active)
                throw new InvalidOperationException("Cannot withdraw from inactive account");

            if (amount.Amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive");

            if (AvailableBalance.Amount < amount.Amount)
            {
                AddDomainEvent(new InsufficientFundsEvent(AccountNumber, amount, AvailableBalance, "Withdrawal"));
                throw new InvalidOperationException("Insufficient available funds");
            }

            // Special business rule for Savings accounts (monthly withdrawal limit)
            if (AccountType == AccountType.Savings)
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var monthlyWithdrawals = _transactions.Count(t =>
                    t.Type == TransactionType.Withdrawal &&
                    t.DateCreated.Month == currentMonth &&
                    t.DateCreated.Year == currentYear);

                if (monthlyWithdrawals >= 6)
                    throw new InvalidOperationException("Savings account monthly withdrawal limit reached");
            }

            CurrentBalance -= amount;
            AvailableBalance -= amount;
            UpdateTimestamp();

            var transaction = new Transaction(
                accountId: Id,
                type: TransactionType.Withdrawal,
                amount: amount,
                description: description
            );

            _transactions.Add(transaction);

            AddDomainEvent(new AccountDebitedEvent(Id, amount, CurrentBalance, description, null));

            return transaction;
        }

        public Result Transfer(Money amount, Account destination, string reference, string description)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination), "Destination account cannot be null");

            if (amount.Amount <= 0)
                throw new InvalidOperationException("Transfer amount must be positive");

            if (Id == destination.Id)
                throw new InvalidOperationException("Cannot transfer to the same account");

            if (AccountStatus != AccountStatus.Active)
                throw new InvalidOperationException("Source account is not active");

            if (destination.AccountStatus != AccountStatus.Active)
                throw new InvalidOperationException("Destination account is not active");

            if (AvailableBalance.Amount < amount.Amount)
            {
                AddDomainEvent(new InsufficientFundsEvent(AccountNumber, amount, AvailableBalance, "Transfer"));
                return Result.Failure("Insufficient available funds for transfer");
            }

            // Savings account withdrawal limit check
            if (AccountType == AccountType.Savings)
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var monthlyWithdrawals = _transactions.Count(t =>
                    t.Type == TransactionType.Withdrawal &&
                    t.DateCreated.Month == currentMonth &&
                    t.DateCreated.Year == currentYear);

                if (monthlyWithdrawals >= 6)
                    return Result.Failure("Savings account monthly withdrawal limit reached");
            }

            // Execute the transfer as atomic operations
            var debitResult = Debit(amount, $"Transfer to {destination.AccountNumber}", reference);
            if (!debitResult.IsSuccess)
                return debitResult;

            var creditResult = destination.Credit(amount, $"Transfer from {AccountNumber}", reference);
            if (!creditResult.IsSuccess)
            {
                // Rollback the debit if credit fails
                Credit(amount, "Transfer rollback", reference);
                return Result.Failure("Transfer failed - destination account issue");
            }

            var transactionId = TransactionId.Create();
            AddDomainEvent(new MoneyTransferedEvent(transactionId, AccountNumber, destination.AccountNumber, amount, reference, DateCreated));

            return Result.Success();
        }

        public Result Debit(Money amount, string description, string reference)
        {
            if (IsDeleted)
                return Result.Failure("Cannot debit a deleted account");

            if (amount.Amount <= 0)
                return Result.Failure("Debit amount must be positive");

            if (AvailableBalance.Amount < amount.Amount)
            {
                AddDomainEvent(new InsufficientFundsEvent(AccountNumber, amount, AvailableBalance, "Debit"));
                return Result.Failure("Insufficient available funds");
            }

            CurrentBalance -= amount;
            AvailableBalance -= amount;
            UpdateTimestamp();

            var transaction = new Transaction(
                accountId: Id,
                type: TransactionType.Withdrawal,
                amount: amount,
                description: description,
                reference: reference
            );

            _transactions.Add(transaction);

            AddDomainEvent(new AccountDebitedEvent(Id, amount, CurrentBalance, description, reference));

            return Result.Success();
        }

        public Result Credit(Money amount, string description, string reference)
        {
            if (IsDeleted)
                return Result.Failure("Cannot credit a deleted account");

            if (amount.Amount <= 0)
                return Result.Failure("Credit amount must be positive");

            CurrentBalance += amount;
            AvailableBalance += amount;
            UpdateTimestamp();

            var transaction = new Transaction(
                accountId: Id,
                type: TransactionType.Deposit,
                amount: amount,
                description: description,
                reference: reference
            );

            _transactions.Add(transaction);

            AddDomainEvent(new AccountCreditedEvent(Id, amount, CurrentBalance, description, reference));

            return Result.Success();
        }

        //public void PlaceHold(Money amount, string description)
        //{
        //    if (AccountStatus != AccountStatus.Active)
        //        throw new InvalidOperationException("Cannot place hold on inactive account");

        //    if (AvailableBalance.Amount < amount.Amount)
        //        throw new InvalidOperationException("Cannot place hold - insufficient available funds");

        //    var hold = new Hold(Id, amount, description, DateTime.UtcNow);
        //    _holds.Add(hold);
        //    AvailableBalance = CurrentBalance - GetTotalHolds();
        //    UpdateTimestamp();

        //    AddDomainEvent(new HoldPlacedEvent(Id, amount, description, AvailableBalance));
        //}

        //public void RemoveHold(HoldId holdId)
        //{
        //    var hold = _holds.FirstOrDefault(h => h.Id == holdId);
        //    if (hold != null)
        //    {
        //        _holds.Remove(hold);
        //        AvailableBalance = CurrentBalance - GetTotalHolds();
        //        UpdateTimestamp();

        //        AddDomainEvent(new HoldRemovedEvent(Id, hold.Amount, AvailableBalance));
        //    }
        //}

        //private Money GetTotalHolds() => new Money(_holds.Sum(h => h.Amount.Amount));

        public void CloseAccount(string reason = "Customer request")
        {
            if (CurrentBalance.Amount != 0)
                throw new InvalidOperationException("Cannot close account with non-zero balance");

            var oldStatus = AccountStatus;
            AccountStatus = AccountStatus.Closed;
            DateClosed = DateTime.UtcNow;
            UpdateTimestamp();

            AddDomainEvent(new AccountStatusChangedEvent(Id, oldStatus, AccountStatus, reason));
        }

        public void ReopenAccount(string reason = "Customer request")
        {
            if (AccountStatus != AccountStatus.Closed)
                throw new InvalidOperationException("Account is not closed");

            var oldStatus = AccountStatus;
            AccountStatus = AccountStatus.Active;
            DateClosed = null;
            UpdateTimestamp();

            AddDomainEvent(new AccountStatusChangedEvent(Id, oldStatus, AccountStatus, reason));
        }

        public void SoftDelete(string deletedBy, string reason = "Customer request")
        {
            if (IsDeleted)
                throw new InvalidOperationException("Account is already deleted");

            if (CurrentBalance.Amount != 0)
                throw new InvalidOperationException("Cannot delete account with non-zero balance");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            AccountStatus = AccountStatus.Closed;
            UpdateTimestamp();

            AddDomainEvent(new AccountDeletedEvent(Id, deletedBy, reason));
        }

        public bool CanWithdraw(Money amount)
        {
            return AccountStatus == AccountStatus.Active &&
                   !IsDeleted &&
                   AvailableBalance.Amount >= amount.Amount &&
                   (AccountType != AccountType.Savings || GetMonthlyWithdrawalCount() < 6);
        }

        private int GetMonthlyWithdrawalCount()
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            return _transactions.Count(t =>
                t.Type == TransactionType.Withdrawal &&
                t.DateCreated.Month == currentMonth &&
                t.DateCreated.Year == currentYear);
        }
    }
}