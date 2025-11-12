using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;

namespace CoreBanking.Application.User.Queries.GetUserByUsername;

public record GetUserByUsernameQuery : IQuery<UserDetailsDto>
{
    public required string Username { get; init; }
}
