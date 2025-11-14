namespace CoreBanking.Application.Accounts.DTOs
{
    public class UpdateAccountPreferencesRequest
    {
        public bool EnableTransactionAlerts { get; set; } = true;
        public bool EnableLowBalanceAlerts { get; set; } = true;
        public decimal? LowBalanceThreshold { get; set; }
        public string MonthlyStatementPreference { get; set; } = "Email"; // Email, Paper, Both
    }
}
