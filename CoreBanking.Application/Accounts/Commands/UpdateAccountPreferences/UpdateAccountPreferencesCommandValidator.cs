using CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences;
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

            RuleFor(x => x.LowBalanceThreshold)
                .GreaterThan(0).WithMessage("Low balance threshold must be positive")
                .When(x => x.LowBalanceThreshold.HasValue && x.EnableLowBalanceAlerts);

            RuleFor(x => x.MonthlyStatementPreference)
                .NotEmpty().WithMessage("Statement preference is required")
                .Must(BeAValidPreference).WithMessage("Statement preference must be 'Email', 'Paper', 'Both', or 'None'");
        }

        private bool BeAValidPreference(string preference)
        {
            var validPreferences = new[] { "Email", "Paper", "Both", "None" };
            return validPreferences.Contains(preference);
        }
    }
}