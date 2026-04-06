using LegacyRenewalApp.Interfaces.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Strategies
{
    public class StartSupportFee : ISupportFeeStrategy
    {
        public decimal CalculateFee() => 250m;
    }
}
