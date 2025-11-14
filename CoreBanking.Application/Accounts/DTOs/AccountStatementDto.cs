using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Accounts.DTOs
{
    public class AccountStatementDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        //public List<StatementTransactionDto> Transactions { get; set; } = new();
        //public StatementSummaryDto Summary { get; set; } = new();
    }
}
