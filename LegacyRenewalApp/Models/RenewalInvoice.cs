using LegacyRenewalApp.Enums;
using System;

namespace LegacyRenewalApp.Models
{
    public class RenewalInvoice
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PlanCode { get; set; } = string.Empty;

        //I am not really sure if this breaks our contract, since PaymentMethod is never explicitly used as a string in Legacy classes.
        // Assuming that the legacy classes will never change we can use enum here for additional safety and clarity
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.None;
        public int SeatCount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal SupportFee { get; set; }
        public decimal PaymentFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }

        public override string ToString()
        {
            return $"InvoiceNumber={InvoiceNumber}, Customer={CustomerName}, Plan={PlanCode}, Seats={SeatCount}, FinalAmount={FinalAmount:F2}, Notes={Notes}";
        }
    }
}
