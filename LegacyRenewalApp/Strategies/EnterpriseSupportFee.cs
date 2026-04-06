using LegacyRenewalApp.Interfaces.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Strategies
{
    public class EnterpriseSupportFee : ISupportFeeStrategy
    {
        public decimal CalculateFee() => 700m;
    }
}
