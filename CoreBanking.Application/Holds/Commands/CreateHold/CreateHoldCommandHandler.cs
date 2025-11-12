using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Commands.CreateHold
{
    public class CreateHoldCommandHandler : IRequestHandler<CreateHoldCommand, Result<Guid>>
    {

        private readonly IHoldRepository _holdRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateHoldCommandHandler(
            IHoldRepository holdRepository,
            IUnitOfWork unitOfWork)
        {
            _holdRepository = holdRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateHoldCommand request, CancellationToken cancellationToken)
        {
            var hold = Hold.Create(
                accountId: request.AccountId,
                amount: new Money(request.Amount),
                
                description: request.Description,
                duration: DateTime.UtcNow.AddDays(request.DurationInDays).TimeOfDay
            );

            await _holdRepository.AddAsync(hold);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(hold.Id.Value);
        }
    }
}
