using AutoMapper;
using CoreBanking.Application.Holds.Queries.GetHolds;
using CoreBanking.Core.Entities;

namespace CoreBanking.Application.Common.Mappings
{
    public class HoldProfile : Profile
    {
        public HoldProfile()
        {
            CreateMap<Hold, HoldDto>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value))
              .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId.Value))
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
              .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.PlacedAt));

        }

    }
}
