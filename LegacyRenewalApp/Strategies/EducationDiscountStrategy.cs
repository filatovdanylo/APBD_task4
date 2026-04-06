using LegacyRenewalApp.Interfaces.Strategies;
using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Strategies
{
    public class EducationDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(decimal baseAmount, SubscriptionPlan plan)
        {
            if (!plan.IsEducationEligible)
                return 0.0m;

            return baseAmount * 0.20m; 
        }

        public string GetNote()
        {
            return "education discount; ";
        }
    }
}
