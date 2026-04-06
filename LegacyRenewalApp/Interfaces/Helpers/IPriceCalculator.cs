using LegacyRenewalApp.Enums;
using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Helpers
{
    public interface IPriceCalculator
    {
        PaymentDetails CalculateFinalAmount(
            Customer customer, SubscriptionPlan plan, 
            int seatCount,
            bool includePremiumSupport, bool useLoyaltyPoints, 
            PaymentMethod paymentMethod);
    }
}
