using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for username {Username}", request.Username);

        try
        {
            // Get user by username
            var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user {Username} not found", request.Username);
                return Result<LoginUserResponse>.Failure("Invalid username or password");
            }

            // Check if user can login (active and not locked)
            if (!user.CanLogin())
            {
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - user {Username} is inactive", request.Username);
                    return Result<LoginUserResponse>.Failure("Account is inactive. Please contact administrator.");
                }

                if (user.IsLocked())
                {
                    _logger.LogWarning("Login failed - user {Username} is locked until {LockedUntil}",
                        request.Username, user.LockedUntil);
                    return Result<LoginUserResponse>.Failure(
                        $"Account is locked due to multiple failed login attempts. Please try again after {user.LockedUntil?.ToString("yyyy-MM-dd HH:mm:ss")} UTC.");
                }
            }

            // Verify password
            var verificationResult = _passwordHasher.VerifyHashedPassword(user.PasswordHash, request.Password);

            if (string.IsNullOrEmpty(verificationResult) || verificationResult != "Success")
            {
                // Password is incorrect - record failed login
                user.RecordFailedLogin();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Login failed - invalid password for user {Username}. Failed attempts: {FailedAttempts}",
                    request.Username, user.FailedLoginAttempts);

                if (user.FailedLoginAttempts >= 5)
                {
                    return Result<LoginUserResponse>.Failure(
                        "Invalid username or password. Your account has been locked for 30 minutes due to multiple failed attempts.");
                }

                return Result<LoginUserResponse>.Failure("Invalid username or password");
            }

            // Password is correct - record successful login
            user.RecordSuccessfulLogin();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            // Return login response
            var response = new LoginUserResponse
            {
                UserId = user.Id.Value,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive
            };

            return Result<LoginUserResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username {Username}", request.Username);
            return Result<LoginUserResponse>.Failure("An error occurred during login. Please try again.");
        }
    }
}
