using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.BackgroundJobs
{
    // Result classes
    public class StatementGenerationResult
    {
        public int ProcessedAccounts { get; set; }
        public int FailedAccounts { get; set; }
        public bool IsSuccess => FailedAccounts == 0;
        public TimeSpan Duration { get; set; }
    }

    public class AccountStatementResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public AccountId AccountId { get; private set; }

        public static AccountStatementResult Success(AccountId accountId)
            => new AccountStatementResult { IsSuccess = true, AccountId = accountId };

        public static AccountStatementResult Failure(string errorMessage)
            => new AccountStatementResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
