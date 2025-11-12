using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Commands.LoginUser;

namespace CoreBanking.Application.User.Commands.LoginUser;

public record LoginUserCommand : ICommand<LoginUserResponse>
{
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginUserResponse
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
