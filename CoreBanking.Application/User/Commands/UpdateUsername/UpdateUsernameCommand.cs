using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.UpdateUsername;

public record UpdateUsernameCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
    public string NewUsername { get; init; } = string.Empty;
}
