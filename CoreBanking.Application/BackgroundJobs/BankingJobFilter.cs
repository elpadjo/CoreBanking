using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;

namespace CoreBanking.Application.BackgroundJobs
{
    public class BankingJobFilter : JobFilterAttribute, IElectStateFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public BankingJobFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                using var scope = _serviceProvider.CreateScope();
                var failedJobHandler = scope.ServiceProvider.GetRequiredService<IFailedJobHandler>();

                var jobType = context.BackgroundJob.Job.Type.Name;
                var jobId = context.BackgroundJob.Id;
                var exception = failedState.Exception;

                // Route to appropriate handler based on job type
                if (jobType.Contains("Statement"))
                {
                    Task.Run(() => failedJobHandler.HandleFailedStatementJobAsync(jobId, exception));
                }
                else if (jobType.Contains("Interest"))
                {
                    Task.Run(() => failedJobHandler.HandleFailedInterestJobAsync(jobId, exception));
                }
            }
        }
    }
}
