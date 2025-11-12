using CoreBanking.Application.Customers.Commands.UpdateCreditScore;
using FluentValidation;

public class UpdateCreditScoreCommandValidator : AbstractValidator<UpdateCreditScoreCommand>
{
    public UpdateCreditScoreCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("CustomerId cannot be empty.");

        RuleFor(c => c.NewCreditScore)
            .InclusiveBetween(300, 850)
            .WithMessage("New credit score must be between 300 and 850.");

        RuleFor(c => c.Reason)
            .MaximumLength(250)
            .WithMessage("Reason cannot exceed 250 characters.");
    }
}
