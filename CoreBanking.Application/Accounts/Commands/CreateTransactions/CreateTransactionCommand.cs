using CoreBanking.Application.Accounts.Queries.GetTransactionHistory;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.Commands.CreateTransactions
{
    public record CreateTransactionCommand : ICommand<TransactionId>
    {
        //public TransactionId TransactionId { get; init; } = TransactionId.Create();
        public AccountNumber AccountNumber { get; init; } 

        public string TransactionType { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
        public decimal TrxAmount { get; init; }
        public string Currency { get; init; } = "NGN";

        //public AccountId ? RelatedAccountId { get; init; } = null;
        //public string TransactionReference { get; init; } = string.Empty;
    }

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result<TransactionId>>
    {
        private readonly IAccountRepository _accountRepository;
        //private readonly ICustomerRepository _customerRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTransactionCommandHandler(
            IAccountRepository accountRepository,
            //ICustomerRepository customerRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository; 
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TransactionId>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            // Validate customer exists
            //var customer = await _accountRepository.GetByIdAsync(request.CustomerId);
            //if (customer == null)
            //    return Result<Guid>.Failure("Customer not found");

            // Validate Account exists
            var account = await _accountRepository.GetByAccountNumberAsync(AccountNumber.Create(request.AccountNumber));

            if (account == null)
                return Result<TransactionId>.Failure("Account not found");

            // Generate unique transaction id
            //var transactionId = await GenerateUniqueAccountNumberAsync();

            // Create account with initial deposit
            //var transaction = Transaction.Create(
            //    accountId: account.Id,
            //    transactionType: Enum.Parse<TransactionType>(request.TransactionType),
            //    transanctionAmount: new Money(request.TrxAmount, request.Currency),
            //    desctiption: request.Description,
            //    relatedAccountId: (AccountId?)null,
            //    transactionReference: request.TransactionReference,
            //    reference: request.Reference

            //);
            var TransactionReference = GenerateTransactionReference();

            var transaction = new Transaction(
                account.Id,
                Enum.Parse<TransactionType>(request.TransactionType),
                new Money(request.TrxAmount, request.Currency),
                request.Description,
                //(AccountId?)null,
                TransactionReference
                );

            // Add to repository
            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<TransactionId>.Success(transaction.Id);
        }

        private string GenerateTransactionReference()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); // e.g. 20251112103245
            var randomPart = Random.Shared.Next(1000, 9999);
            return $"TXN{timestamp}{randomPart}";
        }


        //private async Task<AccountNumber> GenerateUniqueAccountNumberAsync()
        //{
        //    string accountNumber;
        //    do
        //    {
        //        accountNumber = GenerateAccountNumber();
        //    } while (await _accountRepository.AccountNumberExistsAsync(new AccountNumber(accountNumber)));

        //    return new AccountNumber(accountNumber);
        //}

        //private string GenerateAccountNumber() =>
        //    DateTime.UtcNow.ToString("HHmmss") + Random.Shared.Next(1000, 9999).ToString();
    }
}
