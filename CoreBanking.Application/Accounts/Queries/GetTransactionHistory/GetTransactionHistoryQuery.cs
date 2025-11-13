using CoreBanking.Application.Accounts.Queries.GetAccountDetails;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Queries.GetTransactionHistory;

public record GetTransactionHistoryQuery : IQuery<TransactionHistoryDto>
{
    public required AccountNumber AccountNumber { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record GetSingleTransactionHistoryQuery : IQuery<TransactionDto>
{
    //public required AccountNumber AccountNumber { get; init; }
    public required TransactionId TransactionId { get; init; }
 
}

public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, Result<TransactionHistoryDto>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public GetTransactionHistoryQueryHandler(
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

    public async Task<Result<TransactionHistoryDto>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        //var account = await _accountRepository.GetByAccountNumberAsync(new AccountNumber(request.AccountNumber));
        //if (account == null)
        //    return Result<TransactionHistoryDto>.Failure("Account not found");

        var account = await _accountRepository.GetByAccountNumberAsync(AccountNumber.Create(request.AccountNumber));

        if (account == null)
            return Result<TransactionHistoryDto>.Failure("Account not found");

        // Start with IQueryable or IEnumerable from repository
        var transactionsQuery = (await _transactionRepository.GetByAccountIdAsync(account.Id, cancellationToken))
                .AsQueryable(); // Or keep as IEnumerable

        // Apply date filtering
        if (request.StartDate.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.DateCreated >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            transactionsQuery = transactionsQuery.Where(t => t.DateCreated <= request.EndDate.Value);

        // Get total count before pagination
        var totalCount = transactionsQuery.Count();

        // Apply pagination and execute query
        var pagedTransactions = transactionsQuery
            .OrderByDescending(t => t.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var transactionDtos = pagedTransactions.Select(t => new TransactionDto
        {
            TransactionId = TransactionId.Create(t.Id.Value),
            Type = t.Type.ToString(),
            Amount = t.Amount.Amount,
            Currency = t.Amount.Currency,
            Description = t.Description,
            Reference = t.Reference,
            Timestamp = t.DateCreated
        }).ToList();

        var dto = new TransactionHistoryDto
        {
            AccountNumber = request.AccountNumber,
            Transactions = transactionDtos,
            TotalCount = totalCount,
            Page = request.Page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result<TransactionHistoryDto>.Success(dto);
    }

    public class GetSingleTransactionHistoryQueryHandler : IRequestHandler<GetSingleTransactionHistoryQuery, Result<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public GetSingleTransactionHistoryQueryHandler(
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }
        public async Task<Result<TransactionDto>> Handle(GetSingleTransactionHistoryQuery request, CancellationToken cancellationToken)
        {


            //var account = await _accountRepository.GetByAccountNumberAsync(AccountNumber.Create(request.AccountNumber));

            //if (account == null)
            //    return Result<TransactionDto>.Failure("Account not found");

            var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);

            if (transaction == null)
                return Result<TransactionDto>.Failure("Transaction not found");

            // Start with IQueryable or IEnumerable from repository
            //var transactionsQuery = (await _transactionRepository.GetByIdAsync(account.Id, , cancellationToken))
            //        .AsQueryable(); // Or keep as IEnumerable

            // Apply date filtering
            //if (request.StartDate.HasValue)
            //    transactionsQuery = transactionsQuery.Where(t => t.DateCreated >= request.StartDate.Value);
            //if (request.EndDate.HasValue)
            //    transactionsQuery = transactionsQuery.Where(t => t.DateCreated <= request.EndDate.Value);

            // Get total count before pagination
            //var totalCount = transactionsQuery.Count();

            //// Apply pagination and execute query
            //var pagedTransactions = transactionsQuery
            //    .OrderByDescending(t => t.DateCreated)
            //    .Skip((request.Page - 1) * request.PageSize)
            //    .Take(request.PageSize)
            //    .ToList();

            var transactionDtos = new TransactionDto
            {
                TransactionId = TransactionId.Create(transaction.Id.Value),
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount.Amount,
                Currency = transaction.Amount.Currency,
                Description = transaction.Description,
                Reference = transaction.Reference,
                Timestamp = transaction.DateCreated
            };




            return Result<TransactionDto>.Success(transactionDtos);
        }
    }

}       