using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Customers;
using NWheels;
using NWheels.Authorization;
using NWheels.DB;
using NWheels.Ddd;
using DB = NWheels.DB;

namespace ElectricityBilling.Domain.Billing
{
    [DB.TypeContract.View]
    public class PricingPlanContractView
    {
        [DB.MemberContract.MapToMember(typeof(PricingPlanEntity), nameof(PricingPlanEntity.Id))]
        public long PricingPlanId { get; }

        [DB.MemberContract.MapToMember(typeof(ContractEntity), nameof(ContractEntity.Id))]
        public long ContractId { get; }

        [DB.MemberContract.MapToMember(typeof(CustomerEntity), nameof(CustomerEntity.Id))]
        public long CustomerId { get; }
    }
}
