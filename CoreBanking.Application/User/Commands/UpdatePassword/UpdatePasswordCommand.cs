using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.UpdatePassword;

public record UpdatePasswordCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
    public string NewPassword { get; init; } = string.Empty;
}
