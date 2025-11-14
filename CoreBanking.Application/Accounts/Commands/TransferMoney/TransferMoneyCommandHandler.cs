using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Commands.TransferMoney
{
    public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, Result>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransferRepository _transferRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransferMoneyCommandHandler(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            ITransferRepository transferRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;

        }

        public async Task<Result> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Find source and destination accounts
                var sourceAccount = await _accountRepository.GetByAccountNumberAsync(new AccountNumber(request.SourceAccountNumber));
                var destAccount = await _accountRepository.GetByAccountNumberAsync(new AccountNumber(request.DestinationAccountNumber));

                if (sourceAccount == null)
                    return Result.Failure("Source account not found");
                if (destAccount == null)
                    return Result.Failure("Destination account not found");

                var breakpointz = 1;

                // Execute transfer using domain logic - this will now throw exceptions
                sourceAccount.Transfer(
                    amount: request.Amount,
                    destination: destAccount,
                    reference: request.Reference,
                    description: request.Description
                );

                var breakpoint = 1;

                // var transferRecord = new Transfer(
                //     fromAccountId: sourceAccount.Id,
                //     toAccountId: destAccount.Id,
                //     amount: request.Amount,
                //     reference: request.Reference,
                //     description: request.Description
                // );

                // await _transferRepository.CreateTransferRecordAsync(transferRecord, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (InvalidOperationException ex)
            {
                // Handle domain business rule violations
                return Result.Failure(ex.Message);
            }
            catch (ArgumentException ex)
            {
                // Handle argument validation errors
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return Result.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
