using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Commands.DeactivateUser;

public record DeactivateUserCommand : ICommand
{
    public UserId UserId { get; init; } = UserId.Create();
    public string Reason { get; init; } = "Administrative action";
}
