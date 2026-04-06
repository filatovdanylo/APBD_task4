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
        private readonly IPriceCalculator _priceCalculator;
        private readonly IInvoiceGenerator _invoiceGenerator;
        private readonly IBillingGateway _billingGateway;

        public SubscriptionRenewalService()
            : this(new CustomerRepository(), 
                  new SubscriptionPlanRepository(), 
                  new RenewalServiceValidator(), 
                  new PriceCalculator(),
                  new InvoiceGenerator(),
                  new BillingGatewayAdapter())
        {}

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository, 
            ISubscriptionPlanRepository planRepository,
            IRenewalServiceValidator validator,
            IPriceCalculator priceCalculator,
            IInvoiceGenerator invoiceGenerator,
            IBillingGateway billingGateway)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _validator = validator;
            _priceCalculator = priceCalculator;
            _invoiceGenerator = invoiceGenerator;
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


            PaymentDetails paymentDetails = _priceCalculator.CalculateFinalAmount(
                customer, plan, 
                seatCount, 
                includePremiumSupport, useLoyaltyPoints, 
                payment
            );

            var invoice = _invoiceGenerator.GenerateInvoice(paymentDetails, customerId, normalizedPlanCode, customer, payment, seatCount);

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
