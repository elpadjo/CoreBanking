using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.DTOs
{
    public record AccountDetailsDto
    {
        public string AccountNumber { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public decimal CurrentBalance { get; init; } 
        public decimal AvailableBalance { get; init; }
        public DateTime DateOpened { get; init; }
        public AccountStatus AccountStatus { get; init; }
        public bool EnableTransactionAlerts { get; init; } = true;
        public bool EnableLowBalanceAlerts { get; init; } = true;
        public Money? LowBalanceThreshold { get; init; }
        public MonthlyStatementPreferenceType? MonthlyStatementPreference { get; init; } = MonthlyStatementPreferenceType.Email;
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerId { get; init; } = string.Empty; 
    }
}
