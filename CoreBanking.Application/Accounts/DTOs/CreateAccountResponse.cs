namespace CoreBanking.Application.Accounts.DTOs
{
    public class CreateAccountResponse
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime DateOpened { get; set; }
    }
}
