using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences
{
    public class UpdateAccountPreferencesCommand : IRequest<Result>
    {
        public AccountNumber AccountNumber { get; set; }
        //public CustomerId CustomerId { get; set; }
        public bool EnableTransactionAlerts { get; set; } = true;
        public bool EnableLowBalanceAlerts { get; set; } = true;
        public Money? LowBalanceThreshold { get; set; }
        public MonthlyStatementPreferenceType MonthlyStatementPreference { get; set; } = MonthlyStatementPreferenceType.Email;
    }
}