using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Commands.UpdateHold
{
    public record UpdateHoldCommand : ICommand<Guid>
    {

        public HoldId HoldId{ get; init; }
        public decimal? Amount { get; init; }
        public string? Description { get; init; }
        public int? DurationInDays { get; init; }
    }
}
