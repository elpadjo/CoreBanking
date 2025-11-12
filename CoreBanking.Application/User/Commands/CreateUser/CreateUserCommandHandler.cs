using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UserId>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user with username {Username} and role {Role}",
            request.Username, request.Role);

        try
        {
            // Check if username already exists
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
            if (existingUserByUsername != null)
            {
                _logger.LogWarning("Username {Username} already exists", request.Username);
                return Result<UserId>.Failure("Username already exists");
            }

            // Check if email already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUserByEmail != null)
            {
                _logger.LogWarning("Email {Email} already exists", request.Email);
                return Result<UserId>.Failure("Email already exists");
            }

            // Parse and validate role
            if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            {
                _logger.LogWarning("Invalid role {Role} specified", request.Role);
                return Result<UserId>.Failure($"Invalid role. Valid roles are: {string.Join(", ", Enum.GetNames<UserRole>())}");
            }

            // Hash password
            var passwordHash = _passwordHasher.HashPassword(request.Password);

            // Create user entity using static factory method
            var user = Core.Entities.User.Create(
                username: request.Username,
                passwordHash: passwordHash,
                role: userRole,
                email: request.Email
            );

            // Add to repository
            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created user {UserId} with username {Username}",
                user.Id.Value, request.Username);

            return Result<UserId>.Success(user.Id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating user {Username}", request.Username);
            return Result<UserId>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", request.Username);
            return Result<UserId>.Failure("Failed to create user");
        }
    }
}
