using LegacyRenewalApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyRenewalApp.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Customer GetById(int customerId);
    }
}
