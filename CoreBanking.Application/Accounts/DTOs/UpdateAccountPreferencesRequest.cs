using CoreBanking.Core.Enums;
using System.Text.Json.Serialization;

namespace CoreBanking.Application.Accounts.DTOs
{
    public class UpdateAccountPreferencesRequest
    {
        public bool EnableTransactionAlerts { get; set; } = true;
        public bool EnableLowBalanceAlerts { get; set; } = true;
        public decimal? LowBalanceThresholdAmount { get; set; }
        public string? LowBalanceThresholdCurrency { get; set; } = "NGN";

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MonthlyStatementPreferenceType MonthlyStatementPreference { get; set; } = MonthlyStatementPreferenceType.Email;
    }
}
