using AutoMapper;
using CoreBanking.Application.Common.Models;
using CoreBanking.Application.Holds.Queries.GetHolds;
using CoreBanking.Core.Interfaces;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Holds.Queries.GetHoldsByAccount
{
    public class GetHoldsByAccountQueryHandler : IRequestHandler<GetHoldsByAccountQuery, Result<List<HoldDto>>>
    {
        private readonly IHoldRepository _holdRepository;
        private readonly IMapper _mapper;

        public GetHoldsByAccountQueryHandler(IHoldRepository holdRepository, IMapper mapper)
        {
            _holdRepository = holdRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<HoldDto>>> Handle(GetHoldsByAccountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Fetch holds from repository
                var holds = await _holdRepository.GetByAccountNumberAsync(request.AccountNumber);

                if (holds == null || !holds.Any())
                    return Result<List<HoldDto>>.Failure("No holds found for this account.");

                // Map to DTO
                var dtoList = holds.Select(h => new HoldDto
                {
                    Id = h.Id,
                    AccountId = h.AccountId,
                    Amount = h.Amount.Amount,
                    Currency = h.Amount.Currency,
                    Reason = h.Description
                }).ToList();

                return Result<List<HoldDto>>.Success(dtoList);
            }
            catch (Exception ex)
            {
                return Result<List<HoldDto>>.Failure($"An error occurred while retrieving holds: {ex.Message}");
            }
        }
    }
}
