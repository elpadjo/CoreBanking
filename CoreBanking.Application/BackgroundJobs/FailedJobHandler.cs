using CoreBanking.Core.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Application.BackgroundJobs
{
    public class FailedJobHandler : IFailedJobHandler
    {
        private readonly ILogger<FailedJobHandler> _logger;
        //private readonly IEmailService _emailService;
        private readonly IHangfireService _hangfireService;

        public FailedJobHandler(
            ILogger<FailedJobHandler> logger,
            //IEmailService emailService,
            IHangfireService hangfireService)
        {
            _logger = logger;
            //_emailService = emailService;
            _hangfireService = hangfireService;
        }

        public async Task HandleFailedStatementJobAsync(string jobId, Exception exception)
        {
            _logger.LogError(exception, "Statement generation job {JobId} failed", jobId);

            // Send alert to operations team
            /*await _emailService.SendJobFailureAlertAsync(
                "Statement Generation Failure",
                $"Job {jobId} failed: {exception.Message}",
                exception.ToString());*/

            // Check if this is a recoverable failure
            if (await CanRecoverJobAsync(jobId, exception))
            {
                _logger.LogInformation("Attempting to recover statement job {JobId}", jobId);
                await _hangfireService.TriggerJobAsync<DailyStatementService>(
                    x => x.GenerateDailyStatementsAsync(DateTime.UtcNow.Date.AddDays(-1), CancellationToken.None));
            }
        }

        public async Task HandleFailedInterestJobAsync(string jobId, Exception exception)
        {
            _logger.LogError(exception, "Interest calculation job {JobId} failed", jobId);

            // This is critical - interest calculation failures need immediate attention
            /*await _emailService.SendCriticalAlertAsync(
                "CRITICAL: Interest Calculation Failure",
                $"Job {jobId} failed: {exception.Message}",
                exception.ToString());*/

            // For interest jobs, always attempt recovery due to financial impact
            _logger.LogWarning("Attempting immediate recovery of interest calculation job {JobId}", jobId);
            await _hangfireService.TriggerJobAsync<InterestCalculationService>(
                x => x.CalculateMonthlyInterestAsync(DateTime.UtcNow.Date, CancellationToken.None));
        }

        public async Task<bool> CanRecoverJobAsync(string jobId, Exception exception)
        {
            // Determine if the job failure is recoverable based on exception type
            return exception switch
            {
                TimeoutException => true,
                HttpRequestException => true, // Network issues
                SqlException sqlEx when sqlEx.Number == -2 => true, // Timeout
                SqlException sqlEx when sqlEx.Number == 1205 => true, // Deadlock
                _ => false // Don't recover for other exceptions
            };
        }
    }
}
