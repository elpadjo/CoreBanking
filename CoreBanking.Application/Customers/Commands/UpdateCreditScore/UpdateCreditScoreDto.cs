namespace CoreBanking.Application.Customers.Commands.UpdateCreditScore
{
    public class UpdateCreditScoreDto
    {
        public int NewCreditScore { get; set; }
        public string Reason { get; set; }
    }

}
