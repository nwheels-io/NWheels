using System;
using System.Collections.Generic;
using System.Text;
using NWheels;
using NWheels.Ddd;

namespace ElectricityBilling.Domain.Sensors
{
    public struct SensorReadingValueObject
    {
        [NWheels.DB.MemberContract.ManyToOne]
        private readonly SensorEntity.Ref _sensor;

        [MemberContract.Semantics.Utc]
        private readonly DateTime _utcTimestamp;

        [MemberContract.Validation.NonNegative]
        private readonly decimal _kwhValue;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SensorReadingValueObject(string sensorId, DateTime utcTimestamp, decimal kwhValue)
        {
            _sensor = new SensorEntity.Ref(sensorId);
            _utcTimestamp = utcTimestamp;
            _kwhValue = kwhValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public SensorEntity.Ref Sensor => _sensor;
        public DateTime UtcTimestamp => _utcTimestamp;
        public decimal KwhValue => _kwhValue;
    }
}
