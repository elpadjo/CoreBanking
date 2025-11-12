using CoreBanking.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Holds.Queries.GetHolds
{
        public class HoldDto
        {
            public HoldId Id { get; set; }
            public AccountId AccountId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string? Reason { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExpiryDate { get; set; }
        }
    }

