using FluentValidation;

namespace CoreBanking.Application.User.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId.Value)
            .NotEmpty().WithMessage("User ID is required")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty");

        RuleFor(x => x.DeletedBy)
            .NotEmpty().WithMessage("DeletedBy is required")
            .MaximumLength(100).WithMessage("DeletedBy cannot exceed 100 characters");
    }
}
