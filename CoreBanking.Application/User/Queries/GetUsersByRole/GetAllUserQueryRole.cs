using CoreBanking.Application.Common.Interfaces;
using CoreBanking.Application.User.Queries.Common;
using CoreBanking.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.User.Queries.GetUsersByRole
{
    public class GetAllUserQueryRole : IQuery<PaginatedResult<UserDto>>
    {
        public required string Role { get; init; }
        public int pageNumber { get; init; } = 1;
        public int pageSize { get; init; } = 10;
    }
}
