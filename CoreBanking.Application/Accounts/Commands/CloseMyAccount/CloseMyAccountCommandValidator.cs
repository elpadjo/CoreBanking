using CoreBanking.Application.Accounts.Commands.CloseMyAccount;
using FluentValidation;

namespace CoreBanking.Application.Accounts.Commands.CloseMyAccount
{
    public class CloseMyAccountCommandValidator : AbstractValidator<CloseMyAccountCommand>
    {
        public CloseMyAccountCommandValidator()
        {
            RuleFor(x => x.AccountNumber)
                .NotNull().WithMessage("Account number is required")
                .Must(accountNumber => accountNumber?.Value?.Length == 10)
                .WithMessage("Account number must be 10 digits");

            //RuleFor(x => x.CustomerId)
                //.NotNull().WithMessage("Customer ID is required");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason for closing account is required")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters")
                .Must(reason => !string.IsNullOrWhiteSpace(reason))
                .WithMessage("Reason cannot be empty or whitespace");
        }
    }
}