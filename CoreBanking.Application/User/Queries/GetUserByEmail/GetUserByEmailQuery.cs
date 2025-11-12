using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;

namespace CoreBanking.Application.User.Queries.GetUserByEmail;

public record GetUserByEmailQuery : IQuery<UserDetailsDto>
{
    public required string Email { get; init; }
}
