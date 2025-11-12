using AutoMapper;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Holds.Commands.CreateHold;

namespace CoreBanking.API.Mappings
{
    public class HoldMappingProfile : Profile
    {
        public HoldMappingProfile()
        {
            CreateMap<CreateHoldRequest, CreateHoldCommand>();
        }
    }
}
