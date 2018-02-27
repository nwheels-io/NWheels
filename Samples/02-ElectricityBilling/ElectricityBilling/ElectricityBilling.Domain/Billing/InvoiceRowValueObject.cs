using System;
using System.Collections.Generic;
using System.Text;
using NWheels;

namespace ElectricityBilling.Domain.Billing
{
    public struct InvoiceRowValueObject
    {
        [NWheels.DB.MemberContract.ManyToOne]
        private readonly ContractEntity.Ref _contract;

        private readonly PriceValueObject _price;

        [MemberContract.Required]
        [MemberContract.Validation.MaxLength(200)]
        private readonly string _description;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InvoiceRowValueObject(ContractEntity.Ref contract, PriceValueObject price, string description)
        {
            _contract = contract;
            _price = price;
            _description = description;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContractEntity.Ref Contract => _contract;
        public PriceValueObject Price => _price;
        public string Description => _description;
    }
}
