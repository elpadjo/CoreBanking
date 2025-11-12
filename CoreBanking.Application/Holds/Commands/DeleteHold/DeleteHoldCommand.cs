using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreBanking.Application.Common.Models;
using MediatR;

namespace CoreBanking.Application.Holds.Commands.DeleteHold
{
   

    public record DeleteHoldCommand(Guid HoldId) : IRequest<Result<string>>;

}
