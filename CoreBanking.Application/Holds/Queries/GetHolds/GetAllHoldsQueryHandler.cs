using CoreBanking.Application.Common.Models;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Queries.GetHolds
{
    public class GetAllHoldsQueryHandler : IRequestHandler<GetAllHoldsQuery, Result<PaginatedResult<HoldDto>>>
    {
        private readonly IHoldRepository _holdRepository;

        public GetAllHoldsQueryHandler(IHoldRepository holdRepository)
        {
            _holdRepository = holdRepository;
        }

        public async Task<Result<PaginatedResult<HoldDto>>> Handle(GetAllHoldsQuery request, CancellationToken cancellationToken)
        {
            var holds = await _holdRepository.GetAllHoldsAsync(request.PageNumber, request.PageSize);
            var dtoList = holds.Select(h => new HoldDto
            {
                Id = h.Id,
                AccountId = h.AccountId,
                Amount =  h.Amount.Amount,
                Currency = h.Amount.Currency,
                Reason = h.Description
            }).ToList();
            var paginatedResult = new PaginatedResult<HoldDto>
            {
                Items = dtoList,
                TotalCount = dtoList.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<PaginatedResult<HoldDto>>.Success(paginatedResult);
        }
    }
}
