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
    public class PriceCalculator : IPriceCalculator
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
                [Country.Unknown] = 0.20m,
                [Country.Poland] = 0.23m,
                [Country.Germany] = 0.19m,
                [Country.CzechRepublic] = 0.21m,
                [Country.Norway] = 0.25m
        };

        private static readonly Dictionary<CustomerSegment, IDiscountStrategy> _segmentDiscounts =
            new Dictionary<CustomerSegment, IDiscountStrategy>
        {
                [CustomerSegment.Silver] = new SilverDiscountStrategy(),
                [CustomerSegment.Gold] = new GoldDiscountStrategy(),
                [CustomerSegment.Platinum] = new PlatinumDiscountStrategy(),
                [CustomerSegment.Education] = new EducationDiscountStrategy() 
        };

        public PaymentDetails CalculateFinalAmount(
            Customer customer, SubscriptionPlan plan, 
            int seatCount,
            bool includePremiumSupport, bool useLoyaltyPoints, 
            PaymentMethod paymentMethod)
        {
            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            PaymentDetails paymentDetails = new PaymentDetails();

            decimal baseAmount = CalculateBaseAmount(plan, seatCount);
            var notes = new StringBuilder();
            paymentDetails.BaseAmount = baseAmount;

            decimal discountAmount = CalculateDiscountAmount(customer, baseAmount, plan, 
                ref notes, useLoyaltyPoints, seatCount);
            paymentDetails.DiscountAmount = discountAmount;

            decimal subtotalAfterDiscount = CalculateSubTotalAfterDiscount(baseAmount, discountAmount, ref notes);

            decimal supportFee = CalculateSupportFee(plan, includePremiumSupport, ref notes);
            paymentDetails.SupportFee = supportFee;

            decimal paymentFee = CalculatePaymentFee(paymentMethod, subtotalAfterDiscount, supportFee, ref notes);
            paymentDetails.PaymentFee = paymentFee;

            decimal taxRate = GetTaxRateByCountry(customer);

            decimal finalAmount = CalculateFinalAmount(subtotalAfterDiscount, 
                supportFee, paymentFee, taxRate, 
                ref notes, ref paymentDetails);

            paymentDetails.FinalAmount = finalAmount;

            return paymentDetails;
        }

        private decimal CalculateBaseAmount(SubscriptionPlan plan, int seatCount)
        {
            return (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
        }

        private decimal CalculateDiscountAmount(Customer customer, decimal baseAmount,
            SubscriptionPlan plan, ref StringBuilder notes, bool useLoyaltyPoints, int seatCount)
        {
            decimal discountAmount = 0.0m;

            if (_segmentDiscounts.TryGetValue(customer.Segment, out var discountStrategy))
            {
                discountAmount += discountStrategy.CalculateDiscount(baseAmount, plan);
                notes.Append(discountStrategy.GetNote());
            }

            if (customer.YearsWithCompany >= 5)
            {
                discountAmount += baseAmount * 0.07m;
                notes.Append("long-term loyalty discount; ");
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discountAmount += baseAmount * 0.03m;
                notes.Append("basic loyalty discount; ");
            }

            if (seatCount >= 50)
            {
                discountAmount += baseAmount * 0.12m;
                notes.Append("large team discount; ");
            }
            else if (seatCount >= 20)
            {
                discountAmount += baseAmount * 0.08m;
                notes.Append("medium team discount; ");
            }
            else if (seatCount >= 10)
            {
                discountAmount += baseAmount * 0.04m;
                notes.Append("small team discount; ");
            }

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                discountAmount += pointsToUse;
                notes.Append($"loyalty points used: {pointsToUse}; ");
            }

            return discountAmount;
        }

        private decimal CalculateSubTotalAfterDiscount(decimal baseAmount, decimal discountAmount,
            ref StringBuilder notes)
        {
            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes.Append("minimum discounted subtotal applied; ");
            }

            return subtotalAfterDiscount;
        }

        private decimal CalculateSupportFee(SubscriptionPlan plan, bool includePremiumSupport, ref StringBuilder notes)
        {
            decimal supportFee = 0m;
            PlanCode planCode = plan.Code;
            if (includePremiumSupport &&
                _feeStrategies.TryGetValue(planCode, out var feeStrategy))
            {
                supportFee = feeStrategy.CalculateFee();
                notes.Append("premium support included; ");
            }

            return supportFee;
        }

        private decimal CalculatePaymentFee(PaymentMethod paymentMethod, 
            decimal subtotalAfterDiscount, decimal supportFee, ref StringBuilder notes)
        {
            decimal paymentFee = 0m;
            if (!_paymentConfigs.TryGetValue(paymentMethod, out var config))
            {
                throw new ArgumentException("Unsupported payment method");
            }

            paymentFee = (subtotalAfterDiscount + supportFee) * config.Rate;

            notes.Append(config.Note);

            return paymentFee;
        }

        private decimal GetTaxRateByCountry(Customer customer)
        {
            return _countryTaxRates.TryGetValue(customer.Country, out var rate)
                ? rate : _countryTaxRates[Country.Unknown];
        }

        private decimal CalculateFinalAmount(decimal subtotalAfterDiscount, decimal supportFee, 
            decimal paymentFee, decimal taxRate, ref StringBuilder notes, ref PaymentDetails paymentDetails)
        {
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            paymentDetails.TaxAmount = taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes.Append("minimum invoice amount applied; ");
            }

            return finalAmount;
        }

    }
}
