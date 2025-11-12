using CoreBanking.Application.Common.Interfaces;

namespace CoreBanking.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IQuery<List<CustomerDto>>;