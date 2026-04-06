using LegacyRenewalApp.Interfaces.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Strategies
{
    public class ProSupportFee : ISupportFeeStrategy
    {
        public decimal CalculateFee() => 400m;
    }
}
