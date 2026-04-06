using LegacyRenewalApp.Enums;
using LegacyRenewalApp.Helper;
using LegacyRenewalApp.Interfaces.Helpers;
using LegacyRenewalApp.Interfaces.Repositories;
using LegacyRenewalApp.Models;
using LegacyRenewalApp.Repositories;
using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {

        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IRenewalServiceValidator _validator;
        private readonly IDiscountCalculator _discountCalculator;
        private readonly IBillingGateway _billingGateway;

        public SubscriptionRenewalService()
            : this(new CustomerRepository(), 
                  new SubscriptionPlanRepository(), 
                  new RenewalServiceValidator(), 
                  new DiscountCalculator(),
                  new BillingGatewayAdapter())
        {}

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository, 
            ISubscriptionPlanRepository planRepository,
            IRenewalServiceValidator validator,
            IDiscountCalculator discountCalculator,
            IBillingGateway billingGateway)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _validator = validator;
            _discountCalculator = discountCalculator;
            _billingGateway = billingGateway;

        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            _validator.Validate(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            PaymentMethod payment;

            if (Enum.TryParse(normalizedPaymentMethod, out PaymentMethod result))
            {
                payment = result;
            } else
            {
                throw new ArgumentException("Unsupported payment type");
            }


            PaymentDetails paymentDetails = _discountCalculator.CalculateFinalAmount(
                customer, plan, 
                seatCount, 
                includePremiumSupport, useLoyaltyPoints, 
                payment
            );

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = payment,
                SeatCount = seatCount,
                BaseAmount = Math.Round(paymentDetails.BaseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(paymentDetails.DiscountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(paymentDetails.SupportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentDetails.PaymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(paymentDetails.TaxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(paymentDetails.FinalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = paymentDetails.Notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
