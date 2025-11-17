using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Queries.GetAccountDetails
{
    public class GetAccountDetailsQueryHandler : IRequestHandler<GetAccountDetailsQuery, Result<AccountDetailsDto>>
    {
        private readonly IAccountRepository _accountRepository;

        public GetAccountDetailsQueryHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Result<AccountDetailsDto>> Handle(GetAccountDetailsQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(AccountNumber.Create(request.AccountNumber));

            if (account == null)
                return Result<AccountDetailsDto>.Failure("Account not found");

            var dto = new AccountDetailsDto
            {
                AccountNumber = account.AccountNumber.ToString(),
                AccountType = account.AccountType,
                CurrentBalance = account.CurrentBalance.Amount,
                AvailableBalance = account.AvailableBalance.Amount,
                DateOpened = account.DateOpened,
                AccountStatus = account.AccountStatus,
                EnableTransactionAlerts = account.EnableTransactionAlerts,
                EnableLowBalanceAlerts = account.EnableLowBalanceAlerts,
                LowBalanceThreshold = account.LowBalanceThreshold,
                MonthlyStatementPreference = account.MonthlyStatementPreference,
                CustomerId = account.CustomerId.ToString(),
                CustomerName = $"{account.Customer.FirstName} {account.Customer.LastName}"
            };

            return Result<AccountDetailsDto>.Success(dto);
        }
    }
}
