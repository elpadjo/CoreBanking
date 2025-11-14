using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.BackgroundJobs
{
    public class AccountInterestResult
    {
        public bool IsSuccess { get; private set; }
        public string ErrorMessage { get; private set; }
        public decimal InterestAmount { get; private set; }
        public AccountId AccountId { get; private set; }
        public DateTime CalculationDate { get; private set; }

        private AccountInterestResult(decimal interestAmount, AccountId accountId, DateTime calculationDate)
        {
            IsSuccess = true;
            InterestAmount = interestAmount;
            AccountId = accountId;
            CalculationDate = calculationDate;
            ErrorMessage = string.Empty;
        }

        private AccountInterestResult(string errorMessage, AccountId accountId, DateTime calculationDate)
        {
            IsSuccess = false;
            ErrorMessage = errorMessage;
            AccountId = accountId;
            CalculationDate = calculationDate;
            InterestAmount = 0;
        }

        public static AccountInterestResult Success(decimal interestAmount, AccountId accountId, DateTime calculationDate)
        {
            return new AccountInterestResult(interestAmount, accountId, calculationDate);
        }

        public static AccountInterestResult Failure(string errorMessage, AccountId accountId, DateTime calculationDate)
        {
            return new AccountInterestResult(errorMessage, accountId, calculationDate);
        }

        public static AccountInterestResult Success(decimal interestAmount)
        {
            return new AccountInterestResult(interestAmount, AccountId.Create(Guid.Empty), DateTime.UtcNow);
        }

        public static AccountInterestResult Failure(string errorMessage)
        {
            return new AccountInterestResult(errorMessage, AccountId.Create(Guid.Empty), DateTime.UtcNow);
        }
    }
}
