using LegacyRenewalApp.Enums;
using LegacyRenewalApp.Interfaces.Helpers;
using LegacyRenewalApp.Interfaces.Strategies;
using LegacyRenewalApp.Models;
using LegacyRenewalApp.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Helper
{
    public class DiscountCalculator : IDiscountCalculator
    {

        private static readonly Dictionary<PlanCode, ISupportFeeStrategy> _feeStrategies = 
            new Dictionary<PlanCode, ISupportFeeStrategy>
        {
            [PlanCode.START] = new StartSupportFee(),
            [PlanCode.PRO] = new ProSupportFee(),
            [PlanCode.ENTERPRISE] = new EnterpriseSupportFee()
        };

        private static readonly Dictionary<PaymentMethod, (decimal Rate, string Note)> _paymentConfigs =
            new Dictionary<PaymentMethod, (decimal Rate, string Note)>
        {
            [PaymentMethod.Card] = (0.02m, "card payment fee; "),
            [PaymentMethod.BankTransfer] = (0.01m, "bank transfer fee; "),
            [PaymentMethod.Paypal] = (0.035m, "paypal fee; "),
            [PaymentMethod.Invoice] = (0m, "invoice payment; ")
        };

        private static readonly Dictionary<Country, decimal> _countryTaxRates =
            new Dictionary<Country, decimal>
        {
                [Country.Poland] = (0.23m),
                [Country.Germany] = (0.19m),
                [Country.CzechRepublic] = (0.21m),
                [Country.Norway] = (0.25m)
        };

        private static readonly Dictionary<CustomerSegment, IDiscountStrategy> _segmentDiscounts =
            new Dictionary<CustomerSegment, IDiscountStrategy>
        {
                [CustomerSegment.Silver] = new SilverDiscountStrategy(),
                [CustomerSegment.Gold] = new GoldDiscountStrategy(),
                [CustomerSegment.Platinum] = new PlatinumDiscountStrategy(),
                [CustomerSegment.Education] = new EducationDiscountStrategy() 
        };

        public decimal calculateDiscount(
            Customer customer, SubscriptionPlan plan, 
            int seatCount,
            bool includePremiumSupport, bool useLoyaltyPoints, 
            PaymentMethod paymentMethod)
        {
            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = 0m;
            string notes = string.Empty;

            if (_segmentDiscounts.TryGetValue(customer.Segment, out var discountStrategy))
            {
                discountAmount += discountStrategy.CalculateDiscount(baseAmount, plan);
                notes += discountStrategy.GetNote();
            }

            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes += "long-term loyalty discount; ";
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes += "basic loyalty discount; ";
            }

            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes += "large team discount; ";
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes += "medium team discount; ";
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes += "small team discount; ";
            }

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes += $"loyalty points used: {pointsToUse}; ";
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            decimal supportFee = 0m;
            PlanCode planCode = plan.Code;
            if (includePremiumSupport && 
                _feeStrategies.TryGetValue(planCode, out var feeStrategy))
            {
                supportFee = feeStrategy.CalculateFee();
                notes += "premium support included; ";
            }


            decimal paymentFee = 0m;
            if (!_paymentConfigs.TryGetValue(paymentMethod, out var config))
            {
                throw new ArgumentException("Unsupported payment method");
            }

            paymentFee = (subtotalAfterDiscount + supportFee) * config.Rate;

            notes += config.Note;


            decimal taxRate = _countryTaxRates.TryGetValue(customer.Country, out var rate) 
                ? rate : 0.20m;

            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            return finalAmount;
        }
    }
}
