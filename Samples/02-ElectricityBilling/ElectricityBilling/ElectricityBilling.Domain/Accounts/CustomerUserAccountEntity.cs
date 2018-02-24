using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Customers;
using NWheels.Ddd;

namespace ElectricityBilling.Domain.Accounts
{
    public class CustomerUserAccountEntity : UserAccountEntity
    {
        [NWheels.DB.MemberContract.ManyToOne]
        private readonly CustomerEntity.Ref _customer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CustomerUserAccountEntity(CustomerEntity customer)
        {
            _customer = customer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CustomerEntity.Ref Customer => _customer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public new struct Ref
        {
            public readonly long Id;

            #region Generated code
            public Ref(long id) => this.Id = id;
            public static implicit operator UserAccountEntity.Ref(Ref derived) => new UserAccountEntity.Ref(derived.Id);
            #endregion
        }
    }
}
