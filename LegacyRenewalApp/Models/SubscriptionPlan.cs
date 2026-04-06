using LegacyRenewalApp.Enums;

namespace LegacyRenewalApp.Models
{
    public class SubscriptionPlan
    {
        public PlanCode Code { get; set; } = PlanCode.None;
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyPricePerSeat { get; set; }
        public decimal SetupFee { get; set; }
        public bool IsEducationEligible { get; set; }
    }
}
