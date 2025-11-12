using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using MediatR;

namespace CoreBanking.Application.Customers.Queries.GetCustomerDetails
{
    public class GetCustomerDetailsQueryHandler : IRequestHandler<GetCustomerDetailsQuery, Result<CustomerDetailsDto>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerDetailsQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Result<CustomerDetailsDto>> Handle(GetCustomerDetailsQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

            if (customer == null)
                return Result<CustomerDetailsDto>.Failure("Account not found");

            var customerDetailsDto = new CustomerDetailsDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.ContactInfo.Email,
                Phone = customer.ContactInfo.PhoneNumber,
                Address = customer.ContactInfo.Address.ToString(),
                //Address = customer.ContactInfo.Address,
                DateOfBirth = customer.DateOfBirth,
                DateRegistered = customer.DateCreated,
                IsActive = customer.IsActive,
            };

            return Result<CustomerDetailsDto>.Success(customerDetailsDto);
        }
    }
}
