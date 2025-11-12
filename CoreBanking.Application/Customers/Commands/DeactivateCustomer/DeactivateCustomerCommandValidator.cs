using CoreBanking.Application.Customers.Commands.DeactivateCustomer;
using FluentValidation;

public class DeactivateCustomerCommandValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerCommandValidator()
    {
        RuleFor(c => c.CustomerId)
            .NotEmpty().WithMessage("CustomerId cannot be empty");

        RuleFor(c => c.Reason)
            .MaximumLength(250).WithMessage("Reason cannot exceed 250 characters");
    }
}
