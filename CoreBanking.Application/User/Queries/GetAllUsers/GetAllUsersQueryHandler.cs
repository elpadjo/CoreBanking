//using CoreBanking.Application.Common.Models;
//using CoreBanking.Application.User.Queries.Common;
//using CoreBanking.Core.Interfaces;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace CoreBanking.Application.User.Queries.GetAllUsers;

//public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserDto>>>
//{
//    private readonly IUserRepository _userRepository;
//    private readonly ILogger<GetAllUsersQueryHandler> _logger;

//    public GetAllUsersQueryHandler(
//        IUserRepository userRepository,
//        ILogger<GetAllUsersQueryHandler> logger)
//    {
//        _userRepository = userRepository;
//        _logger = logger;
//    }

//    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Getting all users");

//        var users = await _userRepository.GetAllAsync(cancellationToken);

//        var userDtos = users
//            .Where(u => u != null)
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
//                DateCreated = u.DateCreated,
             
//            })
//            .ToList();

//        _logger.LogInformation("Retrieved {Count} users", userDtos.Count);

//        return Result<List<UserDto>>.Success(userDtos);
//    }
//}
