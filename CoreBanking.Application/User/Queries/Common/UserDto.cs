using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Queries.Common;

public record UserDto
{
    public UserId UserId { get; init; } = UserId.Create();
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public int FailedLoginAttempts { get; init; }
    public bool IsLocked { get; init; }
    public DateTime? LockedUntil { get; init; }
    public DateTime DateCreated { get; init; }
}
