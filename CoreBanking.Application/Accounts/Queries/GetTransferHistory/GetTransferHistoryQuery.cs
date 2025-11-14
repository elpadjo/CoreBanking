using System;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetTransferHistory;

public class GetTransferHistoryQuery:IQuery<TransferHistoryDto>
{
    public required AccountNumber AccountNumber { get; init; } 
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
