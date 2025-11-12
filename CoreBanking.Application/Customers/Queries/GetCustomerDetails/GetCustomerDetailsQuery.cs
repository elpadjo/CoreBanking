using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;

namespace CoreBanking.Application.Customers.Queries.GetCustomerDetails;

public record GetCustomerDetailsQuery : IQuery<CustomerDetailsDto>
{
    public required CustomerId CustomerId { get; init; }
}