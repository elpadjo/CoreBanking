using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.User.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateRoleCommandHandler> _logger;

    public UpdateRoleCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating role for user {UserId} to {NewRole} by {ChangedBy}",
            request.UserId.Value, request.NewRole, request.ChangedBy);

        try
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId.Value);
                return Result.Failure("User not found");
            }

            // Parse the new role
            if (!Enum.TryParse<UserRole>(request.NewRole, true, out var newRole))
            {
                _logger.LogWarning("Invalid role {Role} specified", request.NewRole);
                return Result.Failure($"Invalid role. Valid roles are: {string.Join(", ", Enum.GetNames<UserRole>())}");
            }

            // Update role
            user.UpdateRole(newRole, request.ChangedBy);

            // Save changes
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated role for user {UserId} to {NewRole}",
                request.UserId.Value, newRole);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating role for user {UserId}", request.UserId.Value);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role for user {UserId}", request.UserId.Value);
            return Result.Failure("Failed to update role");
        }
    }
}
