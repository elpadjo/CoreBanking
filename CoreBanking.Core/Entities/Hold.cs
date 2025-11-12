using CoreBanking.Core.Common;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities
{
    public class Hold : Entity<HoldId>, ISoftDelete
    {
        public AccountId AccountId { get; private set; }
        public Money Amount { get; private set; }
        public string Description { get; private set; }
        public DateTime PlacedAt { get; private set; }
        public DateTime ExpiresAt { get; private set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        // Navigation property
        public Account Account { get; private set; }

        private Hold() { } // For EF Core

        public Hold(AccountId accountId, Money amount, string description, DateTime expiresAt)
        {
            Id = HoldId.Create();
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            Amount = amount ?? throw new ArgumentNullException(nameof(amount));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            PlacedAt = DateTime.UtcNow;
            ExpiresAt = expiresAt;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;

            Validate();
        }

        private void Validate()
        {
            if (Amount.Amount <= 0)
                throw new ArgumentException("Hold amount must be positive", nameof(Amount));

            if (ExpiresAt <= PlacedAt)
                throw new ArgumentException("Expiry date must be after placement date", nameof(ExpiresAt));

            if (string.IsNullOrWhiteSpace(Description))
                throw new ArgumentException("Description cannot be empty", nameof(Description));
        }

        // Business methods
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        public bool IsActive()
        {
            return !IsDeleted && !IsExpired();
        }

        public TimeSpan GetRemainingTime()
        {
            return ExpiresAt - DateTime.UtcNow;
        }

        public void Extend(TimeSpan extension)
        {
            if (IsDeleted)
                throw new InvalidOperationException("Cannot extend a deleted hold");

            if (IsExpired())
                throw new InvalidOperationException("Cannot extend an expired hold");

            ExpiresAt += extension;
            DateUpdated = DateTime.UtcNow;
        }

        public void UpdateDescription(string newDescription)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                throw new ArgumentException("Description cannot be empty", nameof(newDescription));

            Description = newDescription;
            DateUpdated = DateTime.UtcNow;
        }

        public void SoftDelete(string deletedBy, string reason = "Manual removal")
        {
            if (IsDeleted)
                throw new InvalidOperationException("Hold is already deleted");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            DateUpdated = DateTime.UtcNow;
        }

        public void Restore()
        {
            if (!IsDeleted)
                throw new InvalidOperationException("Hold is not deleted");

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            DateUpdated = DateTime.UtcNow;
        }

        // Static factory method
        public static Hold Create(AccountId accountId, Money amount, string description, TimeSpan duration)
        {
            var expiresAt = DateTime.UtcNow + duration;
            return new Hold(accountId, amount, description, expiresAt);
        }

        public static Hold CreateWithID(HoldId holdId, AccountId accountId, Money amount, string description, TimeSpan duration)
        {
            var expiresAt = DateTime.UtcNow + duration;
            return new Hold
            {
                Id = holdId,
                Amount = amount,
                Description = description,
                AccountId = accountId,
                ExpiresAt = expiresAt
            };
        }

        public static Hold CreateForAuthorization(AccountId accountId, Money amount, string merchantName)
        {
            // Standard authorization hold duration (e.g., for card transactions)
            var duration = TimeSpan.FromDays(2);
            var description = $"Authorization hold - {merchantName}";
            return Create(accountId, amount, description, duration);
        }

        public static Hold CreateForCheckDeposit(AccountId accountId, Money amount, string checkReference)
        {
            // Check hold duration (typically longer)
            var duration = TimeSpan.FromDays(5);
            var description = $"Check hold - {checkReference}";
            return Create(accountId, amount, description, duration);
        }

        public override string ToString()
        {
            return $"ID: {Id}, AccountId: {AccountId}, Money: {Amount.Amount}, Duration: {ExpiresAt}, Description: {Description})";
        }
    }
}