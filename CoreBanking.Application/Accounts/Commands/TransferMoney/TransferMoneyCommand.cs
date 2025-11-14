using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.ValueObjects;
namespace CoreBanking.Application.Accounts.Commands.TransferMoney;

public record TransferMoneyCommand : ICommand
{
    public required AccountNumber SourceAccountNumber { get; init; } 
    public required AccountNumber DestinationAccountNumber { get; init; } 
    public required Money Amount { get; init; }
    public string Reference { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}