using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.API.Models.Requests
{
    public class CreateTransactionRequest
    {
            //public TransactionId TransactionId { get; init; } = TransactionId.Create();
            public AccountNumber AccountNumber { get; init; }

            public string TransactionType { get; init; } = string.Empty;

            public string Description { get; init; } = string.Empty;
            //public decimal RunningBalance { get; init; }
            public decimal TrxAmount { get; init; }
            public string Currency { get; init; } = "NGN";
            //public AccountId ? RelatedAccountId { get; init; } = null;
            //public string Reference { get; init; } = string.Empty;
            //public string TransactionReference { get; init; } = string.Empty;
        
    }
}
