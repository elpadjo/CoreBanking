using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.DTOs
{
    public class WithdrawalCheckDto
    {
        public bool CanWithdraw { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal AvailableBalance { get; set; }
        public int MonthlyWithdrawalsUsed { get; set; }
        public int MonthlyWithdrawalsRemaining { get; set; }
    }
}
