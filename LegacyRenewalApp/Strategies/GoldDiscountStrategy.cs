using LegacyRenewalApp.Interfaces.Strategies;
using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Strategies
{
    internal class GoldDiscountStrategy : IDiscountStrategy
    {
        public decimal CalculateDiscount(decimal baseAmount, SubscriptionPlan plan)
        {
            return baseAmount * 0.10m;
        }

        public string GetNote()
        {
            return "gold discount; ";
        }
    }
}
