using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CoreBanking.Application.Holds.Commands.CreateHold
{
    public record CreateHoldCommand : ICommand<Guid>
    {
        public AccountId AccountId { get; init; }
        public decimal Amount { get; init; }
        public string Description { get; init; }
        public int DurationInDays { get; init; }
    }
}
