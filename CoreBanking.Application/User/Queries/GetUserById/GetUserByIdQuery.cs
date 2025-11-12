using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.User.Queries.GetUserById;

public record GetUserByIdQuery : IQuery<UserDetailsDto>
{
    public required UserId UserId { get; init; }
}
