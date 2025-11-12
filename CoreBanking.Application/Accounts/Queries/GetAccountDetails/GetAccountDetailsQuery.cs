using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Accounts.Queries.GetAccountDetails;

public record GetAccountDetailsQuery : IQuery<AccountDetailsDto>
{
    public required AccountNumber AccountNumber { get; init; }
}