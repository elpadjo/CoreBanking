using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Commands.DeleteHold
{
    public class DeleteHoldCommandHandler : IRequestHandler<DeleteHoldCommand, Result<string>>
    {
        private readonly IHoldRepository _holdRepository;

        public DeleteHoldCommandHandler(IHoldRepository holdRepository)
        {
            _holdRepository = holdRepository;
        }

        public async Task<Result<string>> Handle(DeleteHoldCommand request, CancellationToken cancellationToken)
        {
            var hold = await _holdRepository.GetByIdAsync(HoldId.Create(request.HoldId));
            if (hold == null)
                return Result<string>.Failure("Hold not found.");

            await _holdRepository.RemoveAsync(hold.AccountId, hold.Id);

            return Result<string>.Success("Hold deleted successfully.");
        }
    }

}
