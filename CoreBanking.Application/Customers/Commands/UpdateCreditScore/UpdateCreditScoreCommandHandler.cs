using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Customers.Commands.UpdateCreditScore
{
    public class UpdateCreditScoreCommandHandler : IRequestHandler<UpdateCreditScoreCommand, Result<CustomerId>>
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCreditScoreCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CustomerId>> Handle(UpdateCreditScoreCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<CustomerId>.Failure("Customer not found.");

            try
            {
                customer.UpdateCreditScore(request.NewCreditScore, request.Reason);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                return Result<CustomerId>.Failure(ex.Message);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CustomerId>.Success(customer.Id);
        }
    }
}