using CoreBanking.Application.Common.Models;
using CoreBanking.Core.ValueObjects;
using MediatR;

namespace CoreBanking.Application.Accounts.Commands.CloseMyAccount
{
    public class CloseMyAccountCommand : IRequest<Result>
    {
        public AccountNumber AccountNumber { get; set; }
        //public CustomerId CustomerId { get; set; }
        public string Reason { get; set; } = "Customer request";
    }
}