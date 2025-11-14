using System;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{

    private readonly BankingDbContext _context;

    public TransferRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task CreateTransferRecordAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        await _context.Transfers.AddAsync(transfer);
        
    }

    public async Task<List<Transfer>> GetAllTransferHistoryForSingleAccountAsync(AccountId Id, CancellationToken cancellationToken = default)
    {
        var result =  _context.Transfers.Where(t =>  t.FromAccountId == Id).ToList();
        return Task.FromResult(result).Result;
    }

    public async Task<Transfer> GetTransferByIdAsync(TransferId Id, CancellationToken cancellationToken = default)
    {
        var result = await _context.Transfers.FirstOrDefaultAsync(t => t.Id == Id, cancellationToken)!;
        return Task.FromResult(result).Result!;
    }
}
