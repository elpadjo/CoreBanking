using CoreBanking.Core.Common;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Core.Events;

namespace CoreBanking.Core.Entities
{
    public class Customer : AggregateRoot<CustomerId>, ISoftDelete
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string BVN { get; private set; }
        public int CreditScore { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public ContactInfo ContactInfo { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }

        // Computed property for age
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        // Navigation property for accounts
        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

        private Customer() { } // EF Core needs this

        public Customer(string firstName, string lastName, ContactInfo contactInfo, DateTime dateOfBirth, string bVN, int creditScore)
        {
            Id = CustomerId.Create();

            // Validations
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            FirstName = firstName;

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));
            LastName = lastName;

            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));

            if (string.IsNullOrWhiteSpace(bVN))
                throw new ArgumentException("BVN cannot be empty", nameof(bVN));
            if (bVN.Length != 11)
                throw new ArgumentException("BVN must be 11 digits", nameof(bVN));
            BVN = bVN;

            if (creditScore < 300 || creditScore > 850)
                throw new ArgumentException("Credit score must be between 300 and 850", nameof(creditScore));
            CreditScore = creditScore;

            var minimumAgeDate = DateTime.UtcNow.AddYears(-18);
            if (dateOfBirth > minimumAgeDate)
                throw new ArgumentException("Customer must be at least 18 years old", nameof(dateOfBirth));
            DateOfBirth = dateOfBirth;

            DateCreated = DateTime.UtcNow;
            UpdateTimestamp();
            IsActive = true;

            // Single meaningful event
            AddDomainEvent(new CustomerCreatedEvent(Id, firstName, lastName, contactInfo.Email, dateOfBirth, creditScore));
        }

        public void UpdateProfile(ContactInfo newContactInfo)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update inactive customer");

            var oldContactInfo = ContactInfo;
            ContactInfo = newContactInfo;
            UpdateTimestamp();

            // Single event for profile changes
            AddDomainEvent(new CustomerProfileUpdatedEvent(Id, oldContactInfo, newContactInfo));
        }

        public void UpdateCreditScore(int newCreditScore, string reason = "System update")
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update inactive customer");

            if (newCreditScore < 300 || newCreditScore > 850)
                throw new ArgumentException("Credit score must be between 300 and 850", nameof(newCreditScore));

            var oldCreditScore = CreditScore;
            CreditScore = newCreditScore;
            UpdateTimestamp();

            AddDomainEvent(new CustomerCreditScoreChangedEvent(Id, oldCreditScore, newCreditScore, reason));
        }

        public void Deactivate(string reason = "Customer request")
        {
            if (!IsActive)
                throw new InvalidOperationException("Customer is already inactive");

            if (_accounts.Any(a => a.CurrentBalance.Amount > 0))
                throw new InvalidOperationException("Cannot deactivate customer with account balance");

            var wasActive = IsActive;
            IsActive = false;
            UpdateTimestamp();

            AddDomainEvent(new CustomerStatusChangedEvent(Id, wasActive, IsActive, reason));
        }

        public void Reactivate(string reason = "Customer request")
        {
            if (IsActive)
                throw new InvalidOperationException("Customer is already active");

            if (IsDeleted)
                throw new InvalidOperationException("Cannot reactivate deleted customer");

            var wasActive = IsActive;
            IsActive = true;
            UpdateTimestamp();

            AddDomainEvent(new CustomerStatusChangedEvent(Id, wasActive, IsActive, reason));
        }

        public bool CanOpenAccount()
        {
            return IsActive &&
                   !IsDeleted &&
                   CreditScore >= 580 &&
                   Age >= 18;
        }

        public string GetFullName() => $"{FirstName} {LastName}";

        internal void AddAccount(Account account)
        {
            if (!CanOpenAccount())
                throw new InvalidOperationException("Customer cannot open new accounts");

            _accounts.Add(account);
            UpdateTimestamp();
            // No domain event - account creation handles its own events
        }

        public void SoftDelete(string deletedBy, string reason = "Customer request")
        {
            if (IsDeleted)
                throw new InvalidOperationException("Customer is already deleted");

            if (Accounts.Any(a => a.CurrentBalance.Amount > 0))
                throw new InvalidOperationException("Cannot delete customer with account balance");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            IsActive = false;
            UpdateTimestamp();

            AddDomainEvent(new CustomerDeletedEvent(Id, deletedBy, reason));
        }
    }
}