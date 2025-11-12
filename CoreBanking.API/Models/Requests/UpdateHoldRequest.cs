namespace CoreBanking.API.Models.Requests
{
    public class UpdateHoldRequest
    {
        public decimal? Amount { get; set; }

        public string? Description { get; set; }

        // Duration in days for the hold (nullable if not changing)
        public int? DurationInDays { get; set; }
    }

}
