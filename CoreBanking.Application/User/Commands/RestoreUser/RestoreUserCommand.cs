using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.RestoreUser;

public record RestoreUserCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
}
