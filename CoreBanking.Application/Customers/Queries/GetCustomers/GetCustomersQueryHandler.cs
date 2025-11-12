using CoreBanking.Application.Common.Models;
//using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.Interfaces;
using MediatR;

namespace CoreBanking.Application.Customers.Queries.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, Result<PaginatedResult<CustomerDto>>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomersQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Result<PaginatedResult<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {

            var customers = await _customerRepository.GetAllAsync(pageSize: request.PageSize, pageNumber: request.PageNumber);

            var customerDto = customers.Select(customer => new CustomerDto
            {
                //CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.ContactInfo.Email,
                Phone = customer.ContactInfo.PhoneNumber,
                //DateRegistered = customer.DateRegistered,
                IsActive = customer.IsActive
            }).ToList();

            var paginatedResult = new PaginatedResult<CustomerDto>(
             customerDto,
             customerDto.Count,
             request.PageNumber,
             request.PageSize
         );

            return Result<PaginatedResult<CustomerDto>>.Success(paginatedResult);
        }
    }
}