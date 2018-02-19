using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Customers;
using NWheels.Ddd;

namespace ElectricityBilling.Domain.Accounts
{
    public class CustomerUserAccountEntity : UserAccountEntity
    {
        public CustomerUserAccountEntity(CustomerEntity customer)
        {
            //??this.Customer = customer;
        }

        public EntityRef<long, CustomerEntity> Customer { get; }
    }
}
