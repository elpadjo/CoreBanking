using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Commands.RestoreUser;

public class RestoreUserCommandHandler : IRequestHandler<RestoreUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RestoreUserCommandHandler> _logger;

    public RestoreUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<RestoreUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Restoring soft deleted user {UserId}", request.UserId.Value);

        try
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId.Value);
                return Result.Failure("User not found");
            }

            // Restore user
            user.Restore();

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully restored user {UserId}. Note: User must be manually activated.",
                request.UserId.Value);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while restoring user {UserId}", request.UserId.Value);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring user {UserId}", request.UserId.Value);
            return Result.Failure("Failed to restore user");
        }
    }
}
