using System;
using CoreBanking.Application.Accounts.Queries.GetTransfer;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Queries.GetTransferHistory;

public class GetTransferHistoryQueryHandler : IRequestHandler<GetTransferHistoryQuery, Result<TransferHistoryDto>>
{

    ITransferRepository _transferRepository;

    IAccountRepository _accountRepository;

    public GetTransferHistoryQueryHandler(ITransferRepository transferRepository, IAccountRepository accountRepository)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
    }
    public async Task<Result<TransferHistoryDto>> Handle(GetTransferHistoryQuery request, CancellationToken cancellationToken)
    {

        var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);

        if (account is null)
        {
            return Result<TransferHistoryDto>.Failure($"Account with account number {request.AccountNumber} not found");
        }
        
        var transferList = await _transferRepository.GetAllTransferHistoryForSingleAccountAsync(account.Id, cancellationToken);

        var result = new TransferHistoryDto
        {
            AccountNumber = AccountNumber.Create(request.AccountNumber),
            Page = request.Page,
            TotalCount = transferList.Count,
            Transfers = transferList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TransferDto
                {
                    FromAccountId = t.FromAccountId,
                    ToAccountId = t.ToAccountId,
                    Amount = t.Amount,
                    Description = t.Description,
                    CompletedAt = t.CompletedAt,
                    InitiatedAt = t.InitiatedAt,
                    Reference = t.Reference,
                    ScheduledAt = t.ScheduledAt,
                    Status = t.Status
                }).ToList(),
            TotalPages = (int)Math.Ceiling(transferList.Count / (double)request.PageSize)
        };
        
        return Result<TransferHistoryDto>.Success(result);
    }
}
