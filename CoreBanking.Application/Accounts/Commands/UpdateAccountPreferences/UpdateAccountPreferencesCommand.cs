using CoreBanking.Application.Common.Models;
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
        public decimal? LowBalanceThreshold { get; set; }
        public string MonthlyStatementPreference { get; set; } = "Email"; // Email, Paper, Both, None
    }
}