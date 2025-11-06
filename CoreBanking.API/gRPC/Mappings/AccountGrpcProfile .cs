using AutoMapper;
using CoreBanking.API.gRPC; // gRPC-generated types
using CoreBanking.Application.Accounts.Queries.GetAccountDetails;
using CoreBanking.Application.Accounts.Queries.GetTransactionHistory;
using Google.Protobuf.WellKnownTypes;

namespace CoreBanking.API.gRPC.Mappings
{
    public class AccountGrpcProfile : Profile
    {
        public AccountGrpcProfile()
        {
            CreateMap<AccountDetailsDto, AccountResponse>()
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType.ToString()))
                //.ForMember(dest => dest.CurrentBalance, opt => opt.MapFrom(src => src.CurrentBalance)) 
                //.ForMember(dest => dest.AvailableBalance, opt => opt.MapFrom(src => src.AvailableBalance)) 
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "USD"))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
                .ForMember(dest => dest.DateOpened, opt => opt.MapFrom(src =>
                    Timestamp.FromDateTime(DateTime.SpecifyKind(src.DateOpened, DateTimeKind.Utc))));
                //.ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.AccountStatus.ToString()))


            CreateMap<TransactionDto, TransactionResponse>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.TransactionId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src =>
                    Timestamp.FromDateTime(DateTime.SpecifyKind(src.Timestamp, DateTimeKind.Utc))))
                .ForMember(dest => dest.RunningBalance, opt => opt.MapFrom(src => src.RunningBalance));
        }
    }
}
