//using CoreBanking.Application.Common.Models;
//using CoreBanking.Application.User.Queries.Common;
//using CoreBanking.Core.Interfaces;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace CoreBanking.Application.User.Queries.GetActiveUsers;

//public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, Result<List<UserDto>>>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly ILogger<GetActiveUsersQueryHandler> _logger;

//    public GetActiveUsersQueryHandler(
//        IUserRepository userRepository,
//        ILogger<GetActiveUsersQueryHandler> logger)
//    {
//        _userRepository = userRepository;
//        _logger = logger;
//    }

//    public async Task<Result<List<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Getting all active users");

//        var users = await _userRepository.GetAllAsync(cancellationToken);

//        var activeUserDtos = users
//            .Where(u => u != null && u.IsActive)
//            .Select(u => new UserDto
//            {
//                UserId = u!.Id,
//                Username = u.Username,
//                Email = u.Email,
//                Role = u.Role.ToString(),
//                IsActive = u.IsActive,
//                LastLoginAt = u.LastLoginAt,
//                FailedLoginAttempts = u.FailedLoginAttempts,
//                IsLocked = u.IsLocked(),
//                LockedUntil = u.LockedUntil,
//                DateCreated = u.DateCreated
//            })
//            .ToList();

//        _logger.LogInformation("Retrieved {Count} active users", activeUserDtos.Count);

//        return Result<List<UserDto>>.Success(activeUserDtos);
//    }
//}
