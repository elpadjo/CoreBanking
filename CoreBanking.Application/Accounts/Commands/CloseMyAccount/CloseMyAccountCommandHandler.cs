using CoreBanking.Application.Accounts.Commands.CloseMyAccount;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Commands.CloseMyAccount
{
    public class CloseMyAccountCommandHandler : IRequestHandler<CloseMyAccountCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CloseMyAccountCommandHandler(
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CloseMyAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);

            if (account == null)
                return Result.Failure("Account not found");

            // Authorization: Customer can only close their own account
            //if (account.CustomerId != request.CustomerId)
                //return Result.Failure("Access denied - you can only close your own accounts");

            // Business rule validation
            var validationResult = ValidateAccountCanBeClosed(account);
            if (!validationResult.IsSuccess)
                return validationResult;

            try
            {
                // Execute domain behavior
                account.CloseAccount(request.Reason);

                // Update repository
                _accountRepository.Update(account);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        private Result ValidateAccountCanBeClosed(Account account)
        {
            // Check if account is already closed
            if (account.AccountStatus == CoreBanking.Core.Enums.AccountStatus.Closed)
                return Result.Failure("Account is already closed");

            // Check if account has zero balance (customer requirement)
            if (account.CurrentBalance.Amount != 0)
                return Result.Failure("Cannot close account with non-zero balance. Please withdraw remaining funds first.");

            // Check if account is active
            if (account.AccountStatus != CoreBanking.Core.Enums.AccountStatus.Active)
                return Result.Failure("Only active accounts can be closed");

            // Check if account is not already deleted
            if (account.IsDeleted)
                return Result.Failure("Account has already been deleted");

            return Result.Success();
        }
    }
}