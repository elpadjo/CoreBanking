using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.ValueObjects;
namespace CoreBanking.Application.Accounts.Commands.TransferMoney;

public record TransferMoneyCommand : ICommand
{
    public AccountNumber SourceAccountNumber { get; init; } = AccountNumber.Create(string.Empty);
    public AccountNumber DestinationAccountNumber { get; init; } = AccountNumber.Create(string.Empty);
    public Money Amount { get; init; } = new Money(0);
    public string Reference { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}