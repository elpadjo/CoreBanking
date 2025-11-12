using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Commands.DeactivateCustomer
{
    public class DeactivateCustomerCommand : ICommand<CustomerId>
    {
        public CustomerId CustomerId { get; set; }
        public string Reason { get; set; }
    }
}
