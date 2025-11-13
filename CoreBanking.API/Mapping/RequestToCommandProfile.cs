using AutoMapper;
using CoreBanking.API.Models.Requests;
using CoreBanking.Application.Accounts.Commands.CreateAccount;
using CoreBanking.Application.Accounts.Commands.CreateTransactions;
using CoreBanking.Application.Customers.Commands.CreateCustomer;

namespace CoreBanking.API.Mapping
{
    public class RequestToCommandProfile : Profile
    {
        public RequestToCommandProfile()
        {
            // Map API Request DTOs to Application Commands

            // CreateTransactionRequest -> CreateTransanctionCommand
            CreateMap<CreateTransactionRequest, CreateTransactionCommand>()
                .ForMember(dest => dest.TrxAmount,
                    opt => opt.MapFrom(src => src.TrxAmount))
                .ForMember(dest => dest.Currency,
                    opt => opt.MapFrom(src => src.Currency))
                //.ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Reference))
                //.ForMember(dest => dest.RunningBalance, opt => opt.MapFrom(src => src.RunningBalance))
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.TransactionType))
                .ForMember(dest => dest.AccountNumber, opt => opt.MapFrom(src => src.AccountNumber))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
                

            // CreateCustomerRequest -> CreateCustomerCommand
            // All properties match exactly (string to string, DateTime to DateTime)
            // AutoMapper can map automatically, but explicit mapping is better practice
        //    CreateMap<CreateTransactionRequest, CreateTransactionCommand>();
        }
    }
}

