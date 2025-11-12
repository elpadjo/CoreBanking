using CoreBanking.Core.Common;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Entities
{
    public class User : Entity<UserId>, ISoftDelete
    {
        public string Username { get; private set; }

        public string PasswordHash { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }
        public string Email { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockedUntil { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public string? DeletedBy { get; private set; }
        private User() { } // For EF Core

        public User(string username, string passwordHash, UserRole role, string email)
        {
            Id = UserId.Create();
            Username = ValidateUsername(username);
            PasswordHash = ValidatePasswordHash(passwordHash);
            Role = role;
            IsActive = true;
            FailedLoginAttempts = 0;
            DateCreated = DateTime.UtcNow;
            DateUpdated = DateTime.UtcNow;
            Email = email;
        }

        // Business methods
        public void UpdatePassword(string newPasswordHash)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update password for inactive user");

            PasswordHash = ValidatePasswordHash(newPasswordHash);
            DateUpdated = DateTime.UtcNow;
        }

        public void UpdateRole(UserRole newRole, string changedBy)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update role for inactive user");

            if (Role == newRole)
                return;

            Role = newRole;
            DateUpdated = DateTime.UtcNow;

            // Could raise domain event: UserRoleChangedEvent
        }

        public void UpdateUsername(string newUsername)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update username for inactive user");

            Username = ValidateUsername(newUsername);
            DateUpdated = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
                throw new InvalidOperationException("User is already active");

            IsActive = true;
            FailedLoginAttempts = 0; // Reset failed attempts
            LockedUntil = null; // Clear any lock
            DateUpdated = DateTime.UtcNow;
        }

        public void Deactivate(string reason = "Administrative action")
        {
            if (!IsActive)
                throw new InvalidOperationException("User is already inactive");

            IsActive = false;
            DateUpdated = DateTime.UtcNow;

            // Could raise domain event: UserDeactivatedEvent
        }

        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0; // Reset on successful login
            LockedUntil = null; // Clear any lock
            DateUpdated = DateTime.UtcNow;
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;

            // Auto-lock after 5 failed attempts for 30 minutes
            if (FailedLoginAttempts >= 5)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(30);
            }

            DateUpdated = DateTime.UtcNow;
        }

        public void Unlock()
        {
            FailedLoginAttempts = 0;
            LockedUntil = null;
            DateUpdated = DateTime.UtcNow;
        }

        public bool IsLocked()
        {
            return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
        }

        public bool CanLogin()
        {
            return IsActive && !IsLocked();
        }

        public bool HasPermission(string permission)
        {
            // Simple permission check based on role
            // You could expand this with a proper permission system
            return Role switch
            {
                UserRole.Admin => true, // Admins have all permissions
                UserRole.Manager => permission != "system_admin", // Managers have most permissions
                UserRole.Teller => permission is "process_transactions" or "view_customers",
                _ => false
            };
        }

        public void Delete(string deletedBy)
        {
            if (IsDeleted)
                throw new InvalidOperationException("User is already deleted");

            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
            IsActive = false; // Also deactivate the user
            DateUpdated = DateTime.UtcNow;
        }

        public void Restore()
        {
            if (!IsDeleted)
                throw new InvalidOperationException("User is not deleted");

            IsDeleted = false;
            DeletedAt = null;
            DeletedBy = null;
            DateUpdated = DateTime.UtcNow;
            // Note: Does not automatically reactivate - must be done separately via Activate()
        }

        // Validation methods
        private static string ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            if (username.Length < 3 || username.Length > 50)
                throw new ArgumentException("Username must be between 3 and 50 characters", nameof(username));

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                throw new ArgumentException("Username can only contain letters, numbers, and underscores", nameof(username));

            return username.Trim();
        }

        private static string ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

            if (passwordHash.Length < 60) // BCrypt hashes are typically 60 chars
                throw new ArgumentException("Invalid password hash format", nameof(passwordHash));



            return passwordHash;
        }

        // Static factory method
        public static User Create(string username, string passwordHash, UserRole role, string email)
        {
            return new User(username, passwordHash, role, email);
        }

        public static User CreateTeller(string username, string passwordHash, string email)
        {
            return new User(username, passwordHash, UserRole.Teller, email);
        }

        public static User CreateManager(string username, string passwordHash, string email)
        {
            return new User(username, passwordHash, UserRole.Manager, email);
        }

        public static User CreateAdmin(string username, string passwordHash, string email)
        {
            return new User(username, passwordHash, UserRole.Admin, email);
        }
    }
}