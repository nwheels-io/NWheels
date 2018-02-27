using System;
using System.Collections.Generic;
using System.Text;
using ElectricityBilling.Domain.Sensors;

namespace ElectricityBilling.Domain.Billing
{
    [NWheels.DB.TypeContract.View(over: typeof(ContractEntity))]
    public struct SensorContractView
    {
        private readonly ContractEntity _contract;
        private readonly SensorEntity _sensor;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ContractEntity Contract => _contract;
        public SensorEntity Sensor => _sensor;
    }
}
