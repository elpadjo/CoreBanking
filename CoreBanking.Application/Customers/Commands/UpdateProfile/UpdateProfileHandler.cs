using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Customers.Commands.UpdateProfile
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result<CustomerId>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CustomerId>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<CustomerId>.Failure("Customer not found.");

            if (!customer.IsActive)
                return Result<CustomerId>.Failure("Cannot update inactive customer.");

            var newContactInfo = new ContactInfo(
                request.Email,
                request.PhoneNumber,
                new Address(
                    request.Street,
                    request.City,
                    request.State,
                    request.ZipCode,
                    request.Country
                ));

            customer.UpdateProfile(newContactInfo);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CustomerId>.Success(customer.Id);
        }
    }
}
