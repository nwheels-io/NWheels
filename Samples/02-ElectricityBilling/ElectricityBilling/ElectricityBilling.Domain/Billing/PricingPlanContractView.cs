using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Customers;
using NWheels;
using NWheels.Authorization;
using NWheels.DB;
using NWheels.Ddd;

namespace ElectricityBilling.Domain.Billing
{
    [NWheels.DB.TypeContract.View(over: typeof(PricingPlanEntity))]
    public class PricingPlanContractView
    {
        [NWheels.DB.MemberContract.MapToMember(typeof(PricingPlanEntity), nameof(PricingPlanEntity.Id))]
        private readonly PricingPlanEntity.Ref _pricingPlan;

        [NWheels.DB.MemberContract.MapToMember(typeof(ContractEntity), nameof(ContractEntity.Id))]
        private readonly ContractEntity.Ref _contract;

        [NWheels.DB.MemberContract.MapToMember(typeof(CustomerEntity), nameof(CustomerEntity.Id))]
        private readonly CustomerEntity.Ref _customer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PricingPlanEntity.Ref PricingPlan => _pricingPlan;
        public ContractEntity.Ref Contract => _contract;
        public CustomerEntity.Ref Customer => _customer;
    }
}
