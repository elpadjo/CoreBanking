using System;
using CoreBanking.Application.Accounts.Queries.GetTransfer;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetTransferHistory;

public class TransferHistoryDto
{
    public required AccountNumber AccountNumber { get; init; } 
    public List<TransferDto> Transfers { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
}
