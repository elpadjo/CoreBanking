using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.ActivateUser;

public record ActivateUserCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
}
