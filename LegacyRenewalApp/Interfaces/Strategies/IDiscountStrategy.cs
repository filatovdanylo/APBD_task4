using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Strategies
{
    public interface IDiscountStrategy
    {
        decimal CalculateDiscount(decimal baseAmount, SubscriptionPlan plan);
        string GetNote();
    }
}
