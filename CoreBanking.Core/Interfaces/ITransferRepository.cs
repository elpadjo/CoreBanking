using System;
using CoreBanking.Core.Entities;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Core.Interfaces;

public interface ITransferRepository
{
    public Task<Transfer> GetTransferByIdAsync(TransferId Id, CancellationToken cancellationToken = default);
    public Task<List<Transfer>> GetAllTransferHistoryForSingleAccountAsync(AccountId Id, CancellationToken cancellationToken = default);

    // create transfer record method 

    public Task CreateTransferRecordAsync(Transfer transfer, CancellationToken cancellationToken = default);
}
