using CoreBanking.Application.Customers.Commands.ReactivateCustomer;
using FluentValidation;

public class ReactivateCustomerCommandValidator : AbstractValidator<ReactivateCustomerCommand>
{
    public ReactivateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("CustomerId cannot be empty");

        RuleFor(c => c.Reason)
            .MaximumLength(250).WithMessage("Reason cannot exceed 250 characters.");
    }
}
