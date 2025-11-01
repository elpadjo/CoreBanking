using CoreBanking.Application.Common.Models;
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
                AccountNumber = account.AccountNumber,
                AccountType = account.AccountType.ToString(),
                Balance = new Money(account.Balance.Amount, account.Balance.Currency),
                DateOpened = account.DateOpened,
                IsActive = account.IsActive,
                CustomerName = $"{account.Customer.FirstName} {account.Customer.LastName}"
            };

            return Result<AccountDetailsDto>.Success(dto);
        }
    }
}
