using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Queries.GetHolds
{
    public record GetHoldsByAccountQuery : IQuery<List<HoldDto>>
    {
        public required string AccountNumber { get; init; }
    }
}
