using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetAccountDetails
{
    public record AccountDetailsDto
    {
        public string AccountNumber { get; init; } = string.Empty;
        public string AccountType { get; init; } = string.Empty;
        public Money Balance { get; init; } = new Money(0);
        public DateTime DateOpened { get; init; }
        public bool IsActive { get; init; }
        public string CustomerName { get; init; } = string.Empty;
    }
}
