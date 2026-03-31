using LegacyRenewalApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Helper
{
    public class RenewalServiceValidator : IRenewalServiceValidator
    {
        public void Validate(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            throw new NotImplementedException();
        }
    }
}
