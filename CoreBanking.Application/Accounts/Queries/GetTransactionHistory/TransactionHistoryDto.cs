using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetTransactionHistory
{
    public record TransactionHistoryDto
    {
        public AccountNumber AccountNumber { get; init; } = AccountNumber.Create(string.Empty);
        public List<TransactionDto> Transactions { get; init; } = new();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int TotalPages { get; init; }
    }
}
