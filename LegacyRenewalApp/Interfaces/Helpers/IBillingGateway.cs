using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Helpers
{
    public interface IBillingGateway
    {
        void SaveInvoice(RenewalInvoice invoice);
        void SendEmail(string email, string subject, string body);
    }
}
