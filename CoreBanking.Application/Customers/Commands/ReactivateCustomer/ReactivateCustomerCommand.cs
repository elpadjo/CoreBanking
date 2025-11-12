using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Commands.ReactivateCustomer
{
    public class ReactivateCustomerCommand : ICommand<CustomerId>
    {
        public CustomerId CustomerId { get; set; }
        public string Reason { get; set; }
    }

}