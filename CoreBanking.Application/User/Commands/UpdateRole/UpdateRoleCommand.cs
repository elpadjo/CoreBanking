using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.UpdateRole;

public record UpdateRoleCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
    public string NewRole { get; init; } = string.Empty;
    public string ChangedBy { get; init; } = string.Empty;
}
