using AutoMapper;
using CoreBanking.Application.Customers.Commands.CreateCustomer;

namespace CoreBanking.Application.Common.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CreateCustomerDto, CreateCustomerCommand>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Street))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Country) ? "NG" : src.Country))
                .ForMember(dest => dest.CreditScore, opt => opt.MapFrom(src => src.CreditScore))
                .ForMember(dest => dest.BVN, opt => opt.MapFrom(src => src.BVN));
        }
    }
}
