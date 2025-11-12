using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;

namespace CoreBanking.Application.User.Queries.GetActiveUsers;

public record GetActiveUsersQuery : IQuery<List<UserDto>>;
