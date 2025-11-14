using CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;
using FluentValidation;

namespace CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences
{
    public class UpdateAccountPreferencesCommandValidator : AbstractValidator<UpdateAccountPreferencesCommand>
    {
        public UpdateAccountPreferencesCommandValidator()
        {
            RuleFor(x => x.AccountNumber)
                .NotNull().WithMessage("Account number is required")
                .Must(accountNumber => accountNumber?.Value?.Length == 10)
                .WithMessage("Account number must be 10 digits");

            //RuleFor(x => x.CustomerId)
                //.NotNull().WithMessage("Customer ID is required");

            // Validate Money object
            RuleFor(x => x.LowBalanceThreshold)
                .Must(BeValidMoney).WithMessage("Low balance threshold must be a positive amount")
                .When(x => x.LowBalanceThreshold != null && x.EnableLowBalanceAlerts);

            // Enum validation - much simpler!
            RuleFor(x => x.MonthlyStatementPreference)
                .IsInEnum().WithMessage("Invalid statement preference value");
        }

        private bool BeValidMoney(Money? money)
        {
            return money == null || money.Amount > 0;
        }
    }
}