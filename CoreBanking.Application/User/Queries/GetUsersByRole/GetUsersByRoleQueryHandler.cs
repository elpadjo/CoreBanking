using CoreBanking.Application.Common.Models;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Queries.GetUsersByRole;

public class GetUsersByRoleQueryHandler : IRequestHandler<GetAllUserQueryRole, Result<PaginatedResult<UserDto>>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersByRoleQueryHandler> _logger;

    public GetUsersByRoleQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUsersByRoleQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }


    public async Task<Result<PaginatedResult<UserDto>>> Handle(GetAllUserQueryRole request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting users with role {Role}", request.Role);

        // Validate role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            _logger.LogWarning("Invalid role {Role} specified", request.Role);
            return Result<PaginatedResult<UserDto>>.Failure($"Invalid role. Valid roles are: {string.Join(", ", Enum.GetNames<UserRole>())}");
        }

        var users = await _userRepository.GetAllAsync(request.pageNumber, request.pageSize, cancellationToken);

        var userDtos = users.Items.Where(u => u != null && u.Role == userRole)
            .Select(u => new UserDto
            {
                UserId = u!.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                FailedLoginAttempts = u.FailedLoginAttempts,
                IsLocked = u.IsLocked(),
                LockedUntil = u.LockedUntil,
                DateCreated = u.DateCreated
            })
            .ToList();

        // Calculate the actual count of filtered users
        var filteredCount = userDtos.Count;

        // Create paginated result with filtered data
        var paginatedResult = PaginatedResult<UserDto>.Create(
            userDtos,
            filteredCount,
            request.pageNumber,
            request.pageSize
        );

        _logger.LogInformation("Retrieved {Count} users with role {Role}", userDtos.Count, userRole);

        return Result<PaginatedResult<UserDto>>.Success(paginatedResult);
    }
}
