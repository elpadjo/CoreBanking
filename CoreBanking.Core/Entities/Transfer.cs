using CoreBanking.Core.Common;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities
{
    public class Transfer : Entity<TransferId>, ISoftDelete
    {
        public AccountId FromAccountId { get; private set; }
        public AccountId ToAccountId { get; private set; }
        public Money Amount { get; private set; }
        public TransferStatus Status { get; private set; } = TransferStatus.Pending;
        public DateTime? ScheduledAt { get; private set; }
        public DateTime InitiatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        // Navigation properties
        public Account FromAccount { get; private set; }
        public Account ToAccount { get; private set; }

        private Transfer() { } // For EF Core

        // Main constructor
        public Transfer(AccountId fromAccountId, AccountId toAccountId, Money amount,
                       string reference, string description = null, DateTime? scheduledAt = null)
        {
            Id = TransferId.Create();
            FromAccountId = fromAccountId ?? throw new ArgumentNullException(nameof(fromAccountId));
            ToAccountId = toAccountId ?? throw new ArgumentNullException(nameof(toAccountId));
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            Description = description ?? $"Transfer from {fromAccountId} to {toAccountId}";
            ScheduledAt = scheduledAt;
            InitiatedAt = DateTime.UtcNow;
            Status = TransferStatus.Pending;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;

            Validate();
        }

        private void Validate()
        {
            if (Amount.Amount <= 0)
                throw new ArgumentException("Transfer amount must be positive", nameof(Amount));

            if (FromAccountId == ToAccountId)
                throw new ArgumentException("Cannot transfer to the same account", nameof(ToAccountId));

            if (ScheduledAt.HasValue && ScheduledAt.Value <= DateTime.UtcNow)
                throw new ArgumentException("Scheduled transfer must be in the future", nameof(ScheduledAt));
        }

        // Business methods
        public void MarkAsCompleted()
        {
            if (Status != TransferStatus.Pending)
                throw new InvalidOperationException($"Cannot complete transfer in {Status} status");

            Status = TransferStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;
        }

        public void MarkAsFailed(string reason = "Transfer failed")
        {
            if (Status != TransferStatus.Pending)
                throw new InvalidOperationException($"Cannot fail transfer in {Status} status");

            Status = TransferStatus.Failed;
            CompletedAt = DateTime.UtcNow;
            Description += $" - Failed: {reason}";
            DateUpdated = DateTime.UtcNow;
        }

        public void MarkAsReversed(string reason = "Transfer reversed")
        {
            if (Status != TransferStatus.Completed)
                throw new InvalidOperationException("Can only reverse completed transfers");

            Status = TransferStatus.Reversed;
            Description += $" - Reversed: {reason}";
            DateUpdated = DateTime.UtcNow;
        }

        public void UpdateDescription(string newDescription)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                throw new ArgumentException("Description cannot be empty", nameof(newDescription));

            Description = newDescription;
            DateUpdated = DateTime.UtcNow;
        }

        public void Reschedule(DateTime newScheduledTime)
        {
            if (Status != TransferStatus.Pending)
                throw new InvalidOperationException("Can only reschedule pending transfers");

            if (newScheduledTime <= DateTime.UtcNow)
                throw new ArgumentException("New scheduled time must be in the future", nameof(newScheduledTime));

            ScheduledAt = newScheduledTime;
            DateUpdated = DateTime.UtcNow;
        }

        public bool CanExecute()
        {
            return Status == TransferStatus.Pending &&
                   (!ScheduledAt.HasValue || ScheduledAt.Value <= DateTime.UtcNow);
        }

        public bool IsOverdue()
        {
            return Status == TransferStatus.Pending &&
                   ScheduledAt.HasValue &&
                   ScheduledAt.Value < DateTime.UtcNow.AddHours(-1); // 1 hour grace period
        }

        public void SoftDelete(string deletedBy, string reason = "Transfer cancelled")
        {
            if (IsDeleted)
                throw new InvalidOperationException("Transfer is already deleted");

            if (Status == TransferStatus.Completed)
                throw new InvalidOperationException("Cannot delete completed transfers");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;

            // If pending, mark as failed when deleting
            if (Status == TransferStatus.Pending)
            {
                Status = TransferStatus.Failed;
                Description += $" - Cancelled: {reason}";
            }

            DateUpdated = DateTime.UtcNow;
        }

        public void Restore()
        {
            if (!IsDeleted)
                throw new InvalidOperationException("Transfer is not deleted");

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            DateUpdated = DateTime.UtcNow;
        }

        // Helper properties
        public bool IsImmediate => !ScheduledAt.HasValue;
        public bool IsScheduled => ScheduledAt.HasValue;
        public TimeSpan? TimeUntilExecution => ScheduledAt.HasValue ? ScheduledAt.Value - DateTime.UtcNow : null;

        // Static factory methods
        public static Transfer CreateImmediate(AccountId fromAccountId, AccountId toAccountId,
                                             Money amount, string reference, string description = null)
        {
            return new Transfer(fromAccountId, toAccountId, amount, reference, description);
        }

        public static Transfer CreateScheduled(AccountId fromAccountId, AccountId toAccountId,
                                             Money amount, string reference, DateTime scheduledAt,
                                             string description = null)
        {
            return new Transfer(fromAccountId, toAccountId, amount, reference, description, scheduledAt);
        }

        public static Transfer CreateInternalTransfer(AccountId fromAccountId, AccountId toAccountId,
                                                    Money amount, string description = "Internal transfer")
        {
            var reference = $"INT-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            return new Transfer(fromAccountId, toAccountId, amount, reference, description);
        }
    }
}