using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Queries.GetAccountBalance
{
    public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, Result<AccountBalanceDto>>
    {
        private readonly IAccountRepository _accountRepository;

        public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<Result<AccountBalanceDto>> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);

            if (account == null)
                return Result<AccountBalanceDto>.Failure("Account not found");

            // Check if customer owns this account (authorization)
            //if (account.CustomerId != request.CustomerId)
                //return Result<AccountBalanceDto>.Failure("Access denied");

            var dto = new AccountBalanceDto
            {
                CurrentBalance = account.CurrentBalance.Amount,
                AvailableBalance = account.AvailableBalance.Amount,
                Currency = account.CurrentBalance.Currency,
                LastUpdated = DateTime.UtcNow
            };

            return Result<AccountBalanceDto>.Success(dto);
        }
    }
}