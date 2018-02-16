using System;
using System.Collections.Generic;
using System.Text;
using NWheelsTempApiLib;

namespace ElectricityBilling.Domain.Sensors
{
    public class SensorReadingValueObject
    {
        public SensorReadingValueObject(string sensorId, DateTime utcTimestamp, decimal kwhValue)
        {
            this.Sensor = EntityRef<SensorEntity>.FromId(sensorId);
            this.UtcTimestamp = utcTimestamp;
            this.KwhValue = kwhValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityRef<string, SensorEntity> Sensor { get; }
        public DateTime UtcTimestamp { get; }
        public decimal KwhValue { get; }
    }
}
