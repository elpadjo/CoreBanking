using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.BackgroundJobs
{
    public interface IInterestCalculationService
    {
        Task CalculateMonthlyInterestAsync(DateTime calculationDate, CancellationToken cancellationToken = default);
        Task<InterestCalculationResult> CalculateAccountInterestAsync(AccountId accountId, DateTime calculationDate, CancellationToken cancellationToken = default);
    }
}
