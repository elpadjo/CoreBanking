using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.DeleteUser;

public record DeleteUserCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
    public string DeletedBy { get; init; } = string.Empty;
}
