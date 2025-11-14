using CoreBanking.Application.Accounts.DTOs;
using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.Queries.GetAccountBalance
{    
    public record GetAccountBalanceQuery : IQuery<AccountBalanceDto>
    {
        public required AccountNumber AccountNumber { get; init; }
    }
}
