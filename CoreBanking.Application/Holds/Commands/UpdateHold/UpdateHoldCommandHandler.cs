using AutoMapper;
using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Entities;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Holds.Commands.UpdateHold
{
    public class UpdateHoldCommandHandler : IRequestHandler<UpdateHoldCommand, Result<Guid>>
    {
        private readonly IHoldRepository _holdRepository;
        private readonly IMapper _mapper;

        public UpdateHoldCommandHandler(IHoldRepository holdRepository, IMapper mapper)
        {
            _holdRepository = holdRepository;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(UpdateHoldCommand request, CancellationToken cancellationToken)
        {
            var hold = await _holdRepository.GetByIdAsync(request.HoldId);
            if (hold is null)
                return Result<Guid>.Failure("Hold not found.");

            // Update entity
            var updatedHold = Hold.CreateWithID(
                holdId : hold.Id,
                accountId: hold.AccountId,
                amount: new Money(request.Amount ?? hold.Amount.Amount),
                description: request.Description ?? hold.Description,
                 duration: request.DurationInDays == null ? hold.ExpiresAt - DateTime.UtcNow : DateTime.UtcNow.AddDays((double)request.DurationInDays) - DateTime.UtcNow
                );

            Console.WriteLine(hold);
            Console.WriteLine(updatedHold);


            await _holdRepository.UpdateAsync(updatedHold);
            return Result<Guid>.Success(hold.Id.Value);
        }
    }

}
