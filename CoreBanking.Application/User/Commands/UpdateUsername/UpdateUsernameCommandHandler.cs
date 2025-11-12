using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Commands.UpdateUsername;

public class UpdateUsernameCommandHandler : IRequestHandler<UpdateUsernameCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUsernameCommandHandler> _logger;

    public UpdateUsernameCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUsernameCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUsernameCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating username for user {UserId}", request.UserId.Value);

        try
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId.Value);
                return Result.Failure("User not found");
            }

            // Check if new username is already taken
            var existingUser = await _userRepository.GetByUsernameAsync(request.NewUsername, cancellationToken);
            if (existingUser != null && existingUser.Id != request.UserId)
            {
                _logger.LogWarning("Username {Username} is already taken", request.NewUsername);
                return Result.Failure("Username is already taken");
            }

            // Update username
            user.UpdateUsername(request.NewUsername);

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated username for user {UserId}", request.UserId.Value);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating username for user {UserId}", request.UserId.Value);
            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid username format for user {UserId}", request.UserId.Value);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating username for user {UserId}", request.UserId.Value);
            return Result.Failure("Failed to update username");
        }
    }
}
