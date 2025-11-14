namespace CoreBanking.Application.BackgroundJobs
{    
    public interface IDailyStatementService
    {
        Task GenerateMonthlyStatementsAsync(DateTime statementDate, CancellationToken cancellationToken = default);
        Task<StatementGenerationResult> GenerateCustomerStatementAsync(Guid customerId, DateTime statementDate, CancellationToken cancellationToken = default);
    }
}
