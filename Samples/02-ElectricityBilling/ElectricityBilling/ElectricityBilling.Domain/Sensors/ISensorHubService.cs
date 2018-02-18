using System;
using System.Threading.Tasks;

namespace ElectricityBilling.Domain.Sensors
{
    public interface ISensorHubService
    {
        Task RequestSensorActivationChangeAsync(string sensorId, bool active);
        event SensorReadingReceivedHandler SensorReadingReceived;
    }

    public delegate Task SensorReadingReceivedHandler(DateTime timestamp, string sensorId, decimal kwh);
}