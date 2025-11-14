using CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.Accounts.Commands.UpdateAccountPreferences
{
    public class UpdateAccountPreferencesCommandHandler : IRequestHandler<UpdateAccountPreferencesCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateAccountPreferencesCommandHandler> _logger;

        public UpdateAccountPreferencesCommandHandler(
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateAccountPreferencesCommandHandler> logger)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateAccountPreferencesCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);

            if (account == null)
                return Result.Failure("Account not found");

            // Authorization: Customer can only update their own account preferences
            //if (account.CustomerId != request.CustomerId)
                //return Result.Failure("Access denied - you can only update preferences for your own accounts");

            // Validate account is active
            if (account.AccountStatus != CoreBanking.Core.Enums.AccountStatus.Active)
                return Result.Failure("Cannot update preferences for inactive or closed account");

            try
            {
                // Update preferences (you might need to add these properties to your Account entity)
                UpdateAccountPreferences(account, request);

                // Update repository
                _accountRepository.Update(account);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                //_logger.LogInformation(
                    //"Account preferences updated for account {AccountNumber} by customer {CustomerId}",
                    //request.AccountNumber.Value, request.CustomerId.Value);

                _logger.LogInformation(
                    "Account preferences updated for account {AccountNumber} by customer",
                    request.AccountNumber.Value);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account preferences for account {AccountNumber}",
                    request.AccountNumber.Value);
                return Result.Failure("An error occurred while updating account preferences");
            }
        }

        private void UpdateAccountPreferences(Account account, UpdateAccountPreferencesCommand request)
        {
            // If you have preferences as part of your Account entity:
            // account.EnableTransactionAlerts = request.EnableTransactionAlerts;
            // account.EnableLowBalanceAlerts = request.EnableLowBalanceAlerts;
            // account.LowBalanceThreshold = request.LowBalanceThreshold;
            // account.MonthlyStatementPreference = request.MonthlyStatementPreference;

            // If you don't have these properties yet, you might need to:
            // 1. Add them to your Account entity
            // 2. Or create a separate AccountPreferences entity
            // 3. Or store them in a different way

            // For now, this is a placeholder - you'll need to implement based on your domain model
            _logger.LogInformation(
                "Updating preferences for account {AccountNumber}: TransactionAlerts={TransactionAlerts}, LowBalanceAlerts={LowBalanceAlerts}, Threshold={Threshold}, StatementPref={StatementPref}",
                account.AccountNumber.Value,
                request.EnableTransactionAlerts,
                request.EnableLowBalanceAlerts,
                request.LowBalanceThreshold,
                request.MonthlyStatementPreference);

            // Update timestamp to reflect the change
            // You might need to add an UpdatePreferences method to your Account entity
            // account.UpdatePreferences(...);
        }
    }
}