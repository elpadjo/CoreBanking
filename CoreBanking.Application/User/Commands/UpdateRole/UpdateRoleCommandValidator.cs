using CoreBanking.Core.Enums;
using FluentValidation;

namespace CoreBanking.Application.User.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.UserId.Value)
            .NotEmpty().WithMessage("User ID is required")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

        RuleFor(x => x.NewRole)
            .NotEmpty().WithMessage("Role is required")
            .Must(BeValidRole).WithMessage($"Invalid role. Valid roles are: {string.Join(", ", Enum.GetNames<UserRole>())}");

        RuleFor(x => x.ChangedBy)
            .NotEmpty().WithMessage("ChangedBy is required")
            .MaximumLength(100).WithMessage("ChangedBy cannot exceed 100 characters");
    }

    private bool BeValidRole(string role)
        => Enum.TryParse<UserRole>(role, true, out _);
}
