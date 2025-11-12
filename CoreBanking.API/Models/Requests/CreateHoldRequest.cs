namespace CoreBanking.API.Models.Requests
{
    public record CreateHoldRequest
    {
        public Guid AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; }
        public int DurationInDays { get; init; }
    }
}
