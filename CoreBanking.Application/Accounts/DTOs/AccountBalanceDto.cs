using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.DTOs
{
    public class AccountBalanceDto
    {
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime LastUpdated { get; set; }
    }
}
