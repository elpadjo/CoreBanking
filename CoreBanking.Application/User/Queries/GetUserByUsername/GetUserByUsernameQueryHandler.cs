using CoreBanking.Application.Common.Models;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Queries.GetUserByUsername;

public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, Result<UserDetailsDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByUsernameQueryHandler> _logger;

    public GetUserByUsernameQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByUsernameQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserDetailsDto>> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user details for username {Username}", request.Username);

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User with username {Username} not found", request.Username);
            return Result<UserDetailsDto>.Failure("User not found");
        }

        var dto = new UserDetailsDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            IsLocked = user.IsLocked(),
            LockedUntil = user.LockedUntil,
            CanLogin = user.CanLogin(),
            DateCreated = user.DateCreated,
            DateUpdated = user.DateUpdated
        };

        return Result<UserDetailsDto>.Success(dto);
    }
}
