namespace CoreBanking.Application.BackgroundJobs
{
    public interface IFailedJobHandler
    {
        Task HandleFailedStatementJobAsync(string jobId, Exception exception);
        Task HandleFailedInterestJobAsync(string jobId, Exception exception);
        Task<bool> CanRecoverJobAsync(string jobId, Exception exception);
    }
}
