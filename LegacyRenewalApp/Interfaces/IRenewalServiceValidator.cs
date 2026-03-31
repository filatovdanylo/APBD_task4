using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces
{
    public interface IRenewalServiceValidator
    {
        void Validate(int customerId,
            string planCode,
            int seatCount,
            string paymentMethod);
    }
}
