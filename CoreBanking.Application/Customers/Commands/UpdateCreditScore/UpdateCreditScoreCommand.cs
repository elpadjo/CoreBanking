using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Commands.UpdateCreditScore
{
    public class UpdateCreditScoreCommand : ICommand<CustomerId>
    {
        public CustomerId CustomerId { get; set; }
        public int NewCreditScore { get; set; }
        public string Reason { get; set; } = "System update";
    }
}
