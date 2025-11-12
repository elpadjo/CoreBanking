using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetAccountDetails
{
    public record AccountDetailsDto
    {
        public string AccountNumber { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public decimal CurrentBalance { get; init; }
        public decimal AvailableBalance { get; init; }
        public DateTime DateOpened { get; init; }
        public AccountStatus AccountStatus { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public CustomerId CustomerId { get; init; }
    }
}
