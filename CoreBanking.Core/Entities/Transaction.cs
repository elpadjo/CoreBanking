using CoreBanking.Core.Common;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities
{
    public class Transaction : Entity<TransactionId>, ISoftDelete
    {
        public TransactionId Id { get; private set; }
        public AccountId AccountId { get; private set; }

        // For transfers - links the two related transactions
        public AccountId? RelatedAccountId { get; private set; } // Nullable for non-transfer transactions

        // Unique identifier for the transaction event (useful for linking transfer pairs)
        public string TransactionReference { get; private set; }

        public Account Account { get; private set; }
        public Account RelatedAccount { get; private set; }
        public TransactionType Type { get; private set; }
        public Money Amount { get; private set; } // Can be positive or negative

        public decimal RunningBalance { get; private set; } // Balance AFTER this transaction (private setter)
        public string Description { get; private set; }
        public string Reference { get; private set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        private Transaction() { } // For EF Core

        // Main constructor for regular transactions
        public Transaction(AccountId accountId, TransactionType type, Money amount, string description, string reference = null)
        {
            Id = TransactionId.Create();
            AccountId = accountId;
            Type = type;
            Amount = amount ?? throw new ArgumentNullException(nameof(amount), "Transaction amount cannot be null");
            Description = description ?? throw new ArgumentNullException(nameof(description));
            TransactionReference = GenerateTransactionReference();
            Reference = reference ?? string.Empty;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;

            ValidateTransaction();
        }

        // Constructor for transfers (with related account)
        public Transaction(AccountId accountId, TransactionType type, Money amount, string description,
                         AccountId relatedAccountId, string transactionReference, string reference = null)
            : this(accountId, type, amount, description, reference)
        {
            RelatedAccountId = relatedAccountId;
            TransactionReference = transactionReference; // Use provided reference for transfer pairing
        }

        // Constructor with running balance (for existing transactions)
        public Transaction(AccountId accountId, TransactionType type, Money amount, string description,
                         decimal runningBalance, string reference = null)
            : this(accountId, type, amount, description, reference)
        {
            RunningBalance = runningBalance;
        }

        private void ValidateTransaction()
        {
            if (Amount.Amount == 0)
                throw new ArgumentException("Transaction amount cannot be zero", nameof(Amount));

            // Validate amount sign based on transaction type
            switch (Type)
            {
                case TransactionType.Deposit:
                case TransactionType.TransferIn:
                    if (Amount.Amount <= 0)
                        throw new ArgumentException("Deposit and transfer-in amounts must be positive", nameof(Amount));
                    break;

                // case TransactionType.Withdrawal:
                // case TransactionType.TransferOut:
                // case TransactionType.Fee:
                //     if (Amount.Amount >= 0)
                //         throw new ArgumentException("Withdrawal, transfer-out and fee amounts must be negative", nameof(Amount));
                //     break;

                case TransactionType.Interest:
                    if (Amount.Amount <= 0)
                        throw new ArgumentException("Interest amounts must be positive", nameof(Amount));
                    break;
            }
        }

        private string GenerateTransactionReference()
        {
            return $"{DateCreated:yyyyMMddHHmmss}-{Id.ToString().Substring(0, 8)}";
        }

        // Business methods
        public void UpdateRunningBalance(decimal newRunningBalance)
        {
            RunningBalance = newRunningBalance;
            DateUpdated = DateTime.UtcNow;
        }

        public void UpdateDescription(string newDescription)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                throw new ArgumentException("Description cannot be empty", nameof(newDescription));

            Description = newDescription;
            DateUpdated = DateTime.UtcNow;
        }

        public void MarkAsTransfer(AccountId relatedAccountId, string transactionReference)
        {
            if (Type != TransactionType.TransferOut && Type != TransactionType.TransferIn)
                throw new InvalidOperationException("Can only mark transfer transactions as transfers");

            RelatedAccountId = relatedAccountId;
            TransactionReference = transactionReference;
            DateUpdated = DateTime.UtcNow;
        }

        public void SoftDelete(string deletedBy, string reason = "Correction")
        {
            if (IsDeleted)
                throw new InvalidOperationException("Transaction is already deleted");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            DateUpdated = DateTime.UtcNow;

            // Could raise domain event here if needed
            // AddDomainEvent(new TransactionDeletedEvent(Id, deletedBy, reason));
        }

        public void Restore()
        {
            if (!IsDeleted)
                throw new InvalidOperationException("Transaction is not deleted");

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            DateUpdated = DateTime.UtcNow;
        }

        // Helper properties
        public bool IsTransfer => RelatedAccountId != null;

        public bool IsCredit => Type == TransactionType.Deposit ||
                               Type == TransactionType.TransferIn ||
                               Type == TransactionType.Interest;

        public bool IsDebit => Type == TransactionType.Withdrawal ||
                              Type == TransactionType.TransferOut ||
                              Type == TransactionType.Fee;

        // Static factory methods for common transaction types
        public static Transaction CreateDeposit(AccountId accountId, Money amount, string description = "Deposit")
        {
            return new Transaction(accountId, TransactionType.Deposit, amount, description);
        }

        public static Transaction CreateWithdrawal(AccountId accountId, Money amount, string description = "Withdrawal")
        {
            return new Transaction(accountId, TransactionType.Withdrawal, new Money(-amount.Amount), description);
        }

        public static Transaction CreateTransferOut(AccountId accountId, Money amount, AccountId destinationAccountId,
                                                   string transactionReference, string description = "Transfer")
        {
            return new Transaction(accountId, TransactionType.TransferOut, new Money(-amount.Amount),
                                 description, destinationAccountId, transactionReference);
        }

        public static Transaction CreateTransferIn(AccountId accountId, Money amount, AccountId sourceAccountId,
                                                  string transactionReference, string description = "Transfer")
        {
            return new Transaction(accountId, TransactionType.TransferIn, amount,
                                 description, sourceAccountId, transactionReference);
        }

        public static Transaction CreateFee(AccountId accountId, Money amount, string description = "Service Fee")
        {
            return new Transaction(accountId, TransactionType.Fee, new Money(-amount.Amount), description);
        }

        public static Transaction CreateInterest(AccountId accountId, Money amount, string description = "Interest")
        {
            return new Transaction(accountId, TransactionType.Interest, amount, description);
        }
    }
}