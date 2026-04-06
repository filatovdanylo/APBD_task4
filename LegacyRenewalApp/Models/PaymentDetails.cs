using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Models
{
    public class PaymentDetails
    {
        public decimal FinalAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SupportFee { get; set; }
        public decimal PaymentFee { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; }

        public PaymentDetails(
            decimal finalAmount, decimal baseAmount, decimal discountAmount, 
            decimal supportFee, decimal paymentFee, decimal taxAmount, 
            string notes)
        {
            FinalAmount = finalAmount;
            BaseAmount = baseAmount;
            DiscountAmount = discountAmount;
            SupportFee = supportFee;
            PaymentFee = paymentFee;
            TaxAmount = taxAmount;
            Notes = notes;
        }

        public PaymentDetails() { }
            
    }
}
