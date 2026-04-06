using LegacyRenewalApp.Enums;
using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Helpers
{
    public interface IInvoiceGenerator
    {
        RenewalInvoice GenerateInvoice(PaymentDetails paymentDetails, int customerId, string normalizedPlanCode,
            Customer customer, PaymentMethod payment, int seatCount);
    }
}
