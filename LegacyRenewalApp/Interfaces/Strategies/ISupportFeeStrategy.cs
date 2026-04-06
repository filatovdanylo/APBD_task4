using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Strategies
{
    public interface ISupportFeeStrategy
    {
        decimal CalculateFee();
    }
}
