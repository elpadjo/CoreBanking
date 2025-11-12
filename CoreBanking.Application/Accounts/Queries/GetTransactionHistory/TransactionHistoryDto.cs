using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetTransactionHistory
{
    public record TransactionHistoryDto
    {
        public AccountNumber AccountNumber { get; init; } 
        public List<TransactionDto> Transactions { get; init; } = new();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int TotalPages { get; init; }
    }

    //public record SingleTransactionHistoryDto
    //{
    //    public AccountNumber AccountNumber { get; init; }
    //    public TransactionDto Transactions { get; init; }
    //}
}
