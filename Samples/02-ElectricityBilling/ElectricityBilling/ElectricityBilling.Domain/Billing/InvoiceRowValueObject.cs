using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Billing
{
    public struct InvoiceRowValueObject
    {
        private readonly PriceValueObject _price;

        [MemberContract.Required]
        [MemberContract.Validation.MaxLength(200)]
        private readonly string _description;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InvoiceRowValueObject(PriceValueObject price, string description)
        {
            _price = price;
            _description = description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PriceValueObject Price => _price;
        public string Description => _description;
    }
}
