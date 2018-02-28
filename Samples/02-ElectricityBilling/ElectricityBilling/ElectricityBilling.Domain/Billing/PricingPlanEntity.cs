﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectricityBilling.Domain.Basics;
using ElectricityBilling.Domain.Customers;
using ElectricityBilling.Domain.Sensors;
using NWheels;
using NWheels.DB;
using NWheels.Ddd;
using MemberContract = NWheels.MemberContract;

namespace ElectricityBilling.Domain.Billing
{
    public abstract class PricingPlanEntity
    {
        [MemberContract.InjectedDependency]
        private IThisDomainObjectServices _thisObject;

        [MemberContract.InjectedDependency]
        private IBaseLocalizables _localizables;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.AutoGenerated]
        private readonly long _id;

        [MemberContract.Required]
        private string _description;

        private bool _isReadOnly;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PricingPlanEntity(Injector<IBaseLocalizables> injector)
        {
            injector.Inject(out _localizables);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PricingPlanEntity(Injector<IBaseLocalizables> injector, string description, bool isReadOnly = false)
        {
            injector.Inject(this, out _id, out _thisObject, out _localizables);

            _description = description;
            _isReadOnly = isReadOnly;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract Task<PriceValueObject> CalculatePriceAsync(CustomerEntity customer, IAsyncEnumerable<SensorReadingValueObject> orderedReadings);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual async Task ValidateInvariants(IDomainObjectValidator<PricingPlanEntity> report)
        {
            if (!_isReadOnly)
            {
                var isAssigned = await IsAssignedToAnyContract();

                if (isAssigned)
                {
                    report.InvalidValue(x => x.IsReadOnly, _localizables.PricingPlanMustBeReadOnlyBecauseItIsInUse);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<bool> IsAssignedToAnyContract()
        {
            var query = _thisObject.GetContext<ElectricityBillingContext>().QueryCustomerContractsByPricingPlan(this);
            return query.AnyAsync();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long Id => _id;

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => _isReadOnly = value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct Ref
        {
            public readonly long Id;
            #region Generated code
            public Ref(long id) => this.Id = id;
            public static implicit operator Ref(PricingPlanEntity entity) => new Ref(entity.Id);
            public static implicit operator Ref(long id) => new Ref(id);
            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [NWheels.I18n.TypeContract.Localizables]
        public interface IBaseLocalizables
        {
            string PricingPlanMustBeReadOnlyBecauseItIsInUse { get; }
        }
    }
}