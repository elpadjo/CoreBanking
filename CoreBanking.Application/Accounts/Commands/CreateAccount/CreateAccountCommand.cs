using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Commands.CreateAccount;

public record CreateAccountCommand : ICommand<CreateAccountResponse>
{
    public CustomerId CustomerId { get; init; } = CustomerId.Create();
    public string AccountType { get; init; } = string.Empty;
    public decimal InitialDeposit { get; init; }
    public string Currency { get; init; } = "NGN";
}