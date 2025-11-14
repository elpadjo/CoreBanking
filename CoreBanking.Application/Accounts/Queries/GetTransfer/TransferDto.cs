using System;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetTransfer;

public class TransferDto
{
    public required AccountId FromAccountId { get; init; } 
    public required AccountId ToAccountId { get; init; }  
    public required Money Amount { get; init; }  
    public TransferStatus Status { get; init; } = TransferStatus.Pending;
    public DateTime? ScheduledAt { get; init; }
    public DateTime InitiatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
