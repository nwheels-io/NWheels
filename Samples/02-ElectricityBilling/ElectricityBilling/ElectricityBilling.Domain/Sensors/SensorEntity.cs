using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NWheels;
using ElectricityBilling.Domain.Basics;
using ElectricityBilling.Domain.Accounts;
using NWheels.Authorization;

namespace ElectricityBilling.Domain.Sensors
{
    [SecurityContract.Require(ElectricityBillingClaim.BackOfficeUser)]
    public class SensorEntity
    {
        // initializes a new entity
        public SensorEntity(string id, bool isActive, ref PostalAddressValueObject location)
        {
            this.Id = id;
            this.IsActive = isActive;
            this.Location = location;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SecurityContract.Require(ElectricityBillingClaim.DeviceAdmin)]
        public async Task Activate()
        {
            await SensorHub.RequestSensorActivationChangeAsync(this.Id, active: true);
            this.IsActive = true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SecurityContract.Require(ElectricityBillingClaim.DeviceAdmin)]
        public async Task Deactivate()
        {
            await SensorHub.RequestSensorActivationChangeAsync(this.Id, active: false);
            this.IsActive = false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Id { get; }
        public PostalAddressValueObject Location { get; set; }
        public bool IsActive { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MemberContract.InjectedDependency]
        protected ISensorHubService SensorHub { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct Ref
        {
            public readonly string Id;
            #region Generated code
            public Ref(string id) => this.Id = id;
            public static implicit operator Ref(SensorEntity entity) => new Ref(entity.Id);
            public static implicit operator Ref(string id) => new Ref(id);
            #endregion
        }
    }
}
