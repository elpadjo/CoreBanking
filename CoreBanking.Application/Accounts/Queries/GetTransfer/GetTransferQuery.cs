
using CoreBanking.Application.Accounts.Queries.GetTransfer;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Queries.GetTransaction;

public record GetTransferQuery : IQuery<TransferDto>
{
    public required AccountNumber AccountNumber { get; init; }
    public TransferId TransferId { get; init; } = TransferId.Create();

    // public AccountNumber AccountNumber { get; init; } = AccountNumber.Create(string.Empty);
    // public DateTime? StartDate { get; init; }
    // public DateTime? EndDate { get; init; }
    // public int Page { get; init; } = 1;
    // public int PageSize { get; init; } = 50;
}

public class GetTransferQueryHandler : IRequestHandler<GetTransferQuery, Result<TransferDto>>
{

     ITransferRepository _transferRepository;

    IAccountRepository _accountRepository;

    public GetTransferQueryHandler(ITransferRepository transferRepository, IAccountRepository accountRepository)
    {
        _transferRepository = transferRepository;
        _accountRepository = accountRepository;
    }

    Task<Result<TransferDto>> IRequestHandler<GetTransferQuery, Result<TransferDto>>.Handle(GetTransferQuery request, CancellationToken cancellationToken)
    {

        var account = _accountRepository.GetByAccountNumberAsync(request.AccountNumber).Result;

        if (account is null)
        {
            return Task.FromResult(Result<TransferDto>.Failure($"Account with account number {request.AccountNumber} not found"));
        }
        
        var transfer = _transferRepository.GetTransferByIdAsync(request.TransferId, cancellationToken).Result;

        if (transfer is null)
        {
            return Task.FromResult(Result<TransferDto>.Failure($"Transfer with id {request.TransferId} not found"));
        }

        var transferDto = new TransferDto
        {
            FromAccountId = transfer.FromAccountId,
            ToAccountId = transfer.ToAccountId,
            Amount = transfer.Amount,
            Description = transfer.Description,
            CompletedAt = transfer.CompletedAt,
            InitiatedAt = transfer.InitiatedAt,
            Reference = transfer.Reference,
            ScheduledAt = transfer.ScheduledAt,
            Status = transfer.Status
        };
        return Task.FromResult(Result<TransferDto>.Success(transferDto));
    }
}

