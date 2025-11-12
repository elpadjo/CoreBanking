using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Customers.Commands.DeactivateCustomer
{
    public class DeactivateCustomerCommandHandler : IRequestHandler<DeactivateCustomerCommand, Result<CustomerId>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CustomerId>> Handle(DeactivateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<CustomerId>.Failure("Customer not found.");

            try
            {
                customer.Deactivate(request.Reason);
            }
            catch (InvalidOperationException ex)
            {
                return Result<CustomerId>.Failure(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CustomerId>.Success(customer.Id);
        }
    }
}
