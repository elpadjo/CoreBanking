using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;

namespace CoreBanking.Application.User.Queries.GetAllUsers;

public record GetAllUsersQuery : IQuery<List<UserDto>>;
