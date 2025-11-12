using CoreBanking.Application.Common.Models;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Queries.GetUserByEmail;

public class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, Result<UserDetailsDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;

    public GetUserByEmailQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUserByEmailQueryHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserDetailsDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user details for email {Email}", request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User with email {Email} not found", request.Email);
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
