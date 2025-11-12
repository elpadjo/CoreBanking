using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using MediatR;

namespace CoreBanking.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IQuery<PaginatedResult<CustomerDto>>,  IRequest<PaginatedResult<CustomerDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }