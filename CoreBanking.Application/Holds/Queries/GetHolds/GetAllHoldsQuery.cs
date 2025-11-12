using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Queries.GetHolds
{
    public record GetAllHoldsQuery(int PageNumber , int PageSize )
    : IRequest<Result<PaginatedResult<HoldDto>>>;
}
